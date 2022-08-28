using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    public GameObject trainPrefab;
    public GameObject carriagePrefab;
    public TileMoving tileMoving;
    public Tiles tiles;
    public PowerupSystem powerupSystem;
    public UIManager uiManager;

    private Difficulty.DifficultyProperties difficultyProperties;

    public float trainCurrentSpeed = 0;
    public float trainHeight = 1.15f;
    public static int trainStartHex = 3;
    public float carriageDistance = 3.77f;
    public float verticalRotationSpeed = 30f;
    private readonly float arcDistance = 11.5f;
    public float trainStationScore = 100;
    public bool trainIsMoving = false;
    public float distanceToGetPowerup = 2f;
    [Range(0.0f, 7.0f)]
    public float stationStopX = 5.9f;
    public float trainCrashForce = 10f;
    public float maxCrashForceAngle = 45f;
    public float trainSoundStartDelay = 2f;

    private GameObject train;
    private GameObject[] carriages;
    public GameObject GetTrain() { return train; }

    public Tile previousTile;
    public Tile currentTile;
    public Tile nextTile;
    private int currentEntryEdge;
    private int currentExitEdge;
    private int currentPointIndex;
    private Vector3 nextPoint;
    private bool isCurve;
    private float arcDistancePassed;
    private Vector3[] tilePoints;
    private Vector3[] edgePositions;
    private Vector3[] internalEdgePositions;
    private int currentHorizontalRotation;
    private int targetHorizontalRotation;
    private float targetVerticalRotation;
    private bool willStationStop;
    private int currentSegmentId;
    private bool tileHasPowerup;

    private float maxX;
    private float minX = 43;
    private float startX;
    private float currentXScore;
    private float currentExtraScore;
    private float segmentStartX = 0;

    private int speedMultiplier = 1;

    // how many tiles before level 2 instruction2 will be displayed
    private bool instruction2Displayed = false;
    private bool instruction3Displayed = false;
    public int instructionTilesAhead = 4;

    private SoundSystem soundSystem;
    private GameState gameState;

    public void InitTrain()
    {
        soundSystem = GameObject.Find("SoundSystem").GetComponent<SoundSystem>();
        gameState = GameObject.Find("GameState").GetComponent<GameState>();

        // read difficulty sttings
        difficultyProperties = Difficulty.difficultyMapping[gameState.difficultySetting];

        // spawn train prefab on the start tile
        Tile startTile = Tiles.tiles[trainStartHex, MapGenerator.oceanWidth + (MapGenerator.mapWidth - 1) / 2];
        Vector3 startPosition = startTile.coordinates + new Vector3(-TileGenerator.h,startTile.elevation * TileGenerator.elevationHeight + trainHeight, 0);
        train = Instantiate(trainPrefab, startPosition, Quaternion.identity, transform);

        // positions of the middle point of each edge of a tile
        edgePositions = new Vector3[]
        {
            new Vector3(0.5f * TileGenerator.h,0,0.75f * TileGenerator.a),
            new Vector3(TileGenerator.h,0,0),
            new Vector3(0.5f * TileGenerator.h,0,-0.75f * TileGenerator.a),
            new Vector3(-0.5f * TileGenerator.h,0,-0.75f * TileGenerator.a),
            new Vector3(-TileGenerator.h,0,0),
            new Vector3(-0.5f * TileGenerator.h,0,0.75f * TileGenerator.a)
        };

        // positions of the middle point of each edge of a tile after taking into account the part assigned to elevation and rivers
        internalEdgePositions = new Vector3[]
        {
            edgePositions[0] + ((edgePositions[3] - edgePositions[0]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[1] + ((edgePositions[4] - edgePositions[1]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[2] + ((edgePositions[5] - edgePositions[2]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[3] + ((edgePositions[0] - edgePositions[3]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[4] + ((edgePositions[1] - edgePositions[4]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[5] + ((edgePositions[2] - edgePositions[5]) * TileGenerator.elevationBorderSize)/2,
        };

        // set up all values required by the controller
        previousTile = startTile.neighbors[4];
        currentTile = startTile;
        nextTile = startTile.neighbors[1];
        currentEntryEdge = 4;
        currentExitEdge = 1;
        currentPointIndex = 0;
        (tilePoints, isCurve) = GetTilePoints();
        trainCurrentSpeed = difficultyProperties.trainStartSpeed;
        nextPoint = tilePoints[1];
        arcDistancePassed = 0;
        currentHorizontalRotation = 0;
        targetHorizontalRotation = 0;
        targetVerticalRotation = 0;
        trainIsMoving = true;
        willStationStop = false;
        currentSegmentId = 0;
        tileHasPowerup = false;

        startX = startPosition.x;
        maxX = startPosition.x;
        currentXScore = 0;

        // spawn carriages at the postion of the train
        carriages = new GameObject[difficultyProperties.carriageNumber];
        for (int i = 0; i < difficultyProperties.carriageNumber; i++)
        {
            carriages[i] = Instantiate(carriagePrefab, startPosition, Quaternion.identity, transform);
            carriages[i].GetComponent<CarriageController>().InitCarriage(i == 0 ? train : carriages[i - 1], this);
        }

        

        StartCoroutine(StartTrainSound(trainSoundStartDelay));
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && speedMultiplier < 8 && gameState.state == GameState.State.GameActive) speedMultiplier *= 2;
        if (Input.GetKeyDown(KeyCode.S) && speedMultiplier > 1 && gameState.state == GameState.State.GameActive) speedMultiplier /= 2;
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F5) && gameState.state == GameState.State.GameActive)
        {
            trainIsMoving = !trainIsMoving;
            gameState.cheating = true;
        }

        if (trainIsMoving)
        {
            // calculate speed depending on the distance from the start of the level
            float currentStartSpeed = difficultyProperties.trainStartSpeed + difficultyProperties.trainStartSpeedBonus * currentTile.segmentId - (currentTile.segmentId == 0 ? difficultyProperties.fistSegmentSpeedSlow : 0);
            trainCurrentSpeed = (currentStartSpeed + (currentTile.coordinates.x - segmentStartX) * (difficultyProperties.trainSpeedIncrease + difficultyProperties.trainSpeedIncreaseIncrease * currentSegmentId)) * speedMultiplier;
            if (trainCurrentSpeed < currentStartSpeed) trainCurrentSpeed = difficultyProperties.trainStartSpeed;

            // rotate a little bit towards target vertical rotation 
            train.transform.rotation = Quaternion.RotateTowards(train.transform.rotation, Quaternion.Euler(0, train.transform.rotation.eulerAngles.y, targetVerticalRotation), Time.deltaTime * verticalRotationSpeed * trainCurrentSpeed);
            if (train.transform.position.x > maxX)
            {
                currentXScore = train.transform.position.x - startX - minX;
                if (currentXScore < 0) currentXScore = 0;
                maxX = train.transform.position.x;
                gameState.SetScore(Mathf.FloorToInt(currentXScore + currentExtraScore));
            }

            // check if powerup reached
            if (tileHasPowerup && Vector3.Distance(train.transform.position, new Vector3(currentTile.powerupRef.transform.position.x, train.transform.position.y, currentTile.powerupRef.transform.position.z)) < distanceToGetPowerup)
            {
                tileHasPowerup = false;
                powerupSystem.CollectPowerup(currentTile);
            }

            // if reached station
            if (willStationStop && Vector3.Distance(train.transform.position, new Vector3(currentTile.coordinates.x + (currentEntryEdge == 1 ? -stationStopX : stationStopX), train.transform.position.y, currentTile.coordinates.z)) < 2 * Time.deltaTime * trainCurrentSpeed)
            {
                soundSystem.StopSound("TrainMovement");
                soundSystem.PlaySound("TrainStop");
                trainIsMoving = false;
                currentExtraScore += trainStationScore;
                gameState.SetScore(Mathf.FloorToInt(currentXScore + currentExtraScore));
                StartCoroutine(StartMovingAfterDelay(difficultyProperties.stationStopDuration));
                StartCoroutine(StartTrainSound(difficultyProperties.stationStopDuration));
            }

            // if train is on a curve
            if (isCurve && currentPointIndex == 1)
            {
                // move train along the curbe
                arcDistancePassed += Time.deltaTime * trainCurrentSpeed;
                Vector3 tileMiddle = currentTile.coordinates + new Vector3(0, currentTile.elevation * TileGenerator.elevationHeight + trainHeight, 0);
                Vector3 nextPosition12 = Vector3.Lerp(tilePoints[currentPointIndex], tileMiddle, arcDistancePassed / arcDistance);
                Vector3 nextPosition23 = Vector3.Lerp(tileMiddle, nextPoint, arcDistancePassed / arcDistance);
                Vector3 nextPosition13 = Vector3.Lerp(nextPosition12, nextPosition23, arcDistancePassed / arcDistance);
                train.transform.position = nextPosition13;

                // rotate train along the curve
                targetHorizontalRotation = currentHorizontalRotation + ((currentExitEdge - currentEntryEdge == 2 || currentEntryEdge - currentExitEdge == 4) ? -1 : 1);
                Quaternion trainRoation = Quaternion.Lerp(Quaternion.Euler(0, currentHorizontalRotation * 60, train.transform.rotation.eulerAngles.z), Quaternion.Euler(0, targetHorizontalRotation * 60, train.transform.rotation.eulerAngles.z), arcDistancePassed / arcDistance);
                train.transform.rotation = trainRoation;

                // determine if end of the curve was reached
                if (Vector3.Distance(nextPosition13, nextPoint) < 2 * Time.deltaTime * trainCurrentSpeed)
                {
                    ReachedNextPoint();
                }
            }
            // if train is not on a curve
            else
            {
                // move train alongside the tracks
                Vector3 nextPosition = Vector3.MoveTowards(train.transform.position, nextPoint, Time.deltaTime * trainCurrentSpeed);
                train.transform.position = nextPosition;

                // determine if next point was reached
                if (Vector3.Distance(nextPosition, nextPoint) < 2 * Time.deltaTime * trainCurrentSpeed)
                {
                    ReachedNextPoint();
                }
            }
        }
    }

    private void ReachedNextTile()
    {
        currentPointIndex = 0;
        previousTile = currentTile;
        currentTile = nextTile;
        if (currentTile != null && currentTile.rails != null)
        {
            if (currentTile.powerup != PowerupSystem.PowerupType.NoPowerup) tileHasPowerup = true;
            willStationStop = currentTile.hasStation;
            currentTile.canBeMoved = false;
            currentEntryEdge = (currentExitEdge + 3) % 6;
            currentExitEdge = currentEntryEdge == currentTile.rails[0] ? currentTile.rails[1] : currentTile.rails[0];
            nextTile = currentTile.neighbors[currentExitEdge];
            (tilePoints, isCurve) = GetTilePoints();
            if (tilePoints == null) EndOfTrack();
            else nextPoint = tilePoints[1];
            CheckForNextStripe();
            CheckForNextSegment();
            if (!instruction2Displayed) CheckForInstruction2();
            if (!instruction3Displayed) CheckForInstruction3();
        }
        else EndOfTrack();
    }

    private void CheckForNextStripe()
    {
        if (currentTile.xIndex > previousTile.xIndex)
        {
            tiles.GenerateStripe(currentTile.xIndex + MapGenerator.tilesGeneratedAhead);
            tiles.RemoveStripe(currentTile.xIndex - MapGenerator.tilesGeneratedBehind - 1);
        }
        else if (currentTile.xIndex < previousTile.xIndex)
        {
            tiles.RemoveStripe(currentTile.xIndex + MapGenerator.tilesGeneratedAhead + 1);
            tiles.GenerateStripe(currentTile.xIndex - MapGenerator.tilesGeneratedBehind);
        }
    }

    private void CheckForNextSegment()
    {
        if (currentTile.segmentId > currentSegmentId)
        {
            segmentStartX = currentTile.coordinates.x;
            currentSegmentId = currentTile.segmentId;
            gameState.SetLevel(currentSegmentId + 1, true);
        }
    }

    private void CheckForInstruction2()
    {
        if (Tiles.tiles[currentTile.xIndex + instructionTilesAhead, currentTile.zIndex].segmentId == 1)
        {
            instruction2Displayed = true;
            uiManager.CheckForSecondInstruction();
        }
    }

    private void CheckForInstruction3()
    {
        if (Tiles.tiles[currentTile.xIndex + instructionTilesAhead, currentTile.zIndex].segmentId == 2)
        {
            instruction3Displayed = true;
            uiManager.CheckForThirdInstruction();
        }
    }

    private void ReachedNextPoint()
    {
        currentPointIndex += 1;
        // if reached end of tile
        if (currentPointIndex == 3)
        {
            ReachedNextTile();
        }
        else
        {
            arcDistancePassed = 0;
            currentHorizontalRotation = targetHorizontalRotation;
            nextPoint = tilePoints[currentPointIndex + 1];
            // set target vertical rotation if entering slope
            targetVerticalRotation = GetZRotationBetweenPoints(tilePoints[currentPointIndex], nextPoint);
            // make sure train finishes rotation so that it doesn't under-rotate over time
            train.transform.rotation = Quaternion.Euler(0, currentHorizontalRotation * 60, train.transform.rotation.eulerAngles.z);
        }
    }

    private IEnumerator StartMovingAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        trainIsMoving = true;
        willStationStop = false;
    }


    IEnumerator StartTrainSound(float time)
    {
        yield return new WaitForSeconds(time);
        if (gameState.state != GameState.State.GameOver) soundSystem.PlaySound("TrainMovement");
    }

    private void EndOfTrack()
    {
        soundSystem.PlaySound("TrainCrash");

        foreach (Transform child in transform)
        {
            child.GetComponent<BoxCollider>().enabled = true;
            Rigidbody rigidbody = child.GetComponent<Rigidbody>();
            CarriageController carriageController = child.GetComponent<CarriageController>();
            if (carriageController != null)
            {
                carriageController.enabled = false;
            }
            rigidbody.isKinematic = false;

            float randomAngleX = Random.Range(-maxCrashForceAngle, maxCrashForceAngle);
            float randomAngleZ = Random.Range(-maxCrashForceAngle, 0);
            Vector3 forceDirection = (Quaternion.Euler(randomAngleX, 0, randomAngleZ) * Vector3.up).normalized;

            rigidbody.AddForce(forceDirection * trainCrashForce * (difficultyProperties.kacperMode ? 10 : 1));
        }

        trainIsMoving = false;
        gameState.GameOver();
    }

    // calculate point through which the train needs to go in the current tile
    private (Vector3[] points, bool isCurve) GetTilePoints()
    {
        // if any of the following conditions are met the next tile is unreachable
        if (currentTile == null ||
            currentTile.rails == null ||
            (currentTile.rails[0] != currentEntryEdge && currentTile.rails[1] != currentEntryEdge) ||
            Mathf.RoundToInt(Mathf.Abs(previousTile.elevation - currentTile.elevation)) >= 2 ||
            currentTile.Equals(tileMoving.GetTileHeld()))
        {
            return (null, false);
        }
            

        // if the tile entered is the one we are currently replacing force replacement
        if (currentTile.Equals(tileMoving.GetReplaceTile()))
        {
            currentTile = tileMoving.GetReplaceTile();
            tileMoving.FinishTileReplacement();
        }

        // calculate the points
        float defaultHeight = currentTile.elevation * TileGenerator.elevationHeight + trainHeight;
        float point0Height = defaultHeight;
        float point3Height = defaultHeight;
        if (previousTile.elevation > currentTile.elevation) point0Height += RailsGenerator.initialSteepness * TileGenerator.elevationHeight;
        if (previousTile.elevation < currentTile.elevation) point0Height -= (1 - RailsGenerator.initialSteepness) * TileGenerator.elevationHeight;
        if (nextTile != null && nextTile.elevation > currentTile.elevation) point3Height += RailsGenerator.initialSteepness * TileGenerator.elevationHeight;
        if (nextTile != null && nextTile.elevation < currentTile.elevation) point3Height -= (1 - RailsGenerator.initialSteepness) * TileGenerator.elevationHeight;

        return (new Vector3[]
        {
            edgePositions[currentEntryEdge] + currentTile.coordinates + new Vector3(0, point0Height, 0),
            internalEdgePositions[currentEntryEdge] + currentTile.coordinates + new Vector3(0, defaultHeight, 0),
            internalEdgePositions[currentExitEdge] + currentTile.coordinates + new Vector3(0, defaultHeight, 0),
            edgePositions[currentExitEdge] + currentTile.coordinates + new Vector3(0, point3Height, 0),
        },
        currentTile.rails[0] != (currentTile.rails[1] + 3) % 6);
    }

    private float GetZRotationBetweenPoints(Vector3 currentPointPos, Vector3 nextPointPos)
    {
        float zRotation = 0;
        if (nextPointPos.y - currentPointPos.y > 0.01f)
        {
            zRotation = Mathf.Asin(Mathf.Abs((nextPointPos.y - currentPointPos.y) / Vector3.Distance(currentPointPos, nextPointPos))) * Mathf.Rad2Deg;

        }
        else if (nextPointPos.y - train.transform.position.y < -0.01f)
        {
            zRotation = -Mathf.Asin(Mathf.Abs((nextPointPos.y - currentPointPos.y) / Vector3.Distance(currentPointPos, nextPointPos))) * Mathf.Rad2Deg;
        }
        return zRotation;
    }

    // if next tile was modified points need to be recalculated to prevent wrong height
    public void RecalculateTilePoints(Tile modifiedTile)
    {
        if (nextTile != null && nextTile.Equals(modifiedTile))
        {
            nextTile = currentTile.neighbors[currentExitEdge];
            (tilePoints, isCurve) = GetTilePoints();
            nextPoint = tilePoints[currentPointIndex + 1];
            targetVerticalRotation = GetZRotationBetweenPoints(tilePoints[currentPointIndex], nextPoint);
        }
    }
}

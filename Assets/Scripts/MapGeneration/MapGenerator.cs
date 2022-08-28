using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public TrainController trainController;
    private Difficulty.DifficultyProperties difficultyProperties;
    public Tiles tilesScript;

    public static int mapWidth = 5;
    public static int maxSegments = 100;
    public static int oceanWidth = 2;
    public static int cliffWidth = 2;
    public static int maxElevation = 4;
    public static int cliffHeight = 5;
    public static float forestChance = 0.8f;
    public static int startRails = 5;
    public static int startRailsGeneration = 5;
    public static float stationChance = 0.1f;
    public static int stationMinStart = 9;
    public static int stationMinDistance = 2;
    public static float stationChanceIncrease = 0.2f;
    public static int powerupMinStart = 10;
    public static int powerupMinDistance = 1;
    public static float powerupChanceIncrease = 0.3f;
    public static int tilesGeneratedAhead = 16;
    public static int tilesGeneratedBehind = 8;
    public static float oceanChance = 0.8f;

    private int lastStationX = 0;
    private float currentStationChance = 0;
    private int lastPowerupX = 0;
    private float currentPowerupChance = 0;
    private int totalMapWidth;
    private float[][] railTypeCount;
    private float[] railTypeCountIncrement;
    private Vector3[,] positions;
    private bool[] illegalPowerupConfigurations;
    private float startRailsGenerationX;
    private LevelConfiguration[] levelConfigurations;

    public void StartByScheduler()
    {
        GameState gameState = GameObject.Find("GameState").GetComponent<GameState>();
        difficultyProperties = Difficulty.difficultyMapping[gameState.difficultySetting];

        totalMapWidth = mapWidth + oceanWidth + cliffWidth;

        // a structure containing information how many rails of a certain type and elvation were placed already
        // to get index of specific elvation+rail configuration use formula railTypeCount[elevation][Max(rail1,rail2)*2+Min(rail1,rail2)]  
        // possible rail configurations:
        // 0 -> 2, 2 -> 0 , position in array = 0 + 2*2 = 4
        // 0 -> 3, 3 -> 0 , position in array = 0 + 3*2 = 6
        // 0 -> 4, 4 -> 0 , position in array = 0 + 4*2 = 8
        // 1 -> 3, 3 -> 1 , position in array = 1 + 3*2 = 7
        // 1 -> 4, 4 -> 1 , position in array = 1 + 4*2 = 9
        // 1 -> 5, 5 -> 1 , position in array = 1 + 5*2 = 11
        // 2 -> 4, 4 -> 2 , position in array = 2 + 4*2 = 10
        // 2 -> 5, 5 -> 2 , position in array = 2 + 5*2 = 12
        // 3 -> 5, 5 -> 3 , position in array = 3 + 5*2 = 13
        railTypeCount = new float[maxElevation][];
        for (int i = 0; i < maxElevation; i++)
        {
            railTypeCount[i] = new float[14];
            for (int j = 0; j < 14; j++) railTypeCount[i][j] = 0;
        }

        // a table showing by how much to increase the rail configuration counter when placed, allowing to make certain configurations less or more likely
        // to get index of specific elvation+rail configuration use formula railTypeCountIncrement[Max(rail1,rail2)*2+Min(rail1,rail2)]  
        railTypeCountIncrement = new float[]
        {
            /* no configuration */ 0f,
            /* no configuration */ 0f,
            /* no configuration */ 0f,
            /* no configuration */ 0f,
            /* 0 -> 2, 2 -> 0 : */ 2f,
            /* no configuration */ 0f,
            /* 0 -> 3, 3 -> 0 : */ 1f,
            /* 1 -> 3, 3 -> 1 : */ 0.5f,
            /* 0 -> 4, 4 -> 0 : */ 0.5f,
            /* 1 -> 4, 4 -> 1 : */ 0.75f,
            /* 2 -> 4, 4 -> 2 : */ 0.5f,
            /* 1 -> 5, 5 -> 1 : */ 0.5f,
            /* 2 -> 5, 5 -> 2 : */ 1f,
            /* 3 -> 5, 5 -> 3 : */ 2f
        };

        // makes it so that power ups don't spawn on configurations that are true
        illegalPowerupConfigurations = new bool[]
        {
            /* no configuration */ false,
            /* no configuration */ false,
            /* no configuration */ false,
            /* no configuration */ false,
            /* 0 -> 2, 2 -> 0 : */ false,
            /* no configuration */ false,
            /* 0 -> 3, 3 -> 0 : */ false,
            /* 1 -> 3, 3 -> 1 : */ true,
            /* 0 -> 4, 4 -> 0 : */ true,
            /* 1 -> 4, 4 -> 1 : */ true,
            /* 2 -> 4, 4 -> 2 : */ true,
            /* 1 -> 5, 5 -> 1 : */ true,
            /* 2 -> 5, 5 -> 2 : */ false,
            /* 3 -> 5, 5 -> 3 : */ false,
        };

        int levels = difficultyProperties.levelConfiguration.Length;
        levelConfigurations = new LevelConfiguration[maxSegments];
        for (int i = 0;i < maxSegments; i++)
        {
            if (i < levels)
            {
                levelConfigurations[i] = difficultyProperties.levelConfiguration[i];
            }
            else
            {
                levelConfigurations[i] = difficultyProperties.levelConfiguration[levels - difficultyProperties.repeatLastLevels + (i - levels) % difficultyProperties.repeatLastLevels];
            }
            
        }
    }

    public class LevelConfiguration
    {
        public int maxElevation;
        public bool hasOceanTiles;

        public LevelConfiguration(int maxElevation, bool hasOceanTiles)
        {
            this.maxElevation = maxElevation;
            this.hasOceanTiles= hasOceanTiles;
        }
    }

    public Tile[,] GenerateMap()
    {
        int totalMapLength = 0;
        for (int i = 0; i < maxSegments; i++)
        {
            totalMapLength += difficultyProperties.segmentStartLength + i * difficultyProperties.segmentLengthIncrease;
        }

        positions = new Vector3[totalMapLength, totalMapWidth];
        for (int tileX = 0; tileX < totalMapLength; tileX++)
        {
            for (int tileZ = 0; tileZ < totalMapWidth; tileZ++)
            {
                positions[tileX, tileZ] = new Vector3((tileX * 2 + tileZ) * TileGenerator.h, 0, tileZ * 1.5f * TileGenerator.a);
                //Vector3 currentPosition = new Vector3((tileX * 2 + (tileZ % 2 == 0 ? 0 : 1)) * TileGenerator.h , 0, tileZ * 1.5f * TileGenerator.a);
            }
        }
        startRailsGenerationX = positions[startRailsGeneration, oceanWidth + (mapWidth - 1) / 2].x - 0.1f;

        Tile[,] tiles = new Tile[totalMapLength, totalMapWidth];

        int currentX = 0;
        for (int i = 0; i < maxSegments; i++)
        {
            GenerateSegment(tiles, i, currentX);
            currentX += difficultyProperties.segmentStartLength + i * difficultyProperties.segmentLengthIncrease;
        }

        return tiles;
    }

    public void GenerateSegment(Tile[,] tiles, int segmentId, int currentX)
    {
        for (int tileX = currentX; tileX < currentX + difficultyProperties.segmentStartLength + segmentId * difficultyProperties.segmentLengthIncrease; tileX++)
        {
            // increase the chance to spawn station and powerup on this stripe of tiles if conditions are met
            if (tileX >= stationMinStart && tileX - lastStationX > stationMinDistance) currentStationChance += stationChanceIncrease;
            if (tileX >= powerupMinStart && tileX - lastPowerupX > powerupMinDistance) currentPowerupChance += powerupChanceIncrease;

            int oceanTile = -1;
            if (Random.value < oceanChance)
            {
                oceanTile = Random.Range(oceanWidth, oceanWidth + mapWidth);
                if (tileX > 0 && tiles[tileX - 1, oceanTile].hasStation) oceanTile = -1;
            } 

            for (int tileZ = 0; tileZ < totalMapWidth; tileZ++)
            {
                Vector3 currentPosition = positions[tileX, tileZ];
                Tile newTile;

                // segments are devided on "diagonal lines" from the players perpective
                // to make the lines straight we cheat with the segmentId using actual xCoordinates
                int actualSegmentId = segmentId;
                float previousSegmentX = 0;
                float nextSegmentX = 0;
                if (segmentId > 0) previousSegmentX = positions[currentX - 1, (mapWidth - 1) / 2 + oceanWidth].x;
                if (segmentId < maxSegments - 1) nextSegmentX = positions[currentX + difficultyProperties.segmentStartLength + segmentId * difficultyProperties.segmentLengthIncrease - 1, (mapWidth - 1) / 2 + oceanWidth].x;
                if (segmentId > 0 && currentPosition.x < previousSegmentX + 0.1f) actualSegmentId = segmentId - 1;
                if (segmentId < maxSegments - 1 && currentPosition.x > nextSegmentX + 0.1f) actualSegmentId = segmentId + 1;

                // if tile is ocean
                if (tileZ < oceanWidth)
                {
                    newTile = new Tile(currentPosition, tileX, tileZ)
                    {
                        isOcean = true,
                        hasCollider = false,
                        elevation = 0,
                        canBeMoved = false,
                        segmentId = actualSegmentId
                    };
                }
                // if tile is cliff
                else if (tileZ >= mapWidth + oceanWidth)
                {
                    newTile = new Tile(currentPosition, tileX, tileZ)
                    {
                        type = TileType.Snow,
                        hasForest = false,
                        elevation = cliffHeight + Random.Range(0, 2),
                        isOcean = false,
                        hasCollider = true,
                        canBeMoved = false,
                        hasStation = false,
                        segmentId = actualSegmentId
                    };
                }
                // if tile is one of the starting tiles for the train
                else if (tileZ == oceanWidth + (mapWidth - 1) / 2 && tileX <= startRails)
                {
                    newTile = new Tile(currentPosition, tileX, tileZ)
                    {
                        type = (TileType) 1,
                        hasForest = Random.value < forestChance,
                        elevation = 2,
                        isOcean = false,
                        rails = new int[] { 1, 4 },
                        hasCollider = true,
                        canBeMoved = false,
                        hasStation = false,
                        segmentId = actualSegmentId
                    };
                }
                // if tile is standard game tile
                else
                {
                    PowerupSystem.PowerupType powerup = PowerupSystem.PowerupType.NoPowerup;

                    int elevation = 1;
                    int[] rails = null;
                    bool hasRails = Random.value < difficultyProperties.railsChance && currentPosition.x > startRailsGenerationX;
                    if (hasRails)
                    {
                        bool configurationSelected = false;
                        // if tile has rails we will select random configurations of elevation+rails
                        // if this configuration has a value higher than 0 in "railTypeCount" we will lower the value by 1 and roll again
                        while (!configurationSelected)
                        {
                            elevation = Random.Range(1, levelConfigurations[segmentId].maxElevation + 1);
                            int rail1 = Random.Range(0, 6);
                            List<int> rail2Possibilities = new List<int>() { 0, 1, 2, 3, 4, 5 };
                            rail2Possibilities.Remove(rail1);
                            rail2Possibilities.Remove((rail1 + 1) % 6);
                            rail2Possibilities.Remove((rail1 + 5) % 6);
                            int rail2 = rail2Possibilities[Random.Range(0, rail2Possibilities.Count)];
                            rails = new int[] { rail1, rail2 };
                            if (difficultyProperties.kacperMode) rails = new int[] { 1, 4 };

                            int railConfiguration = Mathf.Max(rail1, rail2) * 2 + Mathf.Min(rail1, rail2);
                            // if count for this elvation+rails configuration is 0 stick with this configuration and increase the counter appropiately
                            if (railTypeCount[elevation - 1][railConfiguration] < 0)
                            {
                                railTypeCount[elevation - 1][railConfiguration] += railTypeCountIncrement[railConfiguration];
                                configurationSelected = true;
                            }
                            // else lower the count and continue searching
                            else
                            {
                                railTypeCount[elevation - 1][railConfiguration] -= 1f;
                            }
                        }
                    }
                    else
                    {
                        elevation = Random.Range(1, levelConfigurations[segmentId].maxElevation + 1);
                    }

                    bool hasStation = false;
                    // station is more likely for every stripe without a station. If station is rolled the chance go back to 0
                    if (Random.value < currentStationChance && tileX > 0 && !tiles[tileX - 1, tileZ].isOcean && (!difficultyProperties.kacperMode || tileZ == oceanWidth + (mapWidth -1)/2))
                    {
                        hasStation = true;
                        currentStationChance = 0;
                        lastStationX = tileX;
                        if (hasRails)
                        {
                            int railConfiguration = Mathf.Max(rails[0], rails[1]) * 2 + Mathf.Min(rails[0], rails[1]);
                            railTypeCount[elevation - 1][railConfiguration] -= 1f;
                        }
                        rails = new int[] { 1, 4 };
                    }

                    // powerup has similar rules to station
                    if (Random.value < currentPowerupChance && !hasStation && rails != null && (!illegalPowerupConfigurations[Mathf.Max(rails[0], rails[1]) * 2 + Mathf.Min(rails[0], rails[1])] || difficultyProperties.kacperMode ))
                    {
                        powerup = (PowerupSystem.PowerupType)Random.Range(1, PowerupSystem.PowerupType.GetNames(typeof(PowerupSystem.PowerupType)).Length);
                        currentPowerupChance = 0;
                        lastPowerupX = tileX;
                    }

                    newTile = new Tile(currentPosition, tileX, tileZ)
                    {
                        type = (TileType) elevation - 1,
                        hasForest = Random.value < forestChance && elevation <= 2 && !hasStation,
                        elevation = elevation,
                        isOcean = false,
                        hasCollider = true,
                        canBeMoved = !hasStation,
                        hasStation = hasStation,
                        rails = rails,
                        segmentId = actualSegmentId,
                        powerup = powerup
                    };

                    // check if tile is ocean and replace it if it is
                    if (levelConfigurations[segmentId].hasOceanTiles && tileZ == oceanTile)
                    {
                        newTile = new Tile(currentPosition, tileX, tileZ)
                        {
                            isOcean = true,
                            hasCollider = false,
                            elevation = 0,
                            canBeMoved = false,
                            segmentId = actualSegmentId
                        };
                    }
                }

                tiles[tileX, tileZ] = newTile;
            }
        }
    }

    public void GenerateMoreRails(int railsCount, int maxNewRailDistance)
    {
        int currectTrainX = trainController.currentTile.xIndex;
        Tile[,] tiles = Tiles.tiles;
        List<Tile> possibleTiles = new List<Tile>();

        // find all tiles where rails can be placed
        for (int tileX = currectTrainX + 1; tileX < currectTrainX + 1 + maxNewRailDistance; tileX++)
        {
            for (int tileZ = oceanWidth; tileZ < oceanWidth + mapWidth; tileZ++)
            {
                Tile tile = tiles[tileX, tileZ];
                if (tile != null && tile.canBeMoved && tile.rails == null)
                {
                    possibleTiles.Add(tile);
                }
            }
        }

        // shuffle tile list
        for (int i = 0; i < possibleTiles.Count; i++)
        {
            Tile temp = possibleTiles[i];
            int randomIndex = Random.Range(i, possibleTiles.Count);
            possibleTiles[i] = possibleTiles[randomIndex];
            possibleTiles[randomIndex] = temp;
        }

        List<Tile> tilesToUpdate = new List<Tile>();
        // add random rails to first  railsCount rails in list
        for (int i = 0; i < railsCount; i++)
        {
            if (i >= possibleTiles.Count) break;
            Tile tile = possibleTiles[i];

            int rail1 = Random.Range(0, 6);
            List<int> rail2Possibilities = new List<int>() { 0, 1, 2, 3, 4, 5 };
            rail2Possibilities.Remove(rail1);
            rail2Possibilities.Remove((rail1 + 1) % 6);
            rail2Possibilities.Remove((rail1 + 5) % 6);
            int rail2 = rail2Possibilities[Random.Range(0, rail2Possibilities.Count)];
            tile.rails = new int[] { rail1, rail2 };
            if (difficultyProperties.kacperMode) tile.rails = new int[] { 1, 4 };
            tilesToUpdate.Add(tile);
        }

        tilesScript.GenerateTiles(tilesToUpdate, false);
    }
}

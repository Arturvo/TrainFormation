using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMoving : MonoBehaviour
{
    private static Vector3 tileEmissionWhenLit = new Vector3(0.1f, 0.1f, 0.1f);

    public GameObject intersectionPlanePrefab;
    private GameObject intersectionPlane;
    public TrainController trainController;
    public PowerupSystem powerupSystem;
    private SoundSystem soundSystem;
    private GameState gameState;

    private Tile tileHeld = null;
    private Tile tilePointed = null;
    private Tile replaceTile = null;
    
    public Tiles tiles;

    public State state = State.Default;

    public enum State
    {
        // player is not holding and not pointing at any tile
        Default,
        // player is not holding but pointing at a tile
        PointingTile,
        // player is holding a tile but not pointing at another tile
        HoldingTile,
        // player is holding a tile and pointing at another tile
        HoldingAndPointing
    }

    private void Awake()
    {
        soundSystem = GameObject.Find("SoundSystem").GetComponent<SoundSystem>();
        gameState = GameObject.Find("GameState").GetComponent<GameState>();
    }

    public Tile GetTileHeld() { return tileHeld; }
    public Tile GetReplaceTile() { return replaceTile; }

    public void StartByScheduler()
    {
        intersectionPlane = Instantiate(intersectionPlanePrefab, Vector3.zero, Quaternion.identity, transform);
        float intersectionPlaneXScale = Tiles.tilesX * 2 * TileGenerator.h;
        float intersectionPlaneZScale = (MapGenerator.oceanWidth + MapGenerator.mapWidth + MapGenerator.cliffWidth) * 1.5f * TileGenerator.a;
        intersectionPlane.transform.localScale = new Vector3(intersectionPlaneXScale, 1, intersectionPlaneZScale);
    }

    void Update()
    {
        if (gameState.state == GameState.State.GameActive)
        {
            switch (state)
            {
                case State.Default:
                    {
                        PointAtTiles();
                        break;
                    }
                case State.PointingTile:
                    {
                        if (Input.GetMouseButtonDown(0) && (tilePointed.canBeMoved || (tilePointed.hasStation && powerupSystem.canMoveStation)))
                        {
                            StartHoldingTile();
                        }
                        else PointAtTiles();
                        break;
                    }
                case State.HoldingTile:
                    {
                        if (Input.GetMouseButtonUp(0) || !IsMouseOverGameWindow())
                        {
                            CancelTileHolding();
                        }
                        else
                        {
                            TileFollowMouse(tileHeld);
                            PointAtTilesWhileHolding();
                        }
                        break;
                    }
                case State.HoldingAndPointing:
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            if (tilePointed.canBeMoved || (tilePointed.hasStation && powerupSystem.canMoveStation)) FinishTileReplacement();
                            else CancelTileReplacment();
                        }
                        else if (Input.GetMouseButtonDown(1) || !IsMouseOverGameWindow())
                        {
                            CancelTileReplacment();
                        }
                        else PointAtTilesWhileHolding();
                        break;
                    }
            }
        }
    }

    public void CancelTileHolding()
    {
        RevertLightUpTile(tileHeld);
        tileHeld.objectRef.SetActive(true);
        tileHeld.objectRef.transform.position = tileHeld.coordinates;
        tileHeld.colliderRef.layer = 8;
        tileHeld = null;
        state = State.Default;
    }

    public void StartHoldingTile()
    {
        tileHeld = tilePointed;
        state = State.HoldingTile;
        tilePointed = null;
        intersectionPlane.transform.position = new Vector3(0, tileHeld.objectRef.transform.position.y, 0);
        tileHeld.colliderRef.layer = 2;
    }

    public void FinishTileReplacement(bool isGameOver = false)
    {
        // check if station was moved to cancel the powerup
        if (tilePointed.hasStation || tileHeld.hasStation)
        {
            powerupSystem.StationMoved();
        }

        RevertLightUpTile(replaceTile);
        state = State.Default;

        // put the replaced tile in the place of the one moved
        Tiles.tiles[tileHeld.xIndex, tileHeld.zIndex] = tilePointed;
        tilePointed.coordinates = tileHeld.coordinates;
        tilePointed.xIndex = tileHeld.xIndex;
        tilePointed.zIndex = tileHeld.zIndex;
        tilePointed.neighbors = tileHeld.neighbors;
        tilePointed.segmentId = tileHeld.segmentId;
        List<Tile> tilesToRecover = new List<Tile>() { tilePointed, replaceTile };
        for (int i = 0; i < 6; i++)
        {
            Tile neighborTile = tilePointed.neighbors[i];
            if (neighborTile != null && !neighborTile.isOcean && !(neighborTile.Equals(tilePointed))) tilesToRecover.Add(neighborTile);
        }
        tiles.GenerateTiles(tilesToRecover, false);

        tileHeld.DestroyTile();

        if (!isGameOver) trainController.RecalculateTilePoints(tilePointed);

        tilePointed = null;
        replaceTile = null;
        tileHeld = null;

        soundSystem.PlaySound("MoveTile");
    }

    public void CancelTileReplacment()
    {
        state = State.Default;
        RevertLightUpTile(tileHeld);
        tileHeld.objectRef.transform.position = tileHeld.coordinates;
        tileHeld.colliderRef.layer = 8;
        tileHeld.objectRef.SetActive(true);
        tileHeld = null;

        if (tilePointed != null)
        {
            Tiles.tiles[tilePointed.xIndex, tilePointed.zIndex] = tilePointed;
            tilePointed.objectRef.SetActive(true);
            if (replaceTile != null) replaceTile.DestroyTile();
            List<Tile> tilesToRecover = new List<Tile>() { };
            for (int i = 0; i < 6; i++)
            {
                Tile neighborTile = tilePointed.neighbors[i];
                if (neighborTile != null && !neighborTile.isOcean && !(neighborTile.Equals(tileHeld))) tilesToRecover.Add(neighborTile);
            }
            tiles.GenerateTiles(tilesToRecover, false);
        }

        tilePointed = null;
        replaceTile = null;
        tileHeld = null;
    }

    private void TileFollowMouse(Tile tile)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layerMask = 1 << 11;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            tile.objectRef.transform.position = hit.point;
        }
    }

    private void PointAtTiles()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layerMask = 1 << 8;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) && IsMouseOverGameWindow())
        {
            Tile tileHit = hit.transform.gameObject.GetComponent<TileColliderScript>().tile;
            if (!tileHit.Equals(tilePointed))
            {
                RevertLightUpTile(tilePointed);
                tilePointed = tileHit;
                if (tilePointed.canBeMoved || (tilePointed.hasStation && powerupSystem.canMoveStation)) LightUpTile(tilePointed);
                state = State.PointingTile;
            }
        }
        else
        {
            RevertLightUpTile(tilePointed);
            tilePointed = null;
            state = State.Default;
        }
    }

    public void CancelPointing()
    {
        RevertLightUpTile(tilePointed);
        tilePointed = null;
        state = State.Default;
    }

    private void PointAtTilesWhileHolding()
    {
        if (IsMouseOverGameWindow())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = 1 << 8;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Tile tileHit = hit.transform.gameObject.GetComponent<TileColliderScript>().tile;
                if (!tileHit.Equals(tilePointed) && (tileHit.canBeMoved || (tileHit.hasStation && powerupSystem.canMoveStation)))
                {
                    // recover previously pointed tile
                    if (tilePointed != null)
                    {
                        Tiles.tiles[tilePointed.xIndex, tilePointed.zIndex] = tilePointed;
                        tilePointed.objectRef.SetActive(true);
                        if (replaceTile != null) replaceTile.DestroyTile();
                        List<Tile> tilesToRecover = new List<Tile>() { };
                        for (int i = 0; i < 6; i++)
                        {
                            Tile neighborTile = tilePointed.neighbors[i];
                            if (neighborTile != null && !neighborTile.isOcean && !(neighborTile.Equals(tileHeld))) tilesToRecover.Add(neighborTile);
                        }
                        tiles.GenerateTiles(tilesToRecover, false);
                        replaceTile = null;
                    }

                    // swap pointed tile to the held tile
                    tilePointed = tileHit;
                    replaceTile = new Tile(tilePointed.coordinates, tilePointed.xIndex, tilePointed.zIndex)
                    {
                        type = tileHeld.type,
                        hasForest = tileHeld.hasForest,
                        elevation = tileHeld.elevation,
                        isOcean = false,
                        rails = tileHeld.rails,
                        hasCollider = true,
                        neighbors = tilePointed.neighbors,
                        segmentId = tilePointed.segmentId,
                        hasStation = tileHeld.hasStation,
                        powerup = tileHeld.powerup,
                        canBeMoved = tileHeld.canBeMoved,
                    };
                    tilePointed.objectRef.SetActive(false);
                    Tiles.tiles[tilePointed.xIndex, tilePointed.zIndex] = replaceTile;
                    List<Tile> tilesToUpdate = new List<Tile>() { replaceTile };
                    for (int i = 0; i < 6; i++)
                    {
                        Tile neighborTile = replaceTile.neighbors[i];
                        if (neighborTile != null && !neighborTile.isOcean && !(neighborTile.Equals(tileHeld))) tilesToUpdate.Add(neighborTile);
                    }
                    tiles.GenerateTiles(tilesToUpdate, false);
                    state = State.HoldingAndPointing;
                    tileHeld.objectRef.SetActive(false);
                    LightUpTile(replaceTile);
                    replaceTile.colliderRef.GetComponent<MeshCollider>().enabled = false;
                    replaceTile.colliderRef.GetComponent<MeshCollider>().enabled = true;
                }
            }
            else
            {
                tileHeld.objectRef.SetActive(true);
                state = State.HoldingTile;

                // recover previously pointed tile
                if (tilePointed != null)
                {
                    Tiles.tiles[tilePointed.xIndex, tilePointed.zIndex] = tilePointed;
                    tilePointed.objectRef.SetActive(true);
                    if (replaceTile != null) replaceTile.DestroyTile();
                    List<Tile> tilesToRecover = new List<Tile>() { };
                    for (int i = 0; i < 6; i++)
                    {
                        Tile neighborTile = tilePointed.neighbors[i];
                        if (neighborTile != null && !neighborTile.isOcean && !(neighborTile.Equals(tileHeld))) tilesToRecover.Add(neighborTile);
                    }
                    tiles.GenerateTiles(tilesToRecover, false);
                    replaceTile = null;
                    tilePointed = null;
                }
            }
        }
    }

    private void LightUpTile(Tile tile)
    {
        if (tile != null)
        {
            tile.objectRef.GetComponent<Renderer>().material.SetVector("EmissionMod", tileEmissionWhenLit);
            Renderer[] childRenderers = tile.objectRef.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in childRenderers)
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", new Color(tileEmissionWhenLit.x, tileEmissionWhenLit.y, tileEmissionWhenLit.z));
            }
        }
    }

    private void RevertLightUpTile(Tile tile)
    {
        if (tile != null)
        {
            tile.objectRef.GetComponent<Renderer>().material.SetVector("EmissionMod", Vector4.zero);
            Renderer[] childRenderers = tile.objectRef.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in childRenderers)
            {
                renderer.material.DisableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", Color.white);
            }
        }
    }

    private bool IsMouseOverGameWindow() { 
        return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y) && 
            !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(); 
    } 
}

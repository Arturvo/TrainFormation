using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tiles : MonoBehaviour
{
    public string currentMapName;
    public GameObject tileDebugTextPrefab;
    public GameObject positionMarkerTestPrefab;
    private Difficulty.DifficultyProperties difficultyProperties;

    public static Tile[,] tiles;

    public static int tilesX;
    public static int tilesZ;

    private bool gridEnabled = false;

    // tile properties
    public static readonly Dictionary<TileType, TileTypeProperties> tilePropertyMapping = new Dictionary<TileType, TileTypeProperties>
    {
        {TileType.Grass, new TileTypeProperties()
            {
                forestChance = 0.7f
            }
        },
        {TileType.Plain, new TileTypeProperties()
            {
                forestChance = 0.4f
            }
        },
        {TileType.Rock, new TileTypeProperties()
            {
                forestChance = 0
            }
        },
        {TileType.Snow, new TileTypeProperties()
            {
                forestChance = 0
            }
        }
    };

    public void StartByScheduler()
    {
        GameState gameState = GameObject.Find("GameState").GetComponent<GameState>();
        difficultyProperties = Difficulty.difficultyMapping[gameState.difficultySetting];

        tilesX = 0;
        for (int i = 0; i < MapGenerator.maxSegments; i++)
        {
            tilesX += difficultyProperties.segmentStartLength + i * difficultyProperties.segmentLengthIncrease;
        }
        tilesZ = MapGenerator.mapWidth + MapGenerator.oceanWidth + MapGenerator.cliffWidth;
    }

    public void GenerateMap()
    {
        tiles = GetComponent<MapGenerator>().GenerateMap();
    }

    // stripe is a column of tiles
    public void GenerateStripe(int x)
    {
        if (x > 0)
        {
            List<Tile> tileList = new List<Tile>();
            List<Tile> oceanTileList = new List<Tile>();
            for (int z = 0; z < MapGenerator.mapWidth + MapGenerator.cliffWidth + MapGenerator.oceanWidth; z++)
            {
                if (tiles[x, z] != null && !tiles[x, z].isOcean && tiles[x, z].objectRef == null)
                {
                    tileList.Add(tiles[x, z]);
                    //DebugTextOnTile(x + "," + z, tiles[x, z]);
                    //if(tiles[x, z].rails != null) DebugTextOnTile(tiles[x, z].rails[0] + "," + tiles[x, z].rails[1], tiles[x, z]);
                }
                else if (tiles[x, z] != null && tiles[x, z].objectRef == null)
                {
                    oceanTileList.Add(tiles[x, z]);
                }
            }

            GenerateTiles(tileList, false);
            GenerateTiles(oceanTileList, true);
        }
    }

    public void RemoveStripe(int x)
    {
        if (x > 0)
        {
            for (int z = 0; z < MapGenerator.mapWidth + MapGenerator.cliffWidth + MapGenerator.oceanWidth; z++)
            {
                if (tiles[x, z] != null) tiles[x, z].DestroyTile();
            }
        }
    }

    public void GenerateInitialTiles()
    {
        for (int x = TrainController.trainStartHex - MapGenerator.tilesGeneratedBehind; x <= TrainController.trainStartHex + MapGenerator.tilesGeneratedAhead; x++)
        {
            GenerateStripe(x);
        }
    }

    public void GenerateTiles(List<Tile> tileListArg, bool isOcean)
    {
        foreach (Tile tile in tileListArg)
        {
            tile.DestroyTile();

            int totalMapWidth = MapGenerator.mapWidth + MapGenerator.oceanWidth + MapGenerator.cliffWidth;

            if (tile.zIndex + 1 < totalMapWidth) tile.neighbors[0] = tiles[tile.xIndex, tile.zIndex + 1];
            if (tile.xIndex + 1 < tilesX) tile.neighbors[1] = tiles[tile.xIndex + 1, tile.zIndex];
            if (tile.xIndex + 1 < tilesX && tile.zIndex - 1 >= 0) tile.neighbors[2] = tiles[tile.xIndex + 1, tile.zIndex - 1];
            if (tile.zIndex - 1 >= 0) tile.neighbors[3] = tiles[tile.xIndex, tile.zIndex - 1];
            if (tile.xIndex - 1 >= 0) tile.neighbors[4] = tiles[tile.xIndex - 1, tile.zIndex];
            if (tile.xIndex - 1 >= 0 && tile.zIndex + 1 < totalMapWidth) tile.neighbors[5] = tiles[tile.xIndex - 1, tile.zIndex + 1];

            /*
            if (tile.zIndex % 2 == 0)
            {
                if (tile.zIndex + 1 < totalMapWidth) tile.neighbors[0] = tiles[tile.xIndex, tile.zIndex + 1];
                if (tile.xIndex + 1 < MapGenerator.segmentLength * MapGenerator.maxSegments) tile.neighbors[1] = tiles[tile.xIndex + 1, tile.zIndex];
                if (tile.zIndex - 1 >= 0) tile.neighbors[2] = tiles[tile.xIndex, tile.zIndex - 1];
                if (tile.xIndex - 1 >= 0 && tile.zIndex - 1 >= 0) tile.neighbors[3] = tiles[tile.xIndex - 1, tile.zIndex - 1];
                if (tile.xIndex - 1 >= 0) tile.neighbors[4] = tiles[tile.xIndex - 1, tile.zIndex];
                if (tile.xIndex - 1 >= 0 && tile.zIndex + 1 < totalMapWidth) tile.neighbors[5] = tiles[tile.xIndex - 1, tile.zIndex + 1];
            }
            else
            {
                if (tile.xIndex + 1 < MapGenerator.segmentLength * MapGenerator.maxSegments && tile.zIndex + 1 < totalMapWidth) tile.neighbors[0] = tiles[tile.xIndex + 1, tile.zIndex + 1];
                if (tile.xIndex + 1 < MapGenerator.segmentLength * MapGenerator.maxSegments) tile.neighbors[1] = tiles[tile.xIndex + 1, tile.zIndex];
                if (tile.xIndex + 1 < MapGenerator.segmentLength * MapGenerator.maxSegments && tile.zIndex - 1 >= 0) tile.neighbors[2] = tiles[tile.xIndex + 1, tile.zIndex - 1];
                if (tile.zIndex - 1 >= 0) tile.neighbors[3] = tiles[tile.xIndex, tile.zIndex - 1];
                if (tile.xIndex - 1 >= 0) tile.neighbors[4] = tiles[tile.xIndex - 1, tile.zIndex];
                if (tile.zIndex + 1 < totalMapWidth) tile.neighbors[5] = tiles[tile.xIndex, tile.zIndex + 1];
            }
            */
        }
        if (isOcean)
        {
            GetComponent<OceanGenerator>().GenerateOcean(tileListArg);
        }
        else
        {
            GetComponent<TileGenerator>().GenerateTiles(tileListArg);
            GetComponent<ForestGenerator>().GenerateForests(tileListArg);
            GetComponent<GridGenerator>().GenerateGrid(tileListArg, gridEnabled);
            GetComponent<LevelBorderGenerator>().GenerateLevelBorder(tileListArg);
            GetComponent<RiverRenderer>().RenderRivers(tileListArg);
            GetComponent<RailsGenerator>().GenerateRails(tileListArg);
            GetComponent<PowerupGenerator>().GeneratePowerups(tileListArg);
            foreach (Tile tile in tileListArg) tile.vertices = null;
        }
        
    }

    // we are using the same size collider for all tiles regardless of height for the sake of tile movement
    // this function will scale up colliders for all existing tiles to match the tile height
    public void ScaleUpColliders()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int z = 0; z < tiles.GetLength(1); z++)
            {
                Tile tile = tiles[x, z];
                if (tile != null && tile.hasCollider && tile.objectRef != null && tile.colliderRef != null)
                {
                    tile.colliderRef.transform.position = tile.coordinates + new Vector3(0, tile.elevation * TileGenerator.elevationHeight);
                    tile.colliderRef.transform.localScale = new Vector3(TileGenerator.a, tile.elevation * TileGenerator.elevationHeight, TileGenerator.a);
                }
            }
        }
    }

    void Update()
    {
        // disable grid when players presses 'G'
        if (Input.GetKeyDown(KeyCode.G))
        {
            gridEnabled = !gridEnabled;
            for (int x = 0; x < tilesX; x++)
            {
                for (int z = 0; z < MapGenerator.mapWidth + MapGenerator.oceanWidth; z++)
                {
                    if (tiles[x, z] != null && !tiles[x, z].isOcean && tiles[x, z].hasCollider && tiles[x, z].objectRef != null)
                    {
                        tiles[x, z].objectRef.GetComponent<LineRenderer>().enabled = gridEnabled;
                    }
                }
            }
        }
    }

    public static void DebugTextOnTile(string text, Tile tile, bool diffCoord = false, Vector3 coords = new Vector3())
    {
        GameObject.Find("Tiles").GetComponent<Tiles>().DebugTextOnTilePriv(text, tile, diffCoord, coords);
    }

    private void DebugTextOnTilePriv(string text, Tile tile, bool diffCoord = false, Vector3 coords = new Vector3())
    {
        GameObject canvasObject = Instantiate(tileDebugTextPrefab, transform);
        if (diffCoord) canvasObject.transform.localPosition = coords + new Vector3(0, tile.elevation * TileGenerator.elevationHeight + 5, 0);
        else canvasObject.transform.localPosition = tile.coordinates + new Vector3(0, tile.elevation * TileGenerator.elevationHeight + 5, 0);
        canvasObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
        GameObject textObject = canvasObject.transform.GetChild(0).gameObject;
        TextMeshProUGUI textMeshPro = textObject.GetComponent<TextMeshProUGUI>();
        textMeshPro.text = text;
    }

    public static void DebugMarkerOnPosition(Vector3 position)
    {
        GameObject.Find("Tiles").GetComponent<Tiles>().DebugMarkerOnPositionPriv(position);
    }

    private void DebugMarkerOnPositionPriv(Vector3 position)
    {
        Instantiate(positionMarkerTestPrefab, position, Quaternion.identity);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public GameObject tilesParent;
    public GameObject tilePrefab;
    public GameObject tileColliderPrefab;
    public ComputeShader vertexShader;

    // VALUES THAT CAN BE TWEAKED
    // --------------------------
    // hex size
    public static readonly float a = 10;
    // each subdivion doubles the amount of points on the edge of the rectangle from which the hex is generated
    public static int subdiviosions = 6;
    // how much of the hex is dedicated to showing the elevation slope
    public static float elevationBorderSize = 0.3f;
    // height of one elevation level
    public static readonly float elevationHeight = 3;
    // global noise setting
    public static readonly float layersValueMultiplier = 2f;
    public static readonly float layersDensityMultiplier = 1.73f;
    // cliff perlin noise parameters
    public static readonly float cliffNoiseDensity = 7f;
    public static int cliffNoiseLayers = 9;
    public static readonly float cliffNoiseValuePercentOnMildEdge = 0.2f;
    public static readonly float cliffNoiseMildingPower = 2f;
    // height perlin noise paramaters
    public static readonly float heightNoiseMaxSize = 2f;
    public static readonly float heightNoiseDensity = 5f;
    public static int heightNoiseLayers = 9;
    public static readonly float heightReductionPower1 = 3; // caused by position
    public static readonly float heightReductionPower2 = 1; // caused by rivers
    // river perlin noise parameters
    public static readonly float riverNoiseMaxSize = 1f;
    public static readonly float riverNoiseDensity = 10f;
    public static int riverNoiseLayers = 9;
    // texture perlin noise parameters
    public static readonly float textureNoiseMaxSize = 1f;
    public static readonly float textureNoiseDensity = 10;
    public static int textureNoiseLayers = 9;
    // secondary texture parameters
    public static readonly float secondaryTextureNoiseMaxSize = 0.05f;
    public static readonly float secondaryTextureNoiseDensity = 2;
    public static int secondaryTextureNoiseLayers = 9;
    public static readonly float secondaryTextureNoiseSpreadPower = 1.4f;
    // texture mixing parameters 
    public static readonly float mixingNoiseMaxSize = 0.3f;
    public static readonly float mixingNoiseDensity = 8;
    public static int mixingNoiseLayers = 9;
    public static readonly float mixingPower = 1.5f;
    public static readonly float mixingNoiseRampingReduction = 5;
    public static readonly float coastRampingReduction = 1;
    public static readonly float riverRampingReductionHeigher = 10;
    public static readonly float riverRampingReductionLower = 1f;
    public static readonly float riverRampingReductionEqual = 5;
    public static readonly float riverStrenghtMultiplierHeigher = 0.5f;
    public static readonly float riverStrenghtMultiplierLower = 2;
    public static readonly float riverStrenghtMultiplierEqual = 1;
    // river parameters
    public static float riverWidth = 0.15f;
    public static readonly float riverMaxDepth = 2f;
    public static readonly float riverShapePower = 0.7f;

    // tile properties
    public static readonly Dictionary<TileType, TileTypeProperties> tilePropertyMapping = new Dictionary<TileType, TileTypeProperties>
    {
        {TileType.Grass, new TileTypeProperties()
            {
                primaryColorMin = new Color32(31, 112, 34, 255),
                primaryColorMax = new Color32(24, 89, 27, 255),
                secondaryColor = new Color32(162, 173, 52, 255),
                layerPriority = 100,
                rockRiver = false
            }
        },
        {TileType.Plain, new TileTypeProperties()
            {
                primaryColorMin = new Color32(162, 173, 52, 255),
                primaryColorMax = new Color32(113, 120, 40, 255),
                secondaryColor = new Color32(173, 137, 52, 255),
                layerPriority = 90,
                rockRiver = false
            }
        },
        {TileType.Rock, new TileTypeProperties()
            {
                primaryColorMin = new Color32(120, 120, 120, 255),
                primaryColorMax = new Color32(80, 80, 80, 255),
                secondaryColor = new Color32(20, 20, 20, 255),
                layerPriority = 80,
                rockRiver = true
            }
        },
        {TileType.Snow, new TileTypeProperties()
            {
                primaryColorMin = new Color32(255, 255, 255, 255),
                primaryColorMax = new Color32(180, 180, 180, 255),
                secondaryColor = new Color32(150, 150, 150, 255),
                layerPriority = 110,
                rockRiver = true
            }
        },
    };
    public static readonly TileTypeProperties cliffProperties = new TileTypeProperties()
    {
        primaryColorMin = new Color32(50, 50, 50, 255),
        primaryColorMax = new Color32(100, 100, 100, 255),
        secondaryColor = new Color32(255, 255, 255, 255),
        layerPriority = 1000
    };
    public static readonly TileTypeProperties coastProperties = new TileTypeProperties()
    {
        primaryColorMin = new Color32(220, 192, 139, 255),
        primaryColorMax = new Color32(220, 192, 139, 255),
        secondaryColor = new Color32(220, 192, 139, 255),
        layerPriority = 900
    };

    // should the mesh be smoothed out
    public static readonly bool smoothShading = false;

    // VALUES CALCULATED BASED ON OTHER VALUES
    // ---------------------------------------
    // hex height
    public static float h;
    // how many points are there on the rectangle from which the hex is generated
    public static Quaternion tileRotation;
    public static int pointsSize;
    // "diameter" of a hexagon in points
    public static int r;
    // coordinates of the lower left corner of a rectangle from which the hex is generated
    public static Vector3 startPoint;
    // distance between points
    public static float xDistance;
    public static float zDistance;
    // how many vertices are dedicated for showing the elevation slope (vertex count)
    public static int elevationBorderSizePoints;
    // how big a noise texture needs to be
    public static int perlinMax;
    // size of the first noise layer
    public static float cliffNoiseStartSize;
    public static float heightNoiseStartSize;
    public static float riverNoiseStartSize;
    public static float textureNoiseStartSize;
    public static float secondaryTextureNoiseStartSize;
    public static float mixingNoiseStartSize;
    // cliff size limit
    public static float cliffNoiseMaxSize;
    // adjust density to the size of the map so that it is always the same
    public static float cliffNoiseAdjustedDensity;
    public static float heightNoiseAdjustedDensity;
    public static float riverNoiseAdjustedDensity;
    public static float textureNoiseAdjustedDensity;
    public static float secondaryTextureNoiseAdjustedDensity;
    public static float mixingNoiseAdjustedDensity;
    // river width in points
    public static int riverWidthPoints;
    // triangles array, same for every hex
    private int[] triangles;
    private int[] smoothTriangles;
    // a precalculated map of closest edges to be eable to find 2 closest edges quickly
    private (int closest, int secondClosest)[,] closestEdges;
    private readonly int numberOfThreads = 128;
    

    // calculate in advance everything that can be calculated in advance
    public void StartByScheduler()
    {
        // apply settings if found
        GameObject graphicSettings = GameObject.Find("GraphicSettings");
        if (graphicSettings != null)
        {
            GraphicSettings graphicSettingsScript = graphicSettings.GetComponent<GraphicSettings>();
            GraphicSettings.SettingPrefab settingPrefab = graphicSettingsScript.settingPrefabMapping[graphicSettingsScript.currentSetting];
            subdiviosions = settingPrefab.terrainQuality;
            cliffNoiseLayers = settingPrefab.cliffNoiseLayers;
            heightNoiseLayers = settingPrefab.heightNoiseLayers;
            riverNoiseLayers = settingPrefab.riverNoiseLayers;
            textureNoiseLayers = settingPrefab.textureNoiseLayers;
            secondaryTextureNoiseLayers = settingPrefab.secondaryTextureNoiseLayers;
            mixingNoiseLayers = settingPrefab.mixingNoiseLayers;
            elevationBorderSize = settingPrefab.elevationBorderSize;
            riverWidth = settingPrefab.riverWidth;
        }

        // calculate all derived parameters
        h = (a * Mathf.Sqrt(3)) / 2;
        tileRotation = Quaternion.identity;
        tileRotation.eulerAngles = new Vector3(0, 30, 0);
        pointsSize = Mathf.RoundToInt(Mathf.Pow(2, subdiviosions)) + 1;
        r = (pointsSize - 1) / 2;
        xDistance = (2f * a) / (pointsSize - 1);
        zDistance = (2f * h) / (pointsSize - 1);
        startPoint = new Vector3(-1.5f * a, 0, -h);
        elevationBorderSizePoints = Mathf.RoundToInt(r * elevationBorderSize);
        int perlinMaxX = (Tiles.tilesX + Tiles.tilesZ) * r + 1;
        int perlinMaxZ = (Tiles.tilesX + Tiles.tilesZ * 2 - 1) * r + 1;
        perlinMax = Mathf.Max(perlinMaxX, perlinMaxZ);
        cliffNoiseMaxSize = elevationBorderSize * a;
        cliffNoiseStartSize = CalculateStartPerlinStrenght(cliffNoiseMaxSize, cliffNoiseLayers, layersValueMultiplier);
        heightNoiseStartSize = CalculateStartPerlinStrenght(heightNoiseMaxSize, heightNoiseLayers, layersValueMultiplier);
        riverNoiseStartSize = CalculateStartPerlinStrenght(riverNoiseMaxSize, riverNoiseLayers, layersValueMultiplier);
        textureNoiseStartSize = CalculateStartPerlinStrenght(textureNoiseMaxSize, textureNoiseLayers, layersValueMultiplier);
        secondaryTextureNoiseStartSize = CalculateStartPerlinStrenght(secondaryTextureNoiseMaxSize, secondaryTextureNoiseLayers, layersValueMultiplier);
        mixingNoiseStartSize = CalculateStartPerlinStrenght(mixingNoiseMaxSize, mixingNoiseLayers, layersValueMultiplier);
        int densityBalancer = Mathf.Max(Tiles.tilesX + Tiles.tilesZ);
        cliffNoiseAdjustedDensity = cliffNoiseDensity * densityBalancer;
        heightNoiseAdjustedDensity = heightNoiseDensity * densityBalancer;
        riverNoiseAdjustedDensity = riverNoiseDensity * densityBalancer;
        textureNoiseAdjustedDensity = textureNoiseDensity * densityBalancer;
        secondaryTextureNoiseAdjustedDensity = secondaryTextureNoiseDensity * densityBalancer;
        mixingNoiseAdjustedDensity = mixingNoiseDensity * densityBalancer;
        riverWidthPoints = Mathf.RoundToInt(r * riverWidth);
        CalculateTriangles();
        CalculateClosestEdges();
    }

    // calculate a starting strenght that will cause a perlin to reach a maximum value after adding all the layers
    private static float CalculateStartPerlinStrenght(float max, float layers, float strenghtMultiplier)
    {
        float maxValueDevider = 0;
        for (int i = 0; i < layers; i++)
        {
            maxValueDevider = maxValueDevider + 1 / Mathf.Pow(strenghtMultiplier, i);
        }
        return max / maxValueDevider;
    }

    public void CalculateTriangles()
    {
        List<int> triangleList = new List<int>();
        List<(int index, bool secondHalf, bool addTriangle1, bool addTriangle2)> newVertices = new List<(int index, bool secondHalf, bool addTriangle1, bool addTriangle2)>();
        int vertexCount = 0;
        // for each row
        for (int z = 0; z < pointsSize; z++)
        {
            // for each vertex that from the previous row
            foreach ((int index, bool secondHalf, bool addTriangle1, bool addTriangle2) in newVertices)
            {
                // calculating vertices of the triangles is different depending if the origin vertice is located in bottom or top half of the hex
                if (addTriangle1)
                {
                    triangleList.Add(index);
                    triangleList.Add(index + newVertices.Count - (secondHalf ? 1 : 0));
                    triangleList.Add(index + newVertices.Count + (secondHalf ? 0 : 1));
                }
                if (addTriangle2)
                {
                    triangleList.Add(index);
                    triangleList.Add(index + newVertices.Count + (secondHalf ? 0 : 1));
                    triangleList.Add(index + 1);
                }
            }
            // remove all element from the list, so that is always contains only last row
            newVertices.Clear();
            // for each vertex in a row
            for (int x = 0; x < pointsSize; x++)
            {
                // this if statement is used to skip calculation for the points in the grid that won't be a part of the hex
                // the specific equestion is derrived from the function that would "cut off" unwanted points if you were to plot them
                if (z >= -x + r && z <= -x + 3 * r)
                {
                    newVertices.Add((vertexCount, z >= r, x > 0 && z < -x + 3 * r, x < pointsSize - 1 && z < -x + 3 * r));
                    vertexCount++;
                }
            }
        }
        triangles = triangleList.ToArray();

        // calculate a triangle vertion for the smooth shaded mesh
        smoothTriangles = new int[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            smoothTriangles[i] = i;
        }
    }

    public void CalculateClosestEdges()
    {
        closestEdges = new (int closest, int secondClosest)[pointsSize, pointsSize];
        for (int z = 0; z < pointsSize; z++)
        {
            for (int x = 0; x < pointsSize; x++)
            {
                // this if statement is used to skip calculation for the points in the grid that won't be a part of the hex
                // the specific equestion is derrived from the function that would "cut off" unwanted points if you were to plot them
                if (z >= -x + r && z <= -x + 3 * r)
                {
                    int[] distances = new int[]
                    {
                        pointsSize - 1 - z,
                        -x + 3*r - z,
                        pointsSize - 1 - x,
                        z,
                        z - (-x + r),
                        x
                    };

                    int closestEdge = -1;
                    int secondClosestEdge = -1;

                    for (int i = 0; i < 6; i++)
                    {
                        if (closestEdge < 0 || distances[closestEdge] > distances[i])
                        {
                            secondClosestEdge = closestEdge;
                            closestEdge = i;
                        }
                        else if (secondClosestEdge < 0 || distances[secondClosestEdge] > distances[i]) secondClosestEdge = i;
                    }

                    closestEdges[x, z] = (closestEdge, secondClosestEdge);
                }
            }
        }
    }

    private bool HasCornerRiver(Tile tile, int closestEdge, int secondClosestEdge)
    {
        Tile closestNeighbour = tile.neighbors[closestEdge];
        int closestEdgeMapping = (closestEdge + 3) % 6;
        bool secondClosestBigger = (secondClosestEdge > closestEdge || (secondClosestEdge == 0 && closestEdge == 5)) && !(secondClosestEdge == 5 && closestEdge == 0);

        return closestNeighbour != null && closestNeighbour.rivers[secondClosestBigger ? (closestEdgeMapping + 5)%6 : (closestEdgeMapping + 1) % 6];
    }

    public struct Vertex
    {
        public int tile;
        public int tileX;
        public int tileZ;
        public int tileType;
        public int tileType1;
        public int tileType2;
        public int x;
        public int z;
        public int elevation;
        public int elevation1;
        public int elevation2;
        public Vector3 position;
        public int colorR;
        public int colorG;
        public int colorB;
        public int steepness;
        public int rivers;
        public float gridHeight;
    }

    public void GenerateTiles(List<Tile> tiles)
    {
        Tile[] tileArray = tiles.ToArray();

        // create an array with all vertices of all tiles to be passed to compute shader
        List<Vertex> vertices = new List<Vertex>();
        for (int i = 0; i < tileArray.Length; i++)
        {
            for (int z = 0; z < pointsSize; z++)
            {
                for (int x = 0; x < pointsSize; x++)
                {
                    // this if statement is used to skip calculation for the points in the grid that won't be a part of the hex
                    // the specific equestion is derrived from the function that would "cut off" unwanted points if you were to plot them
                    if (z >= -x + r && z <= -x + 3 * r)
                    {
                        (int closest, int secondClosest) = closestEdges[x, z];
                        Vertex vertex = new Vertex()
                        {
                            tile = i,
                            tileX = tileArray[i].xIndex,
                            tileZ = tileArray[i].zIndex,
                            tileType = (int)tileArray[i].type,
                            tileType1 = tileArray[i].neighbors[closest] != null ? (int)tileArray[i].neighbors[closest].type : 0,
                            tileType2 = tileArray[i].neighbors[secondClosest] != null ? (int)tileArray[i].neighbors[secondClosest].type : 0,
                            x = x,
                            z = z,
                            elevation = tileArray[i].elevation,
                            elevation1 = tileArray[i].neighbors[closest] != null ? tileArray[i].neighbors[closest].elevation : 0,
                            elevation2 = tileArray[i].neighbors[secondClosest] != null ? tileArray[i].neighbors[secondClosest].elevation : 0,
                            position = Vector3.zero,
                            colorR = 0,
                            colorG = 0,
                            colorB = 0,
                            steepness = 0,
                            rivers = (tileArray[i].rivers[closest] ? 1 : 0) + (tileArray[i].rivers[secondClosest] ? 10 : 0) + (HasCornerRiver(tileArray[i], closest, secondClosest) ? 100 : 0),
                            gridHeight = 0
                        };
                        vertices.Add(vertex);
                    }
                }
            }
        }

        // add empty vertices to fill up the list to desired size
        for (int i = 0; i < vertices.Count % numberOfThreads; i++)
        {
            Vertex vertex = new Vertex()
            {
                tile = -1,
                tileX = 0,
                tileZ = 0,
                tileType = 0,
                tileType1 = 0,
                tileType2 = 0,
                x = 0,
                z = 0,
                elevation1 = 0,
                elevation2 = 0,
                position = Vector3.zero,
                colorR = 0,
                colorG = 0,
                colorB = 0,
                steepness = 0,
                rivers = 0,
                gridHeight = 0
            };
            vertices.Add(vertex);
        }

        Vertex[] vertexArray = vertices.ToArray();

        // send parameters to the shader
        SetShaderParameters();

        int maxthreadGrup = 65535;
        int batchSize = maxthreadGrup - maxthreadGrup % numberOfThreads;
        int vertexSize = sizeof(int) * 16 + sizeof(float) * 4;

        // if there is too many vertices, the data needs to be sent in batches
        if (vertexArray.Length / numberOfThreads > maxthreadGrup)
        {
            int batchesCount = Mathf.CeilToInt((float)vertexArray.Length / batchSize);
            List<Vertex[]> vertexArrays = new List<Vertex[]>();
            for (int i = 0; i < batchesCount; i++)
            {
                int arraySize = batchSize;
                Vertex[] splitArray = new Vertex[arraySize];
                for (int j = 0; j < arraySize; j++)
                {
                    if (i * batchSize + j < vertexArray.Length)
                    {
                        splitArray[j] = vertexArray[i * batchSize + j];
                    }
                    else
                    {
                        splitArray[j] = new Vertex()
                        {
                            tile = -1,
                            tileX = 0,
                            tileZ = 0,
                            x = 0,
                            z = 0,
                            position = Vector3.zero,
                            elevation1 = 0,
                            elevation2 = 0,
                        };
                    }
                }
                vertexArrays.Add(splitArray);
            }
            foreach (Vertex[] splitArray in vertexArrays)
            {
                // send vertices to compute shader to calculate postions
                ComputeBuffer vertexBuffer = new ComputeBuffer(splitArray.Length, vertexSize);
                vertexBuffer.SetData(splitArray);
                vertexShader.SetBuffer(0, "vertexBuffer", vertexBuffer);
                // compute vertices
                vertexShader.Dispatch(0, splitArray.Length / numberOfThreads, 1, 1);
                // receive vertices
                vertexBuffer.GetData(splitArray);
                vertexBuffer.Dispose();
            }
            int splitCount = 0;
            foreach (Vertex[] splitArray in vertexArrays)
            {
                for (int j = 0; j < batchSize; j++)
                {
                    if (splitCount * batchSize + j < vertexArray.Length)
                    {
                        vertexArray[splitCount * batchSize + j] = splitArray[j];
                    }
                }
                splitCount++;
            }
        }
        else
        {
            // send vertices to compute shader to calculate postions
            ComputeBuffer vertexBuffer = new ComputeBuffer(vertexArray.Length, vertexSize);
            vertexBuffer.SetData(vertexArray);
            vertexShader.SetBuffer(0, "vertexBuffer", vertexBuffer);
            // compute vertices
            vertexShader.Dispatch(0, vertexArray.Length / numberOfThreads, 1, 1);
            // receive vertices
            vertexBuffer.GetData(vertexArray);
            vertexBuffer.Dispose();
        }

        // create a structure that will hold a list of vertices for every tile
        List<Vector3>[] tileVertices = new List<Vector3>[tileArray.Length];
        List<Color32>[] tileColors = new List<Color32>[tileArray.Length];
        Vertex[][,] tileVerticesObjects = new Vertex[tileArray.Length][,];
        for (int i = 0; i < tileArray.Length; i++)
        {
            tileVertices[i] = new List<Vector3>();
            tileColors[i] = new List<Color32>();
            tileVerticesObjects[i] = new Vertex[pointsSize, pointsSize];
        }

        // iterate over now calculated vertices to populate the lists with positions and colors
        for (int i = 0; i < vertexArray.Length; i++)
        {
            Vertex vertex = vertexArray[i];
            if (vertex.tile >= 0)
            {
                tileVerticesObjects[vertex.tile][vertex.x, vertex.z] = vertex;
                tileVertices[vertex.tile].Add(vertex.position);
                Color32 vertexColor = new Color32((byte)vertex.colorR, (byte)vertex.colorG, (byte)vertex.colorB, 255);
                tileColors[vertex.tile].Add(vertexColor);
            }
        }

        // create an object for each tile
        for (int i = 0; i < tileArray.Length; i++)
        {
            Tile tile = tileArray[i];
            tile.vertices = tileVerticesObjects[i];

            // initiate tile object and its mesh
            GameObject tileObject = Instantiate(tilePrefab, tile.coordinates, tileRotation, tilesParent.transform);
            Mesh mesh = new Mesh()
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
            tileObject.GetComponent<MeshFilter>().mesh = mesh;
            tile.objectRef = tileObject;
            tile.CreateParentObjects();
            Renderer tileRenderer = tileObject.GetComponent<Renderer>();

            // add a simple collider to the tile
            if (tile.hasCollider)
            {
                // collider adjusted to tile size
                // GameObject colliderRef = Instantiate(tileColliderPrefab, tile.coordinates + new Vector3(0, tile.elevation * elevationHeight), Quaternion.identity, tileObject.transform);
                // colliderRef.transform.localScale = new Vector3(a, tile.elevation * elevationHeight, a);

                // same collider to all tiles
                GameObject colliderRef = Instantiate(tileColliderPrefab, tile.coordinates + new Vector3(0, 2 * elevationHeight), Quaternion.identity, tileObject.transform);
                colliderRef.transform.localScale = new Vector3(a, 2 * elevationHeight, a);

                colliderRef.GetComponent<TileColliderScript>().tile = tile;
                tile.colliderRef = colliderRef;
            }

            Vector3[] positionsArray = tileVertices[i].ToArray();
            Color32[] colorArray = tileColors[i].ToArray();
            int[] trianglesNew = triangles;

            if (!smoothShading)
            {
                // transform arrays in a way that will cause each triangle to be separate. This allows to get a low poly look (otherwise renderer would try to smooth out the mesh = no sharp edges)
                // source: https://answers.unity.com/questions/798510/flat-shading.html

                Vector3[] convertedPositionsArray = new Vector3[triangles.Length];
                Color32[] convertedColorArray = new Color32[triangles.Length];
                for (int j = 0; j < triangles.Length; j++)
                {
                    convertedPositionsArray[j] = positionsArray[triangles[j]];
                    convertedColorArray[j] = colorArray[triangles[j]];
                }
                positionsArray = convertedPositionsArray;
                trianglesNew = smoothTriangles;
                colorArray = convertedColorArray;
            }

            // assign mesh data to mesh and update it
            mesh.vertices = positionsArray;
            mesh.triangles = trianglesNew;
            mesh.colors32 = colorArray;
            mesh.name = "tile_mesh_" + tile.xIndex + "_" + tile.zIndex;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }

    private void SetShaderParameters()
    {
        vertexShader.SetFloat("xDistance", xDistance);
        vertexShader.SetFloat("zDistance", zDistance);
        vertexShader.SetFloats("startPoint", startPoint.x, startPoint.y, startPoint.z);
        vertexShader.SetInt("r", r);
        vertexShader.SetInt("pointsSize", pointsSize);
        vertexShader.SetInt("elevationBorderSizePoints", elevationBorderSizePoints);
        vertexShader.SetFloat("elevationHeight", elevationHeight);
        vertexShader.SetInt("perlinMax", perlinMax);
        vertexShader.SetInt("tilesZ", Tiles.tilesZ);
        vertexShader.SetFloat("layersValueMultiplier", layersValueMultiplier);
        vertexShader.SetFloat("layersDensityMultiplier", layersDensityMultiplier);
        vertexShader.SetInt("numberOfTileTypes", tilePropertyMapping.Count);
        vertexShader.SetInt("riverWidthPoints", riverWidthPoints);
        vertexShader.SetFloat("riverMaxDepth", riverMaxDepth);
        vertexShader.SetFloat("riverShapePower", riverShapePower);
        vertexShader.SetFloat("riverHeight", RiverRenderer.riverHeight);

        vertexShader.SetFloat("cliffNoiseStartSize", cliffNoiseStartSize);
        vertexShader.SetFloat("cliffNoiseDensity", cliffNoiseAdjustedDensity);
        vertexShader.SetInt("cliffNoiseLayers", cliffNoiseLayers);
        vertexShader.SetFloat("cliffNoiseValuePercentOnMildEdge", cliffNoiseValuePercentOnMildEdge);
        vertexShader.SetFloat("cliffNoiseMildingPower", cliffNoiseMildingPower);

        vertexShader.SetFloat("heightNoiseStartSize", heightNoiseStartSize);
        vertexShader.SetFloat("heightNoiseDensity", heightNoiseAdjustedDensity);
        vertexShader.SetInt("heightNoiseLayers", heightNoiseLayers);
        vertexShader.SetFloat("heightReductionPower1", heightReductionPower1);
        vertexShader.SetFloat("heightReductionPower2", heightReductionPower2);

        vertexShader.SetFloat("riverNoiseStartSize", riverNoiseStartSize);
        vertexShader.SetFloat("riverNoiseDensity", riverNoiseAdjustedDensity);
        vertexShader.SetInt("riverNoiseLayers", riverNoiseLayers);

        vertexShader.SetFloat("textureNoiseStartSize", textureNoiseStartSize);
        vertexShader.SetFloat("textureNoiseDensity", textureNoiseAdjustedDensity);
        vertexShader.SetInt("textureNoiseLayers", textureNoiseLayers);

        vertexShader.SetFloat("secondaryTextureNoiseMaxSize", secondaryTextureNoiseMaxSize);
        vertexShader.SetFloat("secondaryTextureNoiseStartSize", secondaryTextureNoiseStartSize);
        vertexShader.SetFloat("secondaryTextureNoiseDensity", secondaryTextureNoiseAdjustedDensity);
        vertexShader.SetInt("secondaryTextureNoiseLayers", secondaryTextureNoiseLayers);
        vertexShader.SetFloat("secondaryTextureNoiseSpreadPower", secondaryTextureNoiseSpreadPower);

        vertexShader.SetFloat("mixingNoiseMaxSize", mixingNoiseMaxSize);
        vertexShader.SetFloat("mixingNoiseStartSize", mixingNoiseStartSize);
        vertexShader.SetFloat("mixingNoiseDensity", mixingNoiseAdjustedDensity);
        vertexShader.SetInt("mixingNoiseLayers", mixingNoiseLayers);
        vertexShader.SetFloat("mixingPower", mixingPower);
        vertexShader.SetFloat("mixingNoiseRampingReduction", mixingNoiseRampingReduction);
        vertexShader.SetFloat("coastRampingReduction", coastRampingReduction);
        vertexShader.SetFloat("riverRampingReductionHeigher", riverRampingReductionHeigher);
        vertexShader.SetFloat("riverRampingReductionLower", riverRampingReductionLower);
        vertexShader.SetFloat("riverRampingReductionEqual", riverRampingReductionEqual);
        vertexShader.SetFloat("riverStrenghtMultiplierHeigher", riverStrenghtMultiplierHeigher);
        vertexShader.SetFloat("riverStrenghtMultiplierLower", riverStrenghtMultiplierLower);
        vertexShader.SetFloat("riverStrenghtMultiplierEqual", riverStrenghtMultiplierEqual);

        Color32 cliffPrimaryColorMin = cliffProperties.primaryColorMin;
        Color32 cliffPrimaryColorMax = cliffProperties.primaryColorMax;
        Color32 cliffSecondaryColor = cliffProperties.secondaryColor;
        Color32 coastPrimaryColorMin = coastProperties.primaryColorMin;
        Color32 coastPrimaryColorMax = coastProperties.primaryColorMax;
        Color32 coastSecondaryColor = coastProperties.secondaryColor;
        Color32 tileType0PrimaryColorMin = tilePropertyMapping[TileType.Grass].primaryColorMin;
        Color32 tileType0PrimaryColorMax = tilePropertyMapping[TileType.Grass].primaryColorMax;
        Color32 tileType0SecondaryColor = tilePropertyMapping[TileType.Grass].secondaryColor;
        Color32 tileType1PrimaryColorMin = tilePropertyMapping[TileType.Plain].primaryColorMin;
        Color32 tileType1PrimaryColorMax = tilePropertyMapping[TileType.Plain].primaryColorMax;
        Color32 tileType1SecondaryColor = tilePropertyMapping[TileType.Plain].secondaryColor;
        Color32 tileType2PrimaryColorMin = tilePropertyMapping[TileType.Rock].primaryColorMin;
        Color32 tileType2PrimaryColorMax = tilePropertyMapping[TileType.Rock].primaryColorMax;
        Color32 tileType2SecondaryColor = tilePropertyMapping[TileType.Rock].secondaryColor;
        Color32 tileType3PrimaryColorMin = tilePropertyMapping[TileType.Snow].primaryColorMin;
        Color32 tileType3PrimaryColorMax = tilePropertyMapping[TileType.Snow].primaryColorMax;
        Color32 tileType3SecondaryColor = tilePropertyMapping[TileType.Snow].secondaryColor;
        vertexShader.SetInts("cliffPrimaryColorMin", cliffPrimaryColorMin.r, cliffPrimaryColorMin.g, cliffPrimaryColorMin.b);
        vertexShader.SetInts("cliffPrimaryColorMax", cliffPrimaryColorMax.r, cliffPrimaryColorMax.g, cliffPrimaryColorMax.b);
        vertexShader.SetInts("cliffSecondaryColor", cliffSecondaryColor.r, cliffSecondaryColor.g, cliffSecondaryColor.b);
        vertexShader.SetInt("cliffLayerPriority", cliffProperties.layerPriority);
        vertexShader.SetInts("coastPrimaryColorMin", coastPrimaryColorMin.r, coastPrimaryColorMin.g, coastPrimaryColorMin.b);
        vertexShader.SetInts("coastPrimaryColorMax", coastPrimaryColorMax.r, coastPrimaryColorMax.g, coastPrimaryColorMax.b);
        vertexShader.SetInts("coastSecondaryColor", coastSecondaryColor.r, coastSecondaryColor.g, coastSecondaryColor.b);
        vertexShader.SetInt("coastLayerPriority", coastProperties.layerPriority);
        vertexShader.SetInts("tileType0PrimaryColorMin", tileType0PrimaryColorMin.r, tileType0PrimaryColorMin.g, tileType0PrimaryColorMin.b);
        vertexShader.SetInts("tileType0PrimaryColorMax", tileType0PrimaryColorMax.r, tileType0PrimaryColorMax.g, tileType0PrimaryColorMax.b);
        vertexShader.SetInts("tileType0SecondaryColor", tileType0SecondaryColor.r, tileType0SecondaryColor.g, tileType0SecondaryColor.b);
        vertexShader.SetInt("tileType0LayerPriority", tilePropertyMapping[TileType.Grass].layerPriority);
        vertexShader.SetInt("tileType0RockRiver", tilePropertyMapping[TileType.Grass].rockRiver ? (int) TileType.Rock : tilePropertyMapping.Count);
        vertexShader.SetInts("tileType1PrimaryColorMin", tileType1PrimaryColorMin.r, tileType1PrimaryColorMin.g, tileType1PrimaryColorMin.b);
        vertexShader.SetInts("tileType1PrimaryColorMax", tileType1PrimaryColorMax.r, tileType1PrimaryColorMax.g, tileType1PrimaryColorMax.b);
        vertexShader.SetInts("tileType1SecondaryColor", tileType1SecondaryColor.r, tileType1SecondaryColor.g, tileType1SecondaryColor.b);
        vertexShader.SetInt("tileType1LayerPriority", tilePropertyMapping[TileType.Plain].layerPriority);
        vertexShader.SetInt("tileType1RockRiver", tilePropertyMapping[TileType.Plain].rockRiver ? (int)TileType.Rock : tilePropertyMapping.Count);
        vertexShader.SetInts("tileType2PrimaryColorMin", tileType2PrimaryColorMin.r, tileType2PrimaryColorMin.g, tileType2PrimaryColorMin.b);
        vertexShader.SetInts("tileType2PrimaryColorMax", tileType2PrimaryColorMax.r, tileType2PrimaryColorMax.g, tileType2PrimaryColorMax.b);
        vertexShader.SetInts("tileType2SecondaryColor", tileType2SecondaryColor.r, tileType2SecondaryColor.g, tileType2SecondaryColor.b);
        vertexShader.SetInt("tileType2LayerPriority", tilePropertyMapping[TileType.Rock].layerPriority);
        vertexShader.SetInt("tileType2RockRiver", tilePropertyMapping[TileType.Rock].rockRiver ? (int)TileType.Rock : tilePropertyMapping.Count);
        vertexShader.SetInts("tileType3PrimaryColorMin", tileType3PrimaryColorMin.r, tileType3PrimaryColorMin.g, tileType3PrimaryColorMin.b);
        vertexShader.SetInts("tileType3PrimaryColorMax", tileType3PrimaryColorMax.r, tileType3PrimaryColorMax.g, tileType3PrimaryColorMax.b);
        vertexShader.SetInts("tileType3SecondaryColor", tileType3SecondaryColor.r, tileType3SecondaryColor.g, tileType3SecondaryColor.b);
        vertexShader.SetInt("tileType3LayerPriority", tilePropertyMapping[TileType.Snow].layerPriority);
        vertexShader.SetInt("tileType3RockRiver", tilePropertyMapping[TileType.Snow].rockRiver ? (int)TileType.Rock : tilePropertyMapping.Count);
    }
}
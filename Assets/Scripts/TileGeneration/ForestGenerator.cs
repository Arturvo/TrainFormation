using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestGenerator : MonoBehaviour
{
    public static readonly float treeRadius = 1.2f;
    public static readonly float treeScaleMultiplier = 0.6f;
    public static readonly float steepness1TreeRemoveChance = 0.5f;
    public static readonly float scaleVariety = 0.1f;
    public static readonly int forestVariations = 50;
    public static readonly int forestPrecision = 30;

    public static readonly float shadowDiameter = 1f;
    public static readonly float shadowHeight = 0.01f;
    public static readonly float shadowStretch = 1.5f;

    public GameObject shadowPrefab;
    public GameObject[] grassTrees;
    public GameObject[] plainTrees;

    public GameObject[] grassForest;
    public GameObject[] plainForest;


    private Dictionary<TileType, GameObject[]> tileTypeTreePrefabMapping;
    private Dictionary<TileType, GameObject[]> tileTypeForestPrefabMapping;

    // sets of spawn point to be used randomly by tiles
    private List<Vector2>[] spawnPoints;

    public void StartByScheduler()
    {
        tileTypeTreePrefabMapping = new Dictionary<TileType, GameObject[]>()
        {
            {TileType.Grass, grassTrees },
            {TileType.Plain, plainTrees }
        };
        tileTypeForestPrefabMapping = new Dictionary<TileType, GameObject[]>()
        {
            {TileType.Grass, grassForest },
            {TileType.Plain, plainForest }
        };

        //GenerateSpawnPoints();
    }


    public void GenerateForests(List<Tile> tiles)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.hasForest)
            {
                UnityEngine.Random.InitState(tile.xIndex + tile.zIndex);

                GameObject forest = null;
                // if tile has no rails generate full forest
                if (tile.rails == null)
                {
                    forest = Instantiate(tileTypeForestPrefabMapping[tile.type][0], tile.objectParents[Tile.ObjectParent.Forest].transform);
                    forest.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 5) * 60, 0);
                }
                // if there are staright rails generate forest with straight gap
                else if (tile.rails[0] == (tile.rails[1] + 3) % 6)
                {
                    forest = Instantiate(tileTypeForestPrefabMapping[tile.type][5], tile.objectParents[Tile.ObjectParent.Forest].transform);
                    forest.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0,2) == 0 ? (tile.rails[0] - 1) * 60 : (tile.rails[0] + 2) * 60, 0);
                }
                // if there are bent rails generate forest with bent gap
                else
                {
                    forest = Instantiate(tileTypeForestPrefabMapping[tile.type][6], tile.objectParents[Tile.ObjectParent.Forest].transform);
                    forest.transform.localRotation = Quaternion.Euler(0, Mathf.Abs(tile.rails[0]- tile.rails[1]) == 2 ? (Mathf.Min(tile.rails[0],tile.rails[1]) - 1) * 60 : (Mathf.Min(tile.rails[0], tile.rails[1]) -  3) * 60, 0);
                }
                forest.transform.localPosition = new Vector3(0, tile.elevation * TileGenerator.elevationHeight, 0);

                for (int i = 0; i < 6; i++)
                {
                    if (tile.neighbors[i] != null && tile.elevation == tile.neighbors[i].elevation && !tile.rivers[i])
                    {
                        int sideVariant = 1;

                        bool leftSideTaken = tile.rivers[(i + 5) % 6] || tile.neighbors[(i + 5) % 6] == null || tile.neighbors[(i + 5) % 6].elevation != tile.neighbors[i].elevation;
                        bool rightSideTaken = tile.rivers[(i + 1) % 6] || tile.neighbors[(i + 1) % 6] == null || tile.neighbors[(i + 1) % 6].elevation != tile.neighbors[i].elevation;

                        if (leftSideTaken && rightSideTaken) sideVariant = 4;
                        else if (leftSideTaken) sideVariant = 2;
                        else if (rightSideTaken) sideVariant = 3;

                        bool hasRail = tile.rails != null && (tile.rails[0] == i || tile.rails[1] == i);

                        GameObject forestSide = Instantiate(tileTypeForestPrefabMapping[tile.type][sideVariant + (hasRail ? 6 : 0)], tile.objectParents[Tile.ObjectParent.Forest].transform);
                        forestSide.transform.localPosition = new Vector3(0, tile.elevation * TileGenerator.elevationHeight, 0);
                        forestSide.transform.localRotation = Quaternion.Euler(0, i * 60, 0);
                    }
                }
            }
        }
    }
}

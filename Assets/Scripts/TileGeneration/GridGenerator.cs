using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public static float lineWidth = 0.15f;
    public static float lineAdjustment = 0.05f;
    public static Vector3 lineColor = new Vector3(0,0,0);

    public void GenerateGrid(List<Tile> tiles, bool gridEnabled)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.hasCollider && tile.zIndex < MapGenerator.oceanWidth + MapGenerator.mapWidth)
            {
                List<Vector3> gridPoints = new List<Vector3>();

                // start from the point at the bottom left of a hex
                (int currX, int currZ) = (TileGenerator.r, 0);

                // what change must be done to point indexes to get next point in the line for each edge of a hex
                (int xPlus, int zPlus)[] nextsSteps =
                    {
                    (1,0),
                    (0,1),
                    (-1,1),
                    (-1,0),
                    (0,-1),
                    (1,-1)
                };

                for (int edge = 0; edge < 6; edge++)
                {
                    for (int i = 0; i < TileGenerator.r; i++)
                    {
                        currX += nextsSteps[edge].xPlus;
                        currZ += nextsSteps[edge].zPlus;

                        gridPoints.Add(new Vector3(tile.vertices[currX, currZ].position.x, tile.vertices[currX, currZ].gridHeight + lineAdjustment, tile.vertices[currX, currZ].position.z));
                    }
                }

                LineRenderer lineRenderer = tile.objectRef.AddComponent<LineRenderer>();
                lineRenderer.loop = true;
                lineRenderer.useWorldSpace = false;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.widthMultiplier = lineWidth;
                lineRenderer.positionCount = gridPoints.Count;
                lineRenderer.sortingOrder = 1;
                Color color = new Color(lineColor.x / 255, lineColor.y / 255, lineColor.z / 255);
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                lineRenderer.SetPositions(gridPoints.ToArray());
                lineRenderer.enabled = gridEnabled;
            }
        }
    }
}

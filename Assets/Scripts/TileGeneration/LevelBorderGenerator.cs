using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBorderGenerator : MonoBehaviour
{
    public static float lineWidth = 0.4f;
    public static float lineAdjustment = 0.5f;
    public static Vector3 lineColor = new Vector3(99, 6, 0);

    public void GenerateLevelBorder(List<Tile> tiles)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.neighbors[4] != null && tile.segmentId > tile.neighbors[4].segmentId && tile.hasCollider && tile.zIndex < MapGenerator.oceanWidth + MapGenerator.mapWidth)
            {
                List<Vector3> gridPoints = new List<Vector3>();
                int currX; int currZ;
                int startEdge; int endEdge;

                if (tile.zIndex % 2 == 0)
                {
                    // start from the point at the bottom left of a hex
                    (currX, currZ) = (TileGenerator.r, 0);
                    (startEdge, endEdge) = (1, 2);
                }
                else
                {
                    // start from the point at the bottom of a hex
                    (currX, currZ) = (2 * TileGenerator.r, 0);
                    (startEdge, endEdge) = (0, 3);
                }


                // what change must be done to point indexes to get next point in the line for each edge of a hex
                (int xPlus, int zPlus)[] nextsSteps =
                    {
                    (-1,0),
                    (-1,1),
                    (0,1),
                };

                for (int edge = startEdge; edge < endEdge; edge++)
                {
                    for (int i = 0; i < TileGenerator.r; i++)
                    {
                        currX += nextsSteps[edge].xPlus;
                        currZ += nextsSteps[edge].zPlus;

                        gridPoints.Add(new Vector3(tile.vertices[currX, currZ].position.x, tile.vertices[currX, currZ].gridHeight + lineAdjustment, tile.vertices[currX, currZ].position.z));
                    }
                }

                LineRenderer lineRenderer = tile.objectParents[Tile.ObjectParent.LevelBorder].AddComponent<LineRenderer>();
                lineRenderer.loop = false;
                lineRenderer.useWorldSpace = false;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.widthMultiplier = lineWidth;
                lineRenderer.positionCount = gridPoints.Count;
                lineRenderer.sortingOrder = 1;
                Color color = new Color(lineColor.x / 255, lineColor.y / 255, lineColor.z / 255);
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                lineRenderer.SetPositions(gridPoints.ToArray());
                lineRenderer.enabled = true;
            }
        }
    }
}

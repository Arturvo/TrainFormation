using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailsGenerator : MonoBehaviour
{
    public GameObject rails;
    public GameObject station;
    private static float railLength = 0.523f;
    private static float railBaseElevation = 0.5f;
    public static float initialSteepness = 0.5f;
    private static int curveSegments = 22;
    private static float stationBaseElevation = 0.5f;

    public void GenerateRails(List<Tile> tiles)
    {
        // positions of the middle point of each edge of a tile
        Vector3[] edgePositions = new Vector3[]
        {
            new Vector3(0.5f * TileGenerator.h,0,0.75f * TileGenerator.a),
            new Vector3(TileGenerator.h,0,0),
            new Vector3(0.5f * TileGenerator.h,0,-0.75f * TileGenerator.a),
            new Vector3(-0.5f * TileGenerator.h,0,-0.75f * TileGenerator.a),
            new Vector3(-TileGenerator.h,0,0),
            new Vector3(-0.5f * TileGenerator.h,0,0.75f * TileGenerator.a)
        };

        // positions of the middle point of each edge of a tile after taking into account the part assigned to elevation and rivers
        Vector3[] internalEdgePositions = new Vector3[]
        {
            edgePositions[0] + ((edgePositions[3] - edgePositions[0]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[1] + ((edgePositions[4] - edgePositions[1]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[2] + ((edgePositions[5] - edgePositions[2]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[3] + ((edgePositions[0] - edgePositions[3]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[4] + ((edgePositions[1] - edgePositions[4]) * TileGenerator.elevationBorderSize)/2,
            edgePositions[5] + ((edgePositions[2] - edgePositions[5]) * TileGenerator.elevationBorderSize)/2,
        };

        foreach (Tile tile in tiles)
        {
            if (tile != null && tile.rails != null)
            {
                // generate border part of the rails if nearest tile has same elevation
                for (int railIndex = 0; railIndex < 2; railIndex++)
                {
                    int edge = tile.rails[railIndex];
                    if (tile.neighbors[edge] != null)
                    {
                        if (tile.neighbors[edge].elevation == tile.elevation)
                        {
                            GenerateRailsBetween2Points(tile, edge, false, edgePositions[edge], internalEdgePositions[edge]);
                        }
                        else if (tile.neighbors[edge].elevation > tile.elevation)
                        {
                            GenerateRailsBetween2Points(tile, edge, false, edgePositions[edge] + new Vector3(0,TileGenerator.elevationHeight * initialSteepness, 0), internalEdgePositions[edge]);
                        }
                        else if (tile.elevation - tile.neighbors[edge].elevation == 1)
                        {
                            GenerateRailsBetween2Points(tile, edge, true, edgePositions[edge] - new Vector3(0, TileGenerator.elevationHeight * (1 - initialSteepness), 0), internalEdgePositions[edge]);
                        }
                    }
                }

                // generate straight middle fragment of rails if rails are straight
                if (tile.rails[0] == (tile.rails[1] + 3) % 6)
                {
                    GenerateRailsBetween2Points(tile, tile.rails[0], false, internalEdgePositions[tile.rails[1]], internalEdgePositions[tile.rails[0]]);
                }
                // generate curved rail fragment
                else
                {
                    Vector3 position1 = internalEdgePositions[tile.rails[0]];
                    Vector3 position2 = Vector3.zero;
                    Vector3 position3 = internalEdgePositions[tile.rails[1]];
                    for (int i = 0; i < curveSegments; i++)
                    {
                        Vector3 position12 = Vector3.Lerp(position1, position2, (float)i / (curveSegments-1));
                        Vector3 position23 = Vector3.Lerp(position2, position3, (float)i / (curveSegments - 1));
                        Vector3 position13 = Vector3.Lerp(position12, position23, (float)i / (curveSegments - 1));

                        Vector3 railsPosition = position13 + tile.coordinates + new Vector3(0, tile.elevation * TileGenerator.elevationHeight + railBaseElevation, 0);
                        
                        Quaternion rotation1 = Quaternion.Euler(new Vector3(0, (tile.rails[0] - 1) * 60, 0));
                        Quaternion rotation2 = Quaternion.Euler(new Vector3(0, (tile.rails[1] + 2) * 60, 0));
                        Quaternion railsRoation = Quaternion.Lerp(rotation1, rotation2, (float)i / (curveSegments - 1));

                        Instantiate(rails, railsPosition, railsRoation, tile.objectParents[Tile.ObjectParent.Rails].transform);
                    }
                }

                if (tile.hasStation)
                {
                    Instantiate(station, tile.coordinates + new Vector3(0,tile.elevation * TileGenerator.elevationHeight + stationBaseElevation,0), Quaternion.identity, tile.objectParents[Tile.ObjectParent.TrainStation].transform);
                }
            }
        }
    }

    private void GenerateRailsBetween2Points(Tile tile, int orientationEdge, bool down, Vector3 point1, Vector3 point2)
    {
        int railNumber = Mathf.CeilToInt(Vector3.Distance(point1, point2) / railLength);
        Vector3 railsVector = (point2 - point1).normalized;
        float yRotation = (orientationEdge - 1) * 60;

        float tangZRotation = Mathf.Abs(point1.y - point2.y) / Vector3.Distance(new Vector3(point1.x, 0, point1.z), new Vector3(point2.x, 0, point2.z));
        float zRotation = Mathf.Atan(tangZRotation) * Mathf.Rad2Deg * (down ? -1 : 1);

        Quaternion railsRoation = Quaternion.Euler(new Vector3(0, yRotation, zRotation));
        for (int i = 0; i < railNumber; i++)
        {
            Vector3 railsPosition = point1 + railsVector * i * railLength + tile.coordinates + new Vector3(0, tile.elevation * TileGenerator.elevationHeight + railBaseElevation, 0);
            Instantiate(rails, railsPosition, railsRoation, tile.objectParents[Tile.ObjectParent.Rails].transform);
        }
    }
}

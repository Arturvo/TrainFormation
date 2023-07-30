using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverRenderer : MonoBehaviour
{
    public GameObject river;
    public GameObject waterfallPrefab;
    public GameObject waterfallBasePrefab;
    public static float riverHeight = 0.05f;
    public static float waterfallYAdjustment = -0.05f;
    public static float waterfallYRotationAdjustment = -10;
    public static float waterfallLifeTimePerElevation = 1;

    private Vector3[] cornerPositions;

    // parameters for waterfall ocean entry from 1 elevation high
    public static float waterfallOceanLifeTime = 0.4f;
    public static float waterfallOceanMinSize = 10;
    public static float waterfallOceanMaxSize = 16;
    public static float waterfallOceanRate = 200;
    public static float warerfallOceanMoveBack = 0.4f;
    public static float warerfallOceanScaleY = 0.4f;

    public void StartByScheduler()
    {
        cornerPositions = new Vector3[]
        {
            new Vector3(0.5f * TileGenerator.a, 0, TileGenerator.h),
            new Vector3(TileGenerator.a, 0, 0),
            new Vector3(0.5f * TileGenerator.a, 0, -TileGenerator.h),
            new Vector3(-0.5f * TileGenerator.a, 0, -TileGenerator.h),
            new Vector3(-TileGenerator.a, 0, 0),
            new Vector3(-0.5f * TileGenerator.a, 0, TileGenerator.h),
        };
    }    

    public void RenderRivers(List<Tile> tiles)
    {
        List<Waterfall> waterfalls = new List<Waterfall>();

        foreach(Tile tile in tiles)
        {
            if (tile != null && tile.elevation > 0)
            {
                Transform riverParent = tile.objectParents[Tile.ObjectParent.Rivers].transform;

                for (int i = 0; i < 6; i++)
                {
                    List<(Vector3[], int[])> vertexData = new List<(Vector3[], int[])>();

                    if (tile.rivers[i])
                    {
                        if (tile.neighbors[i].elevation == tile.elevation && tile.neighbors[(i + 1) % 6] != null && tile.neighbors[(i + 1) % 6].elevation >= tile.elevation && tile.neighbors[(i + 5) % 6] != null && tile.neighbors[(i + 5) % 6].elevation >= tile.elevation)
                        {
                            vertexData.Add((
                                new Vector3[]
                                {
                                    cornerPositions[(i + 5) % 6] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r),
                                    cornerPositions[i] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r),
                                    cornerPositions[(i + 5) % 6],
                                    cornerPositions[i]
                                },
                                new int[] { 0, 2, 3, 0, 3, 1 }
                            ));
                        }
                        if (tile.neighbors[i].elevation == tile.elevation
                            && (tile.neighbors[(i + 1) % 6] == null || tile.neighbors[(i + 1) % 6].elevation < tile.elevation)
                            && (tile.neighbors[(i + 5) % 6] == null || tile.neighbors[(i + 5) % 6].elevation < tile.elevation)
                            && (tile.neighbors[(i + 1) % 6] == null || tile.rivers[(i + 1) % 6] || tile.neighbors[(i + 1) % 6].rivers[(i + 5) % 6])
                            && (tile.neighbors[(i + 5) % 6] == null || tile.rivers[(i + 5) % 6] || tile.neighbors[(i + 5) % 6].rivers[(i + 1) % 6])
                            )
                        {
                            Vector3 middlePoint1 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                            Vector3 middlePoint2 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                            float heightDifference1 = (tile.elevation - (tile.neighbors[(i + 1) % 6] == null ? 0 : tile.neighbors[(i + 1) % 6].elevation)) * TileGenerator.elevationHeight;
                            Vector3 waterFallStart1 = cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance;
                            Vector3 waterFallEnd1 = cornerPositions[i] - new Vector3(0, heightDifference1, 0);

                            vertexData.Add((
                                new Vector3[]
                                {
                                    middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                    cornerPositions[i] + (cornerPositions[(i + 1) % 6] - cornerPositions[i]) * ((float)(TileGenerator.riverWidthPoints) / TileGenerator.r) - new Vector3(0, heightDifference1, 0),
                                    waterFallStart1,
                                    waterFallEnd1
                                },
                                new int[] { 0, 2, 3, 0, 3, 1 }
                            ));

                            if (i < 3)
                            {
                                waterfalls.Add(new Waterfall()
                                    {
                                        start = waterFallStart1,
                                        end = waterFallEnd1,
                                        tile = tile
                                    });
                            }

                            Vector3 middlePoint3 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                            Vector3 middlePoint4 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                            float heightDifference2 = (tile.elevation - (tile.neighbors[(i + 5) % 6] == null ? 0 : tile.neighbors[(i + 5) % 6].elevation)) * TileGenerator.elevationHeight;
                            Vector3 waterFallStart2 = cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance;
                            Vector3 waterFallEnd2 = cornerPositions[(i + 5) % 6] - new Vector3(0, heightDifference2, 0);
                            vertexData.Add((
                                new Vector3[]
                                {
                                    cornerPositions[(i + 5) % 6] + (cornerPositions[(i + 4) % 6] - cornerPositions[(i + 5) % 6]) * ((float)(TileGenerator.riverWidthPoints) / TileGenerator.r) - new Vector3(0, heightDifference2, 0),
                                    middlePoint3 + (middlePoint4 - middlePoint3).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                    waterFallEnd2,
                                    waterFallStart2
                                },
                                new int[] { 0, 2, 3, 0, 3, 1 }
                            ));

                            if (i < 3)
                            {
                                waterfalls.Add(new Waterfall()
                                    {
                                        start = waterFallStart2,
                                        end = waterFallEnd2,
                                        tile = tile
                                    });
                            }

                            vertexData.Add((
                                new Vector3[]
                                {
                                    middlePoint3 + (middlePoint4 - middlePoint3).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                    middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                    cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance,
                                    cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance
                                },
                                new int[] { 0, 2, 3, 0, 3, 1 }
                            ));
                        }
                        else
                        {
                            if (tile.neighbors[i].elevation == tile.elevation && (tile.neighbors[(i + 1) % 6] == null || tile.neighbors[(i + 1) % 6].elevation < tile.elevation))
                            {
                                Vector3 middlePoint1 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                                Vector3 middlePoint2 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                                if ((tile.neighbors[(i + 1) % 6] == null || (tile.neighbors[(i + 1) % 6].elevation < tile.elevation && (tile.rivers[(i + 1) % 6] || tile.neighbors[(i + 1) % 6].rivers[(i + 5) % 6])))
                                    || ((tile.neighbors[(i + 5) % 6] != null && tile.neighbors[(i + 5) % 6].elevation >= tile.elevation)))
                                {
                                    if (tile.neighbors[(i + 5) % 6] != null && tile.neighbors[(i + 5) % 6].elevation < tile.elevation && !tile.rivers[(i + 5) % 6] && !tile.neighbors[(i + 5) % 6].rivers[(i + 1) % 6])
                                    {
                                        vertexData.Add((
                                            new Vector3[]
                                            {
                                                middlePoint2 + (middlePoint1 - middlePoint2).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance,
                                                cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance
                                            },
                                            new int[] { 0, 2, 3, 0, 3, 1 }
                                        ));
                                    }
                                    else
                                    {
                                        vertexData.Add((
                                            new Vector3[]
                                            {
                                                cornerPositions[(i + 5) % 6] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r),
                                                middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                cornerPositions[(i + 5) % 6],
                                                cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance
                                            },
                                            new int[] { 0, 2, 3, 0, 3, 1 }
                                        ));
                                    }
                                }
                                if (tile.neighbors[(i + 1) % 6] == null || tile.neighbors[(i + 1) % 6].isOcean || (tile.neighbors[(i + 1) % 6].elevation < tile.elevation && (tile.rivers[(i + 1) % 6] || tile.neighbors[(i + 1) % 6].rivers[(i + 5) % 6])))
                                {
                                    float heightDifference = (tile.elevation - (tile.neighbors[(i + 1) % 6] == null ? 0 : tile.neighbors[(i + 1) % 6].elevation)) * TileGenerator.elevationHeight;
                                    Vector3 waterFallStart = cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance;
                                    Vector3 waterFallEnd = cornerPositions[i] - new Vector3(0, heightDifference, 0);
                                    vertexData.Add((
                                        new Vector3[]
                                        {
                                            middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                            cornerPositions[i] + (cornerPositions[(i + 1) % 6] - cornerPositions[i]) * ((float)(TileGenerator.riverWidthPoints) / TileGenerator.r) - new Vector3(0, heightDifference, 0),
                                            waterFallStart,
                                            waterFallEnd
                                        },
                                        new int[] { 0, 2, 3, 0, 3, 1 }
                                    ));

                                    if (i < 3)
                                    {
                                        waterfalls.Add(new Waterfall()
                                            {
                                                start = waterFallStart,
                                                end = waterFallEnd,
                                                tile = tile
                                            });
                                    }
                                }
                            }
                            if (tile.neighbors[i].elevation == tile.elevation && (tile.neighbors[(i + 5) % 6] == null || tile.neighbors[(i + 5) % 6].elevation < tile.elevation))
                            {
                                Vector3 middlePoint1 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                                Vector3 middlePoint2 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                                if ((tile.neighbors[(i + 5) % 6] == null || (tile.neighbors[(i + 5) % 6].elevation < tile.elevation && (tile.rivers[(i + 5) % 6] || tile.neighbors[(i + 5) % 6].rivers[(i + 1) % 6])))
                                    || (tile.neighbors[(i + 1) % 6] != null && tile.neighbors[(i + 1) % 6].elevation >= tile.elevation))
                                {
                                    if (tile.neighbors[(i + 1) % 6] != null && tile.neighbors[(i + 1) % 6].elevation < tile.elevation && !tile.rivers[(i + 1) % 6] && !tile.neighbors[(i + 1) % 6].rivers[(i + 5) % 6])
                                    {
                                        vertexData.Add((
                                            new Vector3[]
                                            {
                                                middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                middlePoint2 + (middlePoint1 - middlePoint2).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance,
                                                cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance,
                                            },
                                            new int[] { 0, 2, 3, 0, 3, 1 }
                                        ));
                                    }
                                    else
                                    {
                                        vertexData.Add((
                                            new Vector3[]
                                            {
                                                middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                cornerPositions[i] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r),
                                                cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance,
                                                cornerPositions[i]
                                            },
                                            new int[] { 0, 2, 3, 0, 3, 1 }
                                        ));
                                    }
                                }
                                if (tile.neighbors[(i + 5) % 6] == null || tile.neighbors[(i + 5) % 6].isOcean || (tile.neighbors[(i + 5) % 6].elevation < tile.elevation && (tile.rivers[(i + 5) % 6] || tile.neighbors[(i + 5) % 6].rivers[(i + 1) % 6])))
                                {
                                    float heightDifference = (tile.elevation - (tile.neighbors[(i + 5) % 6] == null ? 0 : tile.neighbors[(i + 5) % 6].elevation)) * TileGenerator.elevationHeight;
                                    Vector3 waterFallStart = cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]).normalized * TileGenerator.elevationBorderSizePoints * TileGenerator.xDistance;
                                    Vector3 waterFallEnd = cornerPositions[(i + 5) % 6] - new Vector3(0, heightDifference, 0);
                                    vertexData.Add((
                                        new Vector3[]
                                        {
                                            cornerPositions[(i + 5) % 6] + (cornerPositions[(i + 4) % 6] - cornerPositions[(i + 5) % 6]) * ((float)(TileGenerator.riverWidthPoints) / TileGenerator.r) - new Vector3(0, heightDifference, 0),
                                            middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                            waterFallEnd,
                                            waterFallStart
                                        },
                                        new int[] { 0, 2, 3, 0, 3, 1 }
                                    ));

                                    if (i < 3)
                                    {
                                        waterfalls.Add(new Waterfall()
                                            {
                                                start = waterFallStart,
                                                end = waterFallEnd,
                                                tile = tile
                                            });
                                    }
                                }
                            }
                        }
                        if (tile.neighbors[i].elevation > tile.elevation && tile.neighbors[(i + 1) % 6] != null && tile.neighbors[(i + 1) % 6].elevation >= tile.elevation && tile.neighbors[(i + 5) % 6] != null && tile.neighbors[(i + 5) % 6].elevation >= tile.elevation)
                        {
                            vertexData.Add((
                                new Vector3[]
                                {
                                    cornerPositions[(i + 5) % 6] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                    cornerPositions[i] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                    cornerPositions[(i + 5) % 6],
                                    cornerPositions[i]
                                },
                                new int[] { 0, 2, 3, 0, 3, 1 }
                            ));
                        }
                        if (tile.neighbors[i].elevation > tile.elevation 
                            && (tile.neighbors[(i + 1) % 6] == null || tile.neighbors[(i + 1) % 6].elevation < tile.elevation) 
                            && (tile.neighbors[(i + 5) % 6] == null || tile.neighbors[(i + 5) % 6].elevation < tile.elevation)
                            && (tile.neighbors[(i + 1) % 6] == null || tile.rivers[(i + 1) % 6] || tile.neighbors[(i + 1) % 6].rivers[(i + 5) % 6])
                            && (tile.neighbors[(i + 5) % 6] == null || tile.rivers[(i + 5) % 6] || tile.neighbors[(i + 5) % 6].rivers[(i + 1) % 6])
                            )
                        {
                            Vector3 middlePoint1 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r);
                            Vector3 middlePoint2 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r);
                            float heightDifference1 = (tile.elevation - (tile.neighbors[(i + 1) % 6] == null ? 0 : tile.neighbors[(i + 1) % 6].elevation)) * TileGenerator.elevationHeight;
                            Vector3 position1 = middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance);
                            Vector3 position2 = cornerPositions[i] + (cornerPositions[(i + 1) % 6] - cornerPositions[i]) * ((2f * TileGenerator.riverWidthPoints) / TileGenerator.r) - new Vector3(0, heightDifference1, 0);
                            Vector3 position3 = cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]) * ((float)TileGenerator.elevationBorderSizePoints / TileGenerator.r);
                            Vector3 position4 = cornerPositions[i] - new Vector3(0, heightDifference1, 0);
                            vertexData.Add((
                                new Vector3[]
                                {
                                    position1,
                                    position2,
                                    position3,
                                    position4
                                },
                                new int[] { 0, 2, 3, 0, 3, 1 }
                            ));

                            waterfalls.Add(new Waterfall()
                                {
                                    start = (position1 + position3) / 2,
                                    end = (position2 + position4) / 2,
                                    tile = tile
                                });

                            Vector3 middlePoint3 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r);
                            Vector3 middlePoint4 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r);
                            float heightDifference2 = (tile.elevation - (tile.neighbors[(i + 5) % 6] == null ? 0 : tile.neighbors[(i + 5) % 6].elevation)) * TileGenerator.elevationHeight;
                            Vector3 position5 = cornerPositions[(i + 5) % 6] + (cornerPositions[(i + 4) % 6] - cornerPositions[(i + 5) % 6]) * ((2f * TileGenerator.riverWidthPoints) / TileGenerator.r) - new Vector3(0, heightDifference2, 0);
                            Vector3 position6 = middlePoint3 + (middlePoint4 - middlePoint3).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance);
                            Vector3 position7 = cornerPositions[(i + 5) % 6] - new Vector3(0, heightDifference2, 0);
                            Vector3 position8 = cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]) * ((float)TileGenerator.elevationBorderSizePoints / TileGenerator.r);
                            vertexData.Add((
                                new Vector3[]
                                {
                                    position5,
                                    position6,
                                    position7,
                                    position8,
                                },
                                new int[] { 0, 2, 3, 0, 3, 1 }
                            ));

                            waterfalls.Add(new Waterfall()
                                {
                                    start = (position6 + position8) / 2,
                                    end = (position5 + position7) / 2,
                                    tile = tile
                                });

                            vertexData.Add((
                                new Vector3[]
                                {
                                    middlePoint3 + (middlePoint4 - middlePoint3).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                    middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                    cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]) * ((float) TileGenerator.elevationBorderSizePoints / TileGenerator.r),
                                    cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]) * ((float) TileGenerator.elevationBorderSizePoints / TileGenerator.r)
                                },
                                new int[] { 0, 2, 3, 0, 3, 1 }
                            ));
                        }
                        else
                        {
                            if (tile.neighbors[i].elevation > tile.elevation && (tile.neighbors[(i + 1) % 6] == null || tile.neighbors[(i + 1) % 6].elevation < tile.elevation))
                            {
                                Vector3 middlePoint1 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r);
                                Vector3 middlePoint2 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r);
                                if ((tile.neighbors[(i + 1) % 6] == null || (tile.neighbors[(i + 1) % 6].elevation < tile.elevation && (tile.rivers[(i + 1) % 6] || tile.neighbors[(i + 1) % 6].rivers[(i + 5) % 6])))
                                    || (tile.neighbors[(i + 5) % 6] != null && tile.neighbors[(i + 5) % 6].elevation >= tile.elevation))
                                {
                                    if (tile.neighbors[(i + 5) % 6] != null && tile.neighbors[(i + 5) % 6].elevation < tile.elevation && !tile.rivers[(i + 5) % 6] && !tile.neighbors[(i + 5) % 6].rivers[(i + 1) % 6])
                                    {
                                        vertexData.Add((
                                            new Vector3[]
                                            {
                                                cornerPositions[(i + 5) % 6] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                                middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]) * ((float) TileGenerator.elevationBorderSizePoints / TileGenerator.r),
                                                cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]) * ((float) TileGenerator.elevationBorderSizePoints / TileGenerator.r)
                                            },
                                            new int[] { 0, 2, 3, 0, 3, 1 }
                                        ));
                                    }
                                    else
                                    {
                                        vertexData.Add((
                                            new Vector3[]
                                            {
                                                cornerPositions[(i + 5) % 6] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                                middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                cornerPositions[(i + 5) % 6],
                                                cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]) * ((float) TileGenerator.elevationBorderSizePoints / TileGenerator.r)
                                            },
                                            new int[] { 0, 2, 3, 0, 3, 1 }
                                        ));
                                    }
                                }
                                if (tile.neighbors[(i + 1) % 6] == null || tile.neighbors[(i + 1) % 6].isOcean || (tile.neighbors[(i + 1) % 6].elevation < tile.elevation && (tile.rivers[(i + 1) % 6] || tile.neighbors[(i + 1) % 6].rivers[(i + 5) % 6])))
                                {
                                    float heightDifference = (tile.elevation - (tile.neighbors[(i + 1) % 6] == null ? 0 : tile.neighbors[(i + 1) % 6].elevation)) * TileGenerator.elevationHeight;
                                    Vector3 position1 = middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance);
                                    Vector3 position2 = cornerPositions[i] + (cornerPositions[(i + 1) % 6] - cornerPositions[i]) * ((2f * TileGenerator.riverWidthPoints) / TileGenerator.r) - new Vector3(0, heightDifference, 0);
                                    Vector3 position3 = cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]) * ((float)TileGenerator.elevationBorderSizePoints / TileGenerator.r);
                                    Vector3 position4 = cornerPositions[i] - new Vector3(0, heightDifference, 0);
                                    vertexData.Add((
                                        new Vector3[]
                                        {
                                            position1,
                                            position2,
                                            position3,
                                            position4
                                        },
                                        new int[] { 0, 2, 3, 0, 3, 1 }
                                    ));

                                    waterfalls.Add(new Waterfall()
                                    {
                                        start = (position1 + position3)/2,
                                        end = (position2 + position4) / 2,
                                        tile = tile
                                    });
                                }
                            }
                            if (tile.neighbors[i].elevation > tile.elevation && (tile.neighbors[(i + 5) % 6] == null || tile.neighbors[(i + 5) % 6].elevation < tile.elevation))
                            {
                                Vector3 middlePoint1 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r);
                                Vector3 middlePoint2 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r);
                                if ((tile.neighbors[(i + 5) % 6] == null || (tile.neighbors[(i + 5) % 6].elevation < tile.elevation && (tile.rivers[(i + 5) % 6] || tile.neighbors[(i + 5) % 6].rivers[(i + 1) % 6])))
                                    || (tile.neighbors[(i + 1) % 6] != null && tile.neighbors[(i + 1) % 6].elevation >= tile.elevation))
                                {
                                    if (tile.neighbors[(i + 1) % 6] != null && tile.neighbors[(i + 1) % 6].elevation < tile.elevation && !tile.rivers[(i + 1) % 6] && !tile.neighbors[(i + 1) % 6].rivers[(i + 5) % 6])
                                    {
                                        vertexData.Add((
                                            new Vector3[]
                                            {
                                                middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                cornerPositions[i] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                                cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]) * ((float) TileGenerator.elevationBorderSizePoints / TileGenerator.r),
                                                cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]) * ((float) TileGenerator.elevationBorderSizePoints / TileGenerator.r)
                                            },
                                            new int[] { 0, 2, 3, 0, 3, 1 }
                                        ));
                                    }
                                    else
                                    {
                                        vertexData.Add((
                                            new Vector3[]
                                            {
                                                middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance),
                                                cornerPositions[i] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                                cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]) * ((float) TileGenerator.elevationBorderSizePoints / TileGenerator.r),
                                                cornerPositions[i]
                                            },
                                            new int[] { 0, 2, 3, 0, 3, 1 }
                                        ));
                                    }
                                }
                                if (tile.neighbors[(i + 5) % 6] == null || tile.neighbors[(i + 5) % 6].isOcean || (tile.neighbors[(i + 5) % 6].elevation < tile.elevation && (tile.rivers[(i + 5) % 6] || tile.neighbors[(i + 5) % 6].rivers[(i + 1) % 6])))
                                {
                                    float heightDifference = (tile.elevation - (tile.neighbors[(i + 5) % 6] == null ? 0 : tile.neighbors[(i + 5) % 6].elevation)) * TileGenerator.elevationHeight;
                                    Vector3 position1 = cornerPositions[(i + 5) % 6] + (cornerPositions[(i + 4) % 6] - cornerPositions[(i + 5) % 6]) * ((2f * TileGenerator.riverWidthPoints) / TileGenerator.r) - new Vector3(0, heightDifference, 0);
                                    Vector3 position2 = middlePoint1 + (middlePoint2 - middlePoint1).normalized * ((TileGenerator.elevationBorderSizePoints - 2 * TileGenerator.riverWidthPoints) * TileGenerator.xDistance);
                                    Vector3 position3 = cornerPositions[(i + 5) % 6] - new Vector3(0, heightDifference, 0);
                                    Vector3 position4 = cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]) * ((float)TileGenerator.elevationBorderSizePoints / TileGenerator.r);
                                    vertexData.Add((
                                        new Vector3[]
                                        {
                                            position1,
                                            position2,
                                            position3,
                                            position4,
                                        },
                                        new int[] { 0, 2, 3, 0, 3, 1 }
                                    ));

                                    waterfalls.Add(new Waterfall()
                                    {
                                        start = (position2 + position4) / 2,
                                        end = (position1 + position3) / 2,
                                        tile = tile
                                    });
                                }
                            }
                        }
                        if (tile.neighbors[i].elevation == tile.elevation && tile.rivers[(i + 5) % 6] && tile.neighbors[(i + 5) % 6].elevation > tile.elevation)
                        {
                            Vector3 middlePoint1 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                            Vector3 middlePoint2 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                            vertexData.Add((
                                new Vector3[]
                                {
                                    cornerPositions[(i + 5) % 6] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                    middlePoint1,
                                    middlePoint1 + (middlePoint2 - middlePoint1) * ((float)(TileGenerator.riverWidthPoints) / TileGenerator.r)
                                },
                                new int[] { 0, 1, 2 }
                            ));
                        }
                        if (tile.neighbors[i].elevation == tile.elevation && tile.rivers[(i + 1) % 6] && tile.neighbors[(i + 1) % 6].elevation > tile.elevation)
                        {
                            Vector3 middlePoint1 = cornerPositions[i] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                            Vector3 middlePoint2 = cornerPositions[(i + 5) % 6] * ((float)(TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r);
                            vertexData.Add((
                                new Vector3[]
                                {
                                    cornerPositions[i] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                    middlePoint1 + (middlePoint2 - middlePoint1) * ((float)(TileGenerator.riverWidthPoints) / TileGenerator.r),
                                    middlePoint1
                                },
                                new int[] { 0, 1, 2 }
                            ));
                        }
                    }
                    if (!tile.rivers[i] && tile.rivers[(i + 1) % 6])
                    {
                        if (tile.neighbors[(i + 1) % 6].elevation == tile.elevation && tile.neighbors[i] != null && tile.neighbors[i].elevation >= tile.elevation)
                        {
                            vertexData.Add((
                                new Vector3[]
                                {
                                    cornerPositions[i] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r),
                                    cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]) * ((float) TileGenerator.riverWidthPoints / TileGenerator.r),
                                    cornerPositions[i]
                                },
                                new int[] { 0, 1, 2 }
                            ));
                        }
                        if (tile.neighbors[(i + 1) % 6].elevation > tile.elevation && tile.neighbors[i] != null && tile.neighbors[i].elevation >= tile.elevation)
                        {
                            vertexData.Add((
                                new Vector3[]
                                {
                                    cornerPositions[i] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                    cornerPositions[i] + (cornerPositions[(i + 5) % 6] - cornerPositions[i]) * ((float) (TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                    cornerPositions[i]
                                },
                                new int[] { 0, 1, 2 }
                            ));
                        }
                    }
                    if (!tile.rivers[i] && tile.rivers[(i + 5) % 6])
                    {
                        if (tile.neighbors[(i + 5) % 6].elevation == tile.elevation && tile.neighbors[i] != null && tile.neighbors[i].elevation >= tile.elevation)
                        {
                            vertexData.Add((
                                new Vector3[]
                                {
                                    cornerPositions[(i + 5) % 6] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints) / TileGenerator.r),
                                    cornerPositions[(i + 5) % 6],
                                    cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]) * ((float) TileGenerator.riverWidthPoints / TileGenerator.r)
                                },
                                new int[] { 0, 1, 2 }
                            ));
                        }
                        if (tile.neighbors[(i + 5) % 6].elevation > tile.elevation && tile.neighbors[i] != null && tile.neighbors[i].elevation >= tile.elevation)
                        {
                            vertexData.Add((
                                new Vector3[]
                                {
                                    cornerPositions[(i + 5) % 6] * ((float) (TileGenerator.r - TileGenerator.riverWidthPoints * 2) / TileGenerator.r),
                                    cornerPositions[(i + 5) % 6],
                                    cornerPositions[(i + 5) % 6] + (cornerPositions[i] - cornerPositions[(i + 5) % 6]) * ((float) (TileGenerator.riverWidthPoints * 2) / TileGenerator.r)
                                },
                                new int[] { 0, 1, 2 }
                            ));
                        }
                    }

                    foreach ((Vector3[] vertices, int[] triangles) in vertexData)
                    {
                        GameObject riverRef = Instantiate(river, riverParent);
                        riverRef.transform.localPosition = new Vector3(0, tile.elevation * TileGenerator.elevationHeight + riverHeight, 0);
                        Mesh mesh = new Mesh();
                        riverRef.GetComponent<MeshFilter>().mesh = mesh;
                        mesh.vertices = vertices;
                        mesh.triangles = triangles;
                        mesh.RecalculateBounds();
                        mesh.RecalculateNormals();
                    }
                }
            }
        }

        RenderWaterfalls(waterfalls);
    }

    private class Waterfall
    {
        public Vector3 start;
        public Vector3 end;
        public Tile tile;
    }

    private void RenderWaterfalls(List<Waterfall> waterfalls)
    {
        foreach (Waterfall waterfall in waterfalls)
        {
            Transform parent = waterfall.tile.objectParents[Tile.ObjectParent.Rivers].transform;
            Vector3 start = waterfall.start + new Vector3(0, waterfall.tile.elevation * TileGenerator.elevationHeight, 0);
            Vector3 end = waterfall.end + new Vector3(0, waterfall.tile.elevation * TileGenerator.elevationHeight, 0);
            bool hitsOcean = false;

            if (end.y < OceanGenerator.oceanHeight)
            {
                end += (start - end) * ((OceanGenerator.oceanHeight - end.y) / (start.y - end.y));
                hitsOcean = true;
            }

            GameObject waterfallRef = Instantiate(waterfallPrefab, parent);
            waterfallRef.transform.localPosition = start + new Vector3(0, waterfallYAdjustment, 0);
            waterfallRef.transform.localRotation = Quaternion.LookRotation(end - start);
            waterfallRef.transform.localEulerAngles += new Vector3(0, waterfallYRotationAdjustment, 0);

            GameObject waterfallBaseRef = Instantiate(waterfallBasePrefab, parent);
            waterfallBaseRef.transform.localPosition = end;
            waterfallBaseRef.transform.localScale *= TileGenerator.a;
            waterfallRef.GetComponent<ParticleSystem>().collision.SetPlane(0, waterfallBaseRef.transform);
            ParticleSystem.MainModule main = waterfallRef.GetComponent<ParticleSystem>().main;
            float startLifetime = waterfallLifeTimePerElevation * ((start.y - end.y) / TileGenerator.elevationHeight);
            main.startLifetime = startLifetime;

            if (hitsOcean && waterfall.tile.elevation == 1)
            {
                ParticleSystem.CollisionModule collision = waterfallRef.GetComponent<ParticleSystem>().collision;
                collision.enabled = false;
                main.startLifetime = waterfallOceanLifeTime;
                main.startSize = new ParticleSystem.MinMaxCurve
                {
                    mode = ParticleSystemCurveMode.TwoConstants,
                    constantMin = waterfallOceanMinSize,
                    constantMax = waterfallOceanMaxSize
                };
                ParticleSystem.EmissionModule emission = waterfallRef.GetComponent<ParticleSystem>().emission;
                emission.rateOverTime = waterfallOceanRate;
                waterfallRef.transform.localScale = new Vector3(waterfallRef.transform.localScale.x, warerfallOceanScaleY, waterfallRef.transform.localScale.z);
                Vector3 moveBackVector = (start - end).normalized;
                moveBackVector = new Vector3(moveBackVector.x, 0, moveBackVector.z);
                waterfallRef.transform.position += moveBackVector * warerfallOceanMoveBack;
            }
        }
    }
}

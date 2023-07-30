using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RiverGenerator
{
    public static void GenerateRivers(Tile[,] tiles, int cellSize, int minElevation, float splitChancePerStep)
    {
        // devide entire map into cells of certain size
        int riverCellXCount = Mathf.CeilToInt((float)Tiles.tilesX / cellSize);
        int riverCellZCount = Mathf.CeilToInt((float)Tiles.tilesZ / cellSize);
        List<RiverCell> riverCells = new List<RiverCell>();
        for (int cellX = 0; cellX < riverCellXCount; cellX++)
        {
            for (int cellZ = 0; cellZ < riverCellZCount; cellZ++)
            {
                // in each cell find highest tiles
                List<Tile> highestTiles = new List<Tile>();
                List<Tile> cellTiles = new List<Tile>();
                for (int tileX = 0; tileX < cellSize; tileX++)
                {
                    for (int tileZ = 0; tileZ < cellSize; tileZ++)
                    {
                        int globalTileX = cellX * cellSize + tileX;
                        int globalTileZ = cellZ * cellSize + tileZ;
                        if (globalTileX < Tiles.tilesX && globalTileZ < Tiles.tilesZ && tiles[globalTileX, globalTileZ] != null)
                        {
                            Tile tile = tiles[globalTileX, globalTileZ];
                            cellTiles.Add(tile);
                            if (highestTiles.Count == 0 || tile.elevation == highestTiles[0].elevation) highestTiles.Add(tile);
                            else if (tile.elevation > highestTiles[0].elevation)
                            {
                                highestTiles.Clear();
                                highestTiles.Add(tile);
                            }
                        }
                    }
                }
                // select random highest tile and save river cell
                if (highestTiles.Count > 0)
                {
                    int randomHighestTile = Random.Range(0, highestTiles.Count);
                    Tile highestTile = highestTiles[randomHighestTile];

                    RiverCell riverCell = new RiverCell()
                    {
                        tiles = cellTiles,
                        highestTile = highestTile
                    };
                    riverCells.Add(riverCell);
                }
            }
        }

        // sort river cells based on the highest tile in each cell
        riverCells.Sort((cell1, cell2) =>
        {
            if (cell1.highestTile.elevation > cell2.highestTile.elevation) return -1;
            else if (cell1.highestTile.elevation == cell2.highestTile.elevation) return 0;
            else return 1;
        });

        // for each river that doesn't have a river yet, try to start a river around the highest tile
        foreach (RiverCell cell in riverCells)
        {
            if (!cell.HasRiver() && cell.highestTile.elevation >= minElevation)
            {
                // find tiles where river can start
                List<int> suitableSides = new List<int>();
                for (int side = 0; side < 6; side++)
                {
                    if (cell.highestTile.neighbors[side] != null && cell.highestTile.neighbors[(side + 1) % 6] != null && cell.highestTile.neighbors[(side + 5) % 6] != null &&
                        cell.highestTile.neighbors[side].elevation > 0 && cell.highestTile.neighbors[(side + 1) % 6].elevation > 0 && cell.highestTile.neighbors[(side + 5) % 6].elevation > 0
                        && !cell.highestTile.rivers[side] && !cell.highestTile.rivers[(side + 1) % 6] && !cell.highestTile.rivers[(side + 5) % 6]
                        && !cell.highestTile.neighbors[side].rivers[(side + 2) % 6] && !cell.highestTile.neighbors[side].rivers[(side + 4) % 6])
                    {
                        suitableSides.Add(side);
                    }
                }

                if (suitableSides.Count > 0)
                {
                    // select random side and start a river there
                    int sideRand = Random.Range(0, suitableSides.Count);
                    int randomSide = suitableSides[sideRand];

                    RiverSegment riverStart = new RiverSegment
                    {
                        tile1 = cell.highestTile,
                        tile2 = cell.highestTile.neighbors[randomSide],
                        tile1Edge = randomSide,
                        tile2Edge = (randomSide + 3) % 6,
                        previousSegment = null,
                        nextSegment1 = null,
                        nextSegment2 = null
                    };

                    // generate rest of the river
                    GenerateRiver(riverStart, splitChancePerStep);
                }
            }
        }
    }

    private class RiverCell
    {
        public List<Tile> tiles;
        public Tile highestTile;

        public bool HasRiver()
        {
            foreach (Tile tile in tiles)
            {
                if (tile.HasRiver()) return true;
            }
            return false;
        }
    }

    private class RiverSegment
    {
        public Tile tile1;
        public Tile tile2;
        public int tile1Edge;
        public int tile2Edge;
        public RiverSegment previousSegment;
        public RiverSegment nextSegment1;
        public RiverSegment nextSegment2;

        public void ApplySegment()
        {
            tile1.rivers[tile1Edge] = tile2.rivers[tile2Edge] = true;
        }

        public bool Equals(RiverSegment segment)
        {
            return segment != null && ((segment.tile1.Equals(tile1) && segment.tile2.Equals(tile2)) || (segment.tile1.Equals(tile2) && segment.tile2.Equals(tile1)));
        }
    }

    private static void GenerateRiver(RiverSegment riverStart, float splitChancePerStep)
    {
        riverStart.ApplySegment();
        RiverSegment currentSegment = riverStart;

        // keep track of how many times river was near a tile in a row
        // usefull for making sure river doesn't go in circles too much
        Tile lastTile1 = riverStart.tile1;
        Tile lastTile2 = riverStart.tile2;
        int lastTile1Repeats = 0;
        int lastTile2Repeats = 0;

        float currentSplitChance = 0;

        while (true)
        {
            // find possible ways to go
            List<RiverSegment> possibleNext = GetNextPossibleRiverSegments(currentSegment);

            // if nowhere to go, stop
            if (possibleNext.Count == 0) break;

            // for each possible segment, calculate it's affect on the repeats of last tile 1 and last tile 2 
            List<int> maxRepeatsIfNext = new List<int>();
            List<RiverSegment> segmentsToRemove = new List<RiverSegment>();
            int totalMaxRepeats = 0;
            foreach (RiverSegment next in possibleNext)
            {
                int tile1RepeatsTmp = 0;
                int tile2RepeatsTmp = 0;
                if (next.tile1.Equals(lastTile1) || next.tile2.Equals(lastTile1)) tile1RepeatsTmp = lastTile1Repeats + 1;
                if (next.tile1.Equals(lastTile2) || next.tile2.Equals(lastTile2)) tile2RepeatsTmp = lastTile2Repeats + 1;
                int maxRepeats = Mathf.RoundToInt(Mathf.Max(tile1RepeatsTmp, tile2RepeatsTmp));
                // if some tile was already used 3 times and this segment would make it 4, remove segment
                if (maxRepeats > 3)
                {
                    segmentsToRemove.Add(next);
                }
                else
                {
                    maxRepeatsIfNext.Add(maxRepeats);
                    totalMaxRepeats += maxRepeats;
                }
            }
            // segments are removed here because they can't be removed in a forach loop
            foreach (RiverSegment segmentToRemove in segmentsToRemove)
            {
                possibleNext.Remove(segmentToRemove);
            }
            if (possibleNext.Count == 0) break;

            // for each possible segment calculate the number, so that it reflects how likely that segment is to be picked relative to other segment values
            List<float> randLimits = new List<float>();
            float lastRandLimit = 0;
            foreach (int repeats in maxRepeatsIfNext)
            {
                lastRandLimit += (float)totalMaxRepeats / repeats;
                randLimits.Add(lastRandLimit);
            }

            // a oute is selected if the random value is below it's random limit. That cause segments that avoid tile repetition more likely
            float nextRand = Random.value * (randLimits[randLimits.Count - 1] - 0.0001f);
            RiverSegment nextSegment = null;
            for (int i = 0; i < possibleNext.Count; i++)
            {
                if (nextRand < randLimits[i])
                {
                    nextSegment = possibleNext[i];
                }
            }

            // keep track of repeating tiles
            bool tile1IsTile1 = nextSegment.tile1.Equals(lastTile1);
            bool tile1IsTile2 = nextSegment.tile1.Equals(lastTile2);
            bool tile2IsTile1 = nextSegment.tile2.Equals(lastTile1);
            bool tile2IsTile2 = nextSegment.tile2.Equals(lastTile2);

            if (tile1IsTile1 || tile2IsTile1) lastTile1Repeats++;
            if (tile1IsTile2 || tile2IsTile2) lastTile2Repeats++;

            if (!tile1IsTile1 && !tile1IsTile2)
            {
                if (tile2IsTile1)
                {
                    lastTile2 = nextSegment.tile1;
                    lastTile2Repeats = 0;
                }
                else
                {
                    lastTile1 = nextSegment.tile1;
                    lastTile1Repeats = 0;
                }
            }

            if (!tile2IsTile1 && !tile2IsTile2)
            {
                if (tile1IsTile2)
                {
                    lastTile1 = nextSegment.tile2;
                    lastTile1Repeats = 0;
                }
                else
                {
                    lastTile2 = nextSegment.tile2;
                    lastTile2Repeats = 0;
                }
            }

            // save river in tile structure
            nextSegment.ApplySegment();
            currentSegment.nextSegment1 = nextSegment;

            // remove selected segment and try to split the river
            possibleNext.Remove(nextSegment);
            if (possibleNext.Count > 0)
            {
                float splitRand = Random.value;
                if (splitRand < currentSplitChance)
                {
                    int splitNextRand = Random.Range(0, possibleNext.Count);
                    RiverSegment splitSegment = possibleNext[splitNextRand];
                    currentSegment.nextSegment2 = splitSegment;
                    currentSplitChance = 0;

                    GenerateRiver(splitSegment, splitChancePerStep / 2);
                }
            }

            currentSegment = nextSegment;

            // increase split chance for the next iteration
            currentSplitChance += splitChancePerStep;
        }
    }

    private static List<RiverSegment> GetNextPossibleRiverSegments(RiverSegment currentSegment)
    {
        Tile[] possibleTiles1 = new Tile[] { currentSegment.tile1, currentSegment.tile1, currentSegment.tile2, currentSegment.tile2 };
        int[] possibleNeighbours = new int[] { (currentSegment.tile1Edge + 1) % 6, (currentSegment.tile1Edge + 5) % 6, (currentSegment.tile2Edge + 1) % 6, (currentSegment.tile2Edge + 5) % 6 };

        List<RiverSegment> possibleNext = new List<RiverSegment>();

        // test each of the 4 directions a river can go to see which ones are suitable
        for (int i = 0; i < 4; i++)
        {
            if (possibleTiles1[i].neighbors[possibleNeighbours[i]] != null && possibleTiles1[i].neighbors[possibleNeighbours[i]].elevation > 0)
            {
                RiverSegment newRiverSegment = new RiverSegment()
                {
                    tile1 = possibleTiles1[i],
                    tile2 = possibleTiles1[i].neighbors[possibleNeighbours[i]],
                    tile1Edge = possibleNeighbours[i],
                    tile2Edge = (possibleNeighbours[i] + 3) % 6,
                    previousSegment = currentSegment,
                    nextSegment1 = null,
                    nextSegment2 = null
                };
                // if there is a river here already, it means we crashed into another river and should return no options
                if (!newRiverSegment.Equals(currentSegment.previousSegment) &&
                    !(currentSegment.previousSegment != null && newRiverSegment.Equals(currentSegment.previousSegment.nextSegment1)) &&
                    !(currentSegment.previousSegment != null && newRiverSegment.Equals(currentSegment.previousSegment.nextSegment2)) &&
                    newRiverSegment.tile1.rivers[newRiverSegment.tile1Edge])
                {
                    return new List<RiverSegment>();
                }
                if (!newRiverSegment.Equals(currentSegment.previousSegment) &&
                    !(currentSegment.previousSegment != null && newRiverSegment.Equals(currentSegment.previousSegment.nextSegment1)) &&
                    !(currentSegment.previousSegment != null && newRiverSegment.Equals(currentSegment.previousSegment.nextSegment2)) &&
                    IsRiverSegmentLegal(newRiverSegment, currentSegment.previousSegment))
                {
                    (bool connectsToOtherRiver, bool sameRiverAsOrigin) = IsConnectingToAnotherRiver(newRiverSegment);

                    // if river segement connect with different river it should become the only option
                    if (connectsToOtherRiver && !sameRiverAsOrigin) return new List<RiverSegment>() { newRiverSegment };
                    // if river segement doesn't connect to other river it should be one of the options
                    else if (!connectsToOtherRiver) possibleNext.Add(newRiverSegment);
                    // if river segement connect with same river, river should end
                    else return new List<RiverSegment>();
                }
            }
        }

        return possibleNext;
    }

    private static bool IsRiverSegmentLegal(RiverSegment segment, RiverSegment previousSegment)
    {
        // if river is already on the segment, river is not legal
        if (segment.tile1.rivers[segment.tile1Edge]) return false;

        // going uphill is not legal
        if (segment.previousSegment != null)
        {
            // lowest elevation at the start of the segment
            int lowestElevationAtStart = Mathf.RoundToInt(Mathf.Min(segment.tile1.elevation, segment.tile2.elevation, segment.previousSegment.tile1.elevation, segment.previousSegment.tile2.elevation));
            // lowest elevation at the end of the river
            int lowestElevationAtEnd = Mathf.RoundToInt(Mathf.Min(segment.tile1.elevation, segment.tile2.elevation));

            if (lowestElevationAtEnd > lowestElevationAtStart) return false;
        }

        // when previous segment of the current segment can reach the new segment, it is not legal (river would not be flowing continuously)
        if (previousSegment != null)
        {
            Tile[] possiblePrevTiles1 = new Tile[] { previousSegment.tile1, previousSegment.tile1, previousSegment.tile2, previousSegment.tile2 };
            int[] possiblePrevNeighbours = new int[] { (previousSegment.tile1Edge + 1) % 6, (previousSegment.tile1Edge + 5) % 6, (previousSegment.tile2Edge + 1) % 6, (previousSegment.tile2Edge + 5) % 6 };

            for (int i = 0; i < 4; i++)
            {
                if (possiblePrevTiles1[i].neighbors[possiblePrevNeighbours[i]] != null)
                {
                    RiverSegment prevPossibleRiverSegment = new RiverSegment()
                    {
                        tile1 = possiblePrevTiles1[i],
                        tile2 = possiblePrevTiles1[i].neighbors[possiblePrevNeighbours[i]],
                    };
                    if (prevPossibleRiverSegment.Equals(segment)) return false;
                }
            }
        }
        return true;
    }

    private static (bool, bool) IsConnectingToAnotherRiver(RiverSegment segment)
    {
        Tile[] possibleTiles1 = new Tile[] { segment.tile1, segment.tile1, segment.tile2, segment.tile2 };
        int[] possibleNeighbours = new int[] { (segment.tile1Edge + 1) % 6, (segment.tile1Edge + 5) % 6, (segment.tile2Edge + 1) % 6, (segment.tile2Edge + 5) % 6 };

        for (int i = 0; i < 4; i++)
        {
            if (possibleTiles1[i].neighbors[possibleNeighbours[i]] != null)
            {
                RiverSegment possibleRiverSegment = new RiverSegment()
                {
                    tile1 = possibleTiles1[i],
                    tile2 = possibleTiles1[i].neighbors[possibleNeighbours[i]],
                    tile1Edge = possibleNeighbours[i],
                    tile2Edge = (possibleNeighbours[i] + 3) % 6,
                };
                if (segment.previousSegment == null || !(possibleRiverSegment.Equals(segment.previousSegment) || possibleRiverSegment.Equals(segment.previousSegment.nextSegment1) || possibleRiverSegment.Equals(segment.previousSegment.nextSegment2)))
                {
                    if (possibleRiverSegment.tile1.rivers[possibleRiverSegment.tile1Edge])
                    {
                        // if some river was found check if this is the same river the segment is part of
                        RiverSegment currentSegment = segment.previousSegment;
                        while (currentSegment != null)
                        {
                            // if it is the same river
                            if (possibleRiverSegment.Equals(currentSegment)) return (true, true);
                            if (currentSegment.nextSegment2 != null)
                            {
                                RiverSegment currentSplitSegment = currentSegment.nextSegment2;
                                while (currentSplitSegment != null)
                                {
                                    if (possibleRiverSegment.Equals(currentSplitSegment)) return (true, true);
                                    currentSplitSegment = currentSplitSegment.nextSegment1;
                                }
                            }
                            currentSegment = currentSegment.previousSegment;
                        }

                        // if it is different river
                        return (true, false);
                    }
                }
            }
        }
        return (false, false);
    }
}

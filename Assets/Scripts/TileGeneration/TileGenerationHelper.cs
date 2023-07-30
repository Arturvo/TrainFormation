using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileGenerationHelper
{
    // find which tile the point belongs to
    public static (int, int) FindTileByCoordinates(Vector2 point)
    {
        int zFrame = Mathf.FloorToInt((point.y) / (1.5f * TileGenerator.a));
        int tileX;
        int tileZ;

        if (point.y - zFrame * 1.5f * TileGenerator.a > 0.5f * TileGenerator.a)
        {
            tileX = Mathf.FloorToInt((point.x - zFrame * TileGenerator.h) / (2 * TileGenerator.h));
            tileZ = zFrame;
        }
        else
        {
            int xFrame = Mathf.FloorToInt((point.x) / (TileGenerator.h));
            bool below;

            if ((zFrame % 2 == 0 && xFrame % 2 == 0) || (zFrame % 2 == 1 && xFrame % 2 == 1))
            {
                below = point.y < -((0.5f * TileGenerator.a) / TileGenerator.h) * point.x + (0.5f + zFrame * 1.5f + xFrame * 0.5f) * TileGenerator.a;
            }
            else
            {
                below = point.y < ((0.5f * TileGenerator.a) / TileGenerator.h) * point.x + (zFrame * 1.5f - xFrame * 0.5f) * TileGenerator.a;
            }

            if (below)
            {
                tileX = xFrame - zFrame + 1 >= 0 ? (xFrame - zFrame + 1) / 2 : -1;
                tileZ = zFrame - 1;
            }
            else
            {
                tileX = xFrame - zFrame >= 0 ? (xFrame - zFrame) / 2 : -1;
                tileZ = zFrame;
            }

        }

        return (tileX, tileZ);
    }

    public static (float, int, bool, Vector3[]) FindHeight(Vector2 point, Tile tile)
    {
        Vector2 pointLocal = point + new Vector2(TileGenerator.h, TileGenerator.a * 1.5f);
        pointLocal = PointAfterAxisRotation(Mathf.Deg2Rad * 30, pointLocal);
        pointLocal = RotatePointAroundPivot(Mathf.Deg2Rad * 60, pointLocal, new Vector2(1.5f * TileGenerator.a, TileGenerator.h));

        float yAdj = pointLocal.y / TileGenerator.zDistance;
        float xAdj = (pointLocal.x - yAdj * (TileGenerator.xDistance / 2)) / TileGenerator.xDistance;

        int leftX = Mathf.FloorToInt(xAdj);
        int rightX = Mathf.CeilToInt(xAdj);
        int lowerZ = Mathf.FloorToInt(yAdj);
        int upperZ = Mathf.CeilToInt(yAdj);

        if (leftX < 0) leftX = 0;
        if (rightX < 0) rightX = 0;
        if (lowerZ < 0) lowerZ = 0;
        if (upperZ < 0) upperZ = 0;
        if (leftX >= TileGenerator.pointsSize) leftX = TileGenerator.pointsSize - 1;
        if (rightX >= TileGenerator.pointsSize) rightX = TileGenerator.pointsSize - 1;
        if (lowerZ >= TileGenerator.pointsSize) lowerZ = TileGenerator.pointsSize - 1;
        if (upperZ >= TileGenerator.pointsSize) upperZ = TileGenerator.pointsSize - 1;

        bool isRiver = 
            tile.vertices[leftX, lowerZ].rivers % 10000 >= 1000 ||
            tile.vertices[leftX, upperZ].rivers % 10000 >= 1000 ||
            tile.vertices[rightX, lowerZ].rivers % 10000 >= 1000 ||
            tile.vertices[rightX, upperZ].rivers % 10000 >= 1000;

        Vector3 averagePosition = (tile.vertices[leftX, lowerZ].position +
            tile.vertices[leftX, upperZ].position +
            tile.vertices[rightX, lowerZ].position +
            tile.vertices[rightX, upperZ].position) / 4;

        Vector2 adjustmentVector = new Vector2(averagePosition.x, averagePosition.z) - (pointLocal - new Vector2(1.5f * TileGenerator.a, TileGenerator.h));
        pointLocal -= adjustmentVector;

        yAdj = pointLocal.y / TileGenerator.zDistance;
        xAdj = (pointLocal.x - yAdj * (TileGenerator.xDistance / 2)) / TileGenerator.xDistance;

        leftX = Mathf.FloorToInt(xAdj);
        rightX = Mathf.CeilToInt(xAdj);
        lowerZ = Mathf.FloorToInt(yAdj);
        upperZ = Mathf.CeilToInt(yAdj);

        if (!(leftX >= 0 && rightX >= 0 && lowerZ >= 0 && upperZ >= 0 && leftX < TileGenerator.pointsSize && rightX < TileGenerator.pointsSize && lowerZ < TileGenerator.pointsSize && upperZ < TileGenerator.pointsSize))
        {
            return (-Mathf.Infinity, 10, true, null);
        }

        Vector3[] positions = new Vector3[]
        {
            tile.vertices[leftX, lowerZ].position,
            tile.vertices[leftX, upperZ].position,
            tile.vertices[rightX, lowerZ].position,
            tile.vertices[rightX, upperZ].position
        };

        float height = (tile.vertices[leftX, lowerZ].position.y +
            tile.vertices[leftX, upperZ].position.y +
            tile.vertices[rightX, lowerZ].position.y +
            tile.vertices[rightX, upperZ].position.y) / 4;

        return (height, Mathf.RoundToInt(Mathf.Max(tile.vertices[leftX, lowerZ].steepness, tile.vertices[leftX, upperZ].steepness, tile.vertices[rightX, lowerZ].steepness, tile.vertices[rightX, upperZ].steepness)), isRiver, positions);
    }

    public static Vector2 PointAfterAxisRotation(float angle, Vector2 point)
    {
        float x = point.x * Mathf.Cos(angle) + point.y * Mathf.Sin(angle);
        float y = point.y * Mathf.Cos(angle) - point.x * Mathf.Sin(angle);
        return new Vector2(x, y);
    }

    public static Vector2 RotatePointAroundPivot(float angle, Vector2 point, Vector2 pivot)
    {
        float s = Mathf.Sin(angle);
        float c = Mathf.Cos(angle);

        // translate point back to origin:
        point.x -= pivot.x;
        point.y -= pivot.y;

        // rotate point
        float xnew = point.x * c - point.y * s;
        float ynew = point.x * s + point.y * c;

        // translate point back:
        point.x = xnew + pivot.x;
        point.y = ynew + pivot.y;
        return point;
    }
}

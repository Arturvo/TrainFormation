using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanGenerator : MonoBehaviour
{
    public GameObject tilesParent;
    public GameObject oceanTilePrefab;
    public GameObject tileColliderPrefab;

    public static readonly float oceanVertexDistance = 1f;
    public static readonly float oceanHeight = 2.7f;
    public static readonly float oceanExtraSize = 1000;

    private Mesh mainMesh;
    private Mesh[] neighbourMeshes;

    public void StartByScheduler()
    {
        GenerateOceanPrefabs();
    }

    public void GenerateOcean(List<Tile> tiles)
    {
        foreach (Tile tile in tiles)
        {
            // initiate ocean tile
            GameObject tileObject = Instantiate(oceanTilePrefab, tile.coordinates, Quaternion.identity, tilesParent.transform);
            tileObject.GetComponent<MeshFilter>().mesh = mainMesh;
            tileObject.GetComponent<MeshRenderer>().sortingOrder = 2;
            tile.objectRef = tileObject;

            // add a simple collider to the tile
            if (tile.hasCollider)
            {
                GameObject colliderRef = Instantiate(tileColliderPrefab, tile.coordinates + new Vector3(0, oceanHeight, 0), Quaternion.identity, tileObject.transform);
                colliderRef.transform.localScale = new Vector3(TileGenerator.a, oceanHeight, TileGenerator.a);
                colliderRef.GetComponent<TileColliderScript>().tile = tile;
                tile.colliderRef = colliderRef;
            }

            for (int i = 0; i < 6; i++)
            {
                if (tile.neighbors[i] != null && !tile.neighbors[i].isOcean)
                {
                    GameObject neighbourMesh = Instantiate(oceanTilePrefab, tile.coordinates, Quaternion.identity, tileObject.transform);
                    neighbourMesh.GetComponent<MeshFilter>().mesh = neighbourMeshes[i];
                    neighbourMesh.GetComponent<MeshRenderer>().sortingOrder = 2;
                }
            }
        }
    }

    private void GenerateOceanPrefabs()
    {
        float a = TileGenerator.a;
        float h = TileGenerator.h;

        Vector3[] mainVertices = new Vector3[]
        {
            new Vector3(0, oceanHeight, a),
            new Vector3(h, oceanHeight, a/2),
            new Vector3(h, oceanHeight, -a/2),
            new Vector3(0, oceanHeight, -a),
            new Vector3(-h, oceanHeight, -a/2),
            new Vector3(-h, oceanHeight, a/2),
            new Vector3(0, oceanHeight, 0)
        };

        int[] mainTriangles = new int[]
        {
            6,0,1,6,1,2,6,2,3,6,3,4,6,4,5,6,5,0
        };

        mainMesh = new Mesh()
        {
            vertices = mainVertices,
            triangles = mainTriangles
        };
        mainMesh.RecalculateNormals();
        mainMesh.RecalculateBounds();

        neighbourMeshes = new Mesh[6];
        float elevationBorderDistance = ((float) TileGenerator.elevationBorderSizePoints / TileGenerator.r) * TileGenerator.a;

        for (int i = 0; i < 6; i++)
        {
            Vector3[] neighbourVertices = new Vector3[]
            {
                mainVertices[i],
                mainVertices[(i+1)%6],
                mainVertices[i] + (mainVertices[i] - mainVertices[6]).normalized * elevationBorderDistance,
                mainVertices[(i+1)%6] + (mainVertices[(i+1)%6] - mainVertices[6]).normalized * elevationBorderDistance,
            };
            int[] neighbourTriangles = new int[]
            {
                0,2,1,2,3,1
            };
            Mesh neighbourMesh = new Mesh()
            {
                vertices = neighbourVertices,
                triangles = neighbourTriangles
            };
            neighbourMesh.RecalculateNormals();
            neighbourMesh.RecalculateBounds();

            neighbourMeshes[i] = neighbourMesh;
        }
    }
}

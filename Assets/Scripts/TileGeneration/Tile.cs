using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public Vector3 coordinates;
    public TileType type;
    public Tile[] neighbors;
    public bool[] rivers;
    public int xIndex;
    public int zIndex;
    public GameObject objectRef;
    public GameObject colliderRef;
    public Dictionary<ObjectParent, GameObject> objectParents;
    public int elevation;
    public TileGenerator.Vertex[,] vertices;
    public bool hasForest;
    public bool isOcean;
    public int[] rails;
    public bool hasCollider;
    public bool canBeMoved;
    public bool hasStation;
    public int segmentId;
    public PowerupSystem.PowerupType powerup;
    public GameObject powerupRef;

    public Tile(Vector3 coordinates, int xIndex, int zIndex)
    {
        this.coordinates = coordinates;
        this.xIndex = xIndex;
        this.zIndex = zIndex;
        neighbors = new Tile[6];
        rivers = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            neighbors[i] = null;
            rivers[i] = false;
        }
        rails = null;
        canBeMoved = true;
        hasStation = false;
        segmentId = -1;
        powerup = PowerupSystem.PowerupType.NoPowerup;
    }

    // safely destroy tile object in the scene together with mesh to avoid memory leak
    public void DestroyTile()
    {
        if (objectRef != null)
        {
            if (!isOcean)
            {
                MeshFilter meshFilter = objectRef.GetComponent<MeshFilter>();
                if (meshFilter != null) GameObject.Destroy(meshFilter.sharedMesh);

                List<Transform> tileComponents = GetAllChildren(objectRef.transform);
                foreach (Transform component in tileComponents)
                {
                    MeshRenderer meshRenderer = component.GetComponent<MeshRenderer>();
                    if (meshRenderer != null) GameObject.Destroy(meshRenderer.material);
                    GameObject.Destroy(component.gameObject);
                }
            }
            GameObject.Destroy(objectRef);
        }
    }

    public List<Transform> GetAllChildren(Transform parent)
    {
        List<Transform> children = new List<Transform>() {parent};
        foreach (Transform child in parent)
        {
            children.AddRange(GetAllChildren(child));
        }
        return children;
    }

    public enum ObjectParent
    {
        Rivers,
        Trees,
        Forest,
        ForestShadow,
        Rails,
        TrainStation,
        LevelBorder,
        Powerups
    }

    public void CreateParentObjects()
    {
        objectParents = new Dictionary<ObjectParent, GameObject>();
        List<ObjectParent> parentsToRotate = new List<ObjectParent>()
        {
            ObjectParent.ForestShadow,
            ObjectParent.Forest
        };

        foreach(ObjectParent objectParent in Enum.GetValues(typeof(ObjectParent)))
        {
            GameObject parentRef = new GameObject(objectParent.ToString());
            parentRef.transform.SetParent(objectRef.transform);
            parentRef.transform.localPosition = Vector3.zero;
            if (parentsToRotate.Contains(objectParent))
            {
                parentRef.transform.localRotation = Quaternion.Euler(new Vector3(0,-30,0));
            }
            else
            {
                parentRef.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            objectParents.Add(objectParent, parentRef);
        }
    }

    public bool HasRiver()
    {
        for (int i = 0; i < 6; i++)
        {
            if (rivers[i]) return true;
        }
        return false;
    }

    public bool Equals(Tile tile)
    {
        return tile != null && tile.xIndex == xIndex && tile.zIndex == zIndex;
    }
}

public enum TileType
{
    Grass,
    Plain,
    Rock,
    Snow
}

public class TileTypeProperties
{
    public Color32 primaryColorMin;
    public Color32 primaryColorMax;
    public Color32 secondaryColor;
    // if two tile types are next to each other the one with heigher layer 
    // priority will "spill a texture" into the one with lower layer priority
    public int layerPriority;
    // usually the river next to the tile has sand texture. If this bool is true the river will have rock texture instead
    public bool rockRiver;
    public float forestChance;
}

public class TileColorScheme
{
    public Color blendColor;
    public float blendStrength;
}
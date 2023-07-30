using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryCameraController : MonoBehaviour
{
    public float cameraYPosition = 80;
    public float cameraZPosition = 25;

    public float cameraXoffset = 50;

    private float middleTileZ;
    private float startTileX;
    private int startTile = 5; 


    public TrainController TrainController;

    public void StartByScheduler()
    {
        middleTileZ = Tiles.tiles[0, MapGenerator.oceanWidth + (MapGenerator.mapWidth - 1) / 2].coordinates.z - TileGenerator.a * 0.5f;
        startTileX = Tiles.tiles[startTile, MapGenerator.oceanWidth + (MapGenerator.mapWidth - 1) / 2].coordinates.x;
        CenterOnTrain();
    }

    public void CenterOnTrain()
    {
        float xPosition = Mathf.Max(TrainController.GetTrain().transform.position.x,startTileX) + cameraXoffset;
        transform.position = new Vector3(xPosition, cameraYPosition, cameraZPosition);

        Vector3 pointToLookAt = new Vector3(xPosition, 0, middleTileZ);
        Vector3 directionToLookAt = (pointToLookAt - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToLookAt);
    }

    void Update()
    {
        if (TrainController.GetTrain().transform.position.x < transform.position.x + cameraXoffset) CenterOnTrain();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : MonoBehaviour
{
    public TileGenerator tileGenerator;
    public Tiles tiles;
    public ForestGenerator forestGenerator;
    public OceanGenerator oceanGenerator;
    public RiverRenderer riverRenderer;
    public CameraSystem cameraSystem;
    public TrainController trainController;
    public TileMoving tileMoving;
    public MapGenerator mapGenerator;
    public UIManager uiManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("GraphicSettings").GetComponent<GraphicSettings>().ApplyLightSetting();
        GameState gameState = GameObject.Find("GameState").GetComponent<GameState>();
        gameState.StartByScheduler(uiManager);
        gameState.cheating = false;

        mapGenerator.StartByScheduler();
        tiles.StartByScheduler();
        tileGenerator.StartByScheduler();
        forestGenerator.StartByScheduler();
        riverRenderer.StartByScheduler();
        oceanGenerator.StartByScheduler();

        tiles.GenerateMap();
        tiles.GenerateInitialTiles();

        trainController.InitTrain();
        tileMoving.StartByScheduler();
        cameraSystem.StartByScheduler();
    }

    // Update is called once per frame
    void Update()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    public PrimaryCameraController primaryCameraController;
    public SecondaryCameraController secondaryCameraController;
    public GameState gameState;

    public KeyCode cameraSwapKey = KeyCode.F6;

    private bool usingPrimaryCamera = true;

    public void StartByScheduler()
    {
        primaryCameraController.StartByScheduler();
        gameState = GameObject.Find("GameState").GetComponent<GameState>();
    }

    void Update()
    {
        if (Input.GetKeyDown(cameraSwapKey) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            gameState.cheating = true;

            if (usingPrimaryCamera)
            {
                usingPrimaryCamera = false;
                primaryCameraController.enabled = false;
                secondaryCameraController.enabled = true;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                usingPrimaryCamera = true;
                primaryCameraController.enabled = true;
                secondaryCameraController.enabled = false;
                primaryCameraController.CenterOnTrain();
            }
        } 
    }
}

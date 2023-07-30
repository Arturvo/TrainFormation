using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI FPStext;

    private bool showingFPS = false;

    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showingFPS = !showingFPS;
            FPStext.gameObject.SetActive(showingFPS);
        }

        time += Time.deltaTime;
        frameCount++;
        if (time > pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            FPStext.text = frameRate.ToString() + " FPS";

            time -= pollingTime;
            frameCount = 0;
        }
    }
}

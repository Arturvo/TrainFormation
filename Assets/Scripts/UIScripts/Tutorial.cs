using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public UIManager uiManager;

    public GameObject instruction1;
    public GameObject instruction2;
    public GameObject instruction3;

    void Awake()
    {
        if (!PlayerPrefs.HasKey("BeginnerBestLevel") || PlayerPrefs.GetInt("BeginnerBestLevel") < 2)
        {

        }
    }
}

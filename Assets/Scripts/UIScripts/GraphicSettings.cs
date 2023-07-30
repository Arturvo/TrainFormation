using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.SceneManagement;

public class GraphicSettings : MonoBehaviour
{
    public GraphicSetting currentSetting = GraphicSetting.High;

    public static GraphicSettings instance;

    public RenderPipelineAsset lowSettingsAsset;
    public RenderPipelineAsset middleSettingsAsset;
    public RenderPipelineAsset highSettingsAsset;

    public Dictionary<GraphicSetting, SettingPrefab> settingPrefabMapping;

    public enum GraphicSetting
    {
        High,
        Medium,
        Low,
    }

    public struct SettingPrefab
    {
        public RenderPipelineAsset renderPipelineAsset;
        public bool disableShadows;
        public int terrainQuality;
        public int cliffNoiseLayers;
        public int heightNoiseLayers;
        public int riverNoiseLayers;
        public int textureNoiseLayers;
        public int secondaryTextureNoiseLayers;
        public int mixingNoiseLayers;
        public float elevationBorderSize;
        public float riverWidth;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        settingPrefabMapping = new Dictionary<GraphicSetting, SettingPrefab>()
        {
            {GraphicSetting.Low, new SettingPrefab()
                {
                    renderPipelineAsset = lowSettingsAsset,
                    disableShadows = true,
                    terrainQuality = 4,
                    cliffNoiseLayers = 2,
                    heightNoiseLayers = 2,
                    riverNoiseLayers = 2,
                    textureNoiseLayers = 2,
                    secondaryTextureNoiseLayers = 2,
                    mixingNoiseLayers = 2,
                    elevationBorderSize = 0.2f,
                    riverWidth = 0.1f
            }
            },
            {GraphicSetting.Medium, new SettingPrefab()
                {
                    renderPipelineAsset = middleSettingsAsset,
                    disableShadows = false,
                    terrainQuality = 5,
                    cliffNoiseLayers = 5,
                    heightNoiseLayers = 5,
                    riverNoiseLayers = 5,
                    textureNoiseLayers = 5,
                    secondaryTextureNoiseLayers = 5,
                    mixingNoiseLayers = 5,
                    elevationBorderSize = 0.3f,
                    riverWidth = 0.15f
                }
            },
            {GraphicSetting.High, new SettingPrefab()
                {
                    renderPipelineAsset = highSettingsAsset,
                    disableShadows = false,
                    terrainQuality = 6,
                    cliffNoiseLayers = 9,
                    heightNoiseLayers = 9,
                    riverNoiseLayers = 9,
                    textureNoiseLayers = 9,
                    secondaryTextureNoiseLayers = 9,
                    mixingNoiseLayers = 9,
                    elevationBorderSize = 0.3f,
                    riverWidth = 0.15f
                }
            }
        };

        if (PlayerPrefs.HasKey("GraphicsQuality")) currentSetting = (GraphicSetting) PlayerPrefs.GetInt("GraphicsQuality");
        ApplySettings(false);
    }

    public void ApplySettings(bool recreateTiles)
    {
        SettingPrefab activeSettings = settingPrefabMapping[currentSetting];
        GraphicsSettings.renderPipelineAsset = activeSettings.renderPipelineAsset;
        if (recreateTiles)
        {
            GameObject tilesObject = GameObject.Find("Tiles");
            tilesObject.GetComponent<TileGenerator>().StartByScheduler();
            Tiles tilesScript = tilesObject.GetComponent<Tiles>();
            Tile[,] tiles = Tiles.tiles;
            List<Tile> tilesToUpdate = new List<Tile>();

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int z = 0; z < tiles.GetLength(1); z++)
                {
                    Tile tile = tiles[x, z];
                    if (tile != null && tile.objectRef != null && !tile.isOcean)
                    {
                        tilesToUpdate.Add(tile);
                    }
                }
            }
            tilesScript.GenerateTiles(tilesToUpdate, false);
        }
        ApplyLightSetting();
    }

    public void ApplyLightSetting()
    {
        SettingPrefab activeSettings = settingPrefabMapping[currentSetting];
        GameObject light = GameObject.Find("Directional Light");
        if (light != null)
        {
            if (activeSettings.disableShadows)
            {
                light.GetComponent<Light>().shadows = LightShadows.None;
            }
            else
            {
                light.GetComponent<Light>().shadows = LightShadows.Hard;
            }
        }
    }

    public void ChangeSetting(int newSetting, bool isMainMenu)
    {
        currentSetting = (GraphicSetting)newSetting;
        PlayerPrefs.SetInt("GraphicsQuality", newSetting);
        ApplySettings(!isMainMenu);
    }
}
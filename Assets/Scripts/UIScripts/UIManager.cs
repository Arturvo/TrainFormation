using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI level;
    public TextMeshProUGUI score;
    public TextMeshProUGUI gameOverLevel;
    public TextMeshProUGUI gameOverScore;
    public TextMeshProUGUI gameOverBestLevel;
    public TextMeshProUGUI gameOverBestScore;
    public GameObject newBestLevel;
    public GameObject newBestScore;

    public GameObject pauseMenu;
    public GameObject settngsMenu;
    public GameObject gameOverMenu;
    public TileMoving tileMoving;
    public PowerupSystem powerupSystem;
    public PrimaryCameraController primaryCameraController;
    public TrainController trainController;
    public Tiles tiles;

    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;
    public TMP_Dropdown graphicsSettingsDropdown;
    private SoundSystem soundSystem;
    private GraphicSettings graphicsSettings;
    private GameState gameState;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI soundVolumeText;

    public Animator transition;
    public float transitionTime = 1;
    private float fixedDeltaTime;

    public GameObject instruction1;
    public GameObject instruction2;
    public GameObject instruction3;

    public enum PouseType
    {
        PlayerPouse,
        GameOver,
        Tutorial
    }

    public void Awake()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
    }

    public void Start()
    {
        soundSystem = GameObject.Find("SoundSystem").GetComponent<SoundSystem>();
        graphicsSettings = GameObject.Find("GraphicSettings").GetComponent<GraphicSettings>();
        gameState = GameObject.Find("GameState").GetComponent<GameState>();
        musicVolumeSlider.value = soundSystem.musicVolume;
        soundVolumeSlider.value = soundSystem.soundVolume;
        musicVolumeText.text = soundSystem.musicVolume.ToString();
        soundVolumeText.text = soundSystem.soundVolume.ToString();
        graphicsSettingsDropdown.value = (int)graphicsSettings.currentSetting;
        musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);
        soundVolumeSlider.onValueChanged.AddListener(ChangeSoundVolume);
        graphicsSettingsDropdown.onValueChanged.AddListener(ChangeGraphicsSettings);
    }

    public void CheckForFirstInstruction()
    {
        if ((!PlayerPrefs.HasKey("BeginnerBestLevel") || PlayerPrefs.GetInt("BeginnerBestLevel") < 2) && gameState.difficultySetting == Difficulty.DifficultySetting.Beginner)
        {
            PauseGame(PouseType.Tutorial, instruction1);
        }
    }

    public void CheckForSecondInstruction()
    {
        if ((!PlayerPrefs.HasKey("BeginnerBestLevel") || PlayerPrefs.GetInt("BeginnerBestLevel") < 3) && gameState.difficultySetting == Difficulty.DifficultySetting.Beginner)
        {
            PauseGame(PouseType.Tutorial, instruction2);
        }
    }

    public void CheckForThirdInstruction()
    {
        if ((!PlayerPrefs.HasKey("BeginnerBestLevel") || PlayerPrefs.GetInt("BeginnerBestLevel") < 4) && gameState.difficultySetting == Difficulty.DifficultySetting.Beginner)
        {
            PauseGame(PouseType.Tutorial, instruction3);
        }
    }

    public void ChangeMusicVolume(float value)
    {
        soundSystem.SetMusicVolume(Mathf.RoundToInt(value));
        soundSystem.PlaySound("MoveSlider");
        musicVolumeText.text = value.ToString();
    }

    public void ChangeSoundVolume(float value)
    {
        soundSystem.SetSoundVolume(Mathf.RoundToInt(value));
        soundSystem.PlaySound("MoveSlider");
        soundVolumeText.text = value.ToString();
    }

    public void ChangeGraphicsSettings(int value)
    {
        graphicsSettings.ChangeSetting(value, false);
        ChangeRailsVisibility(false);
    }

    public void GameOver(bool bestLevel, bool bestScore)
    {
        gameOverLevel.text = "Level " + gameState.currentLevel;
        gameOverScore.text = "Score " + gameState.currentScore;
        gameOverBestLevel.text = "Level " + gameState.bestLevel;
        gameOverBestScore.text = "Score " + gameState.bestScore;
        if (bestLevel) newBestLevel.SetActive(true);
        if (bestScore) newBestScore.SetActive(true);

        powerupSystem.CancelSlowTimeSpeed();
        primaryCameraController.enabled = false;
        powerupSystem.HideButtons();
        level.gameObject.SetActive(false);
        score.gameObject.SetActive(false);

        PauseGame(PouseType.GameOver);
        GameObject soundSystemObject = GameObject.Find("SoundSystem");
        if (soundSystemObject != null)
        {
            soundSystemObject.GetComponent<SoundSystem>().StopMusic();
        }
        tiles.ScaleUpColliders();
    }

    public void StartNewGame()
    {
        UnpauseGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameObject soundSystemObject = GameObject.Find("SoundSystem");
        if (soundSystemObject != null)
        {
            soundSystemObject.GetComponent<SoundSystem>().RestartMusic();
        }
    }

    public void SetLevel(int newLevel, bool animation = false)
    {
        level.text = "Level " + newLevel;
        if (animation) level.gameObject.GetComponent<Animator>().SetTrigger("NextLevel");
    }

    public void SetScore(int newScore)
    {
        score.text = "Score " + newScore;
    }

    public void PauseGame(PouseType pouseType, GameObject instruction = null)
    {
        soundSystem.StopSound("TrainMovement");

        if (pouseType == PouseType.PlayerPouse)
        {
            Time.timeScale = 0;
            Time.fixedDeltaTime = 0;
            ChangeRailsVisibility(false);
            pauseMenu.SetActive(true);
        }
        else if (pouseType == PouseType.Tutorial)
        {
            gameState.state = GameState.State.GameTutorialPaused;
            Time.timeScale = 0;
            Time.fixedDeltaTime = 0;
            instruction.SetActive(true);
        }
        else if (pouseType == PouseType.GameOver)
        {
            gameOverMenu.SetActive(true);
        }

        powerupSystem.DisableButtons();

        switch (tileMoving.state)
        {
            case TileMoving.State.PointingTile:
                tileMoving.CancelPointing();
                break;
            case TileMoving.State.HoldingTile:
                tileMoving.CancelTileHolding();
                break;
            case TileMoving.State.HoldingAndPointing:
                if (gameState.state == GameState.State.GameOver && trainController.currentTile.Equals(tileMoving.GetReplaceTile())) {
                    tileMoving.FinishTileReplacement(true);
                }
                else tileMoving.CancelTileReplacment();
                break;
        }
    }

    public void UnpauseGameButton()
    {
        gameState.UnpauseGame();
    }

    public void UnpauseGame()
    {
        soundSystem.PlaySound("TrainMovement");
        if (powerupSystem.slowingTime)
        {
            Time.timeScale = powerupSystem.slowTimeAmount;
            Time.fixedDeltaTime = fixedDeltaTime * powerupSystem.slowTimeAmount;
        }
        else
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = fixedDeltaTime;
        }
        pauseMenu.SetActive(false);
        settngsMenu.SetActive(false);
        instruction1.SetActive(false);
        instruction2.SetActive(false);
        instruction3.SetActive(false);
        tileMoving.state = TileMoving.State.Default;
        powerupSystem.EnableButtons();
        ChangeRailsVisibility(true);
    }

    public void ReturnToMainMenu()
    {
        gameState.state = GameState.State.GameOver;
        trainController.trainIsMoving = false;
        trainController.trainCurrentSpeed = 0;
        StartCoroutine(LoadPreviousScene());
    }

    IEnumerator LoadPreviousScene()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = fixedDeltaTime;
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        
        GameObject soundSystemObject = GameObject.Find("SoundSystem");
        if (soundSystemObject != null)
        {
            soundSystemObject.GetComponent<SoundSystem>().RestartMusic();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1); 
    }

    private void ChangeRailsVisibility(bool show)
    {
        Tile[,] tiles = Tiles.tiles;
        for (int x = trainController.currentTile.xIndex - MapGenerator.tilesGeneratedBehind - 1; x < trainController.currentTile.xIndex + MapGenerator.tilesGeneratedAhead + 1; x++)
        {
            for (int z = MapGenerator.oceanWidth; z < MapGenerator.oceanWidth + MapGenerator.mapWidth; z++)
            {    
                if (x >= 0 && z >= 0 && x < tiles.GetLength(0) && z < tiles.GetLength(1))
                {
                    Tile tile = tiles[x, z];
                    if (tile != null && tile.objectRef != null && tile.objectParents != null)
                    {
                        if (tile.objectParents[Tile.ObjectParent.Rails] != null) tile.objectParents[Tile.ObjectParent.Rails].SetActive(show);
                        if (tile.objectParents[Tile.ObjectParent.TrainStation] != null) tile.objectParents[Tile.ObjectParent.TrainStation].SetActive(show);
                        if (tile.objectParents[Tile.ObjectParent.Powerups] != null) tile.objectParents[Tile.ObjectParent.Powerups].SetActive(show);
                    }
                }
            }
        }
    }

    void Update()
    {
        if ((Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape)) && (gameState.state == GameState.State.GameActive || gameState.state == GameState.State.GamePlayerPaused))
        {
            soundSystem.PlaySound("ButtonHover");
            if (gameState.state == GameState.State.GamePlayerPaused)
            {
                gameState.UnpauseGame();
            }
            else
            {
                gameState.PauseGame();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
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
    private bool startingGame = false;

    // Highscores
    public TextMeshProUGUI beginnerHighscoreLevel;
    public TextMeshProUGUI beginnerHighscore;
    public TextMeshProUGUI beginnerDevscoreLevel;
    public TextMeshProUGUI beginnerDevscore;
    public GameObject beginnerTrophyOn;
    public GameObject beginnerTrophyOff;
    public TextMeshProUGUI standardHighscoreLevel;
    public TextMeshProUGUI standardHighscore;
    public TextMeshProUGUI standardDevscoreLevel;
    public TextMeshProUGUI standardDevscore;
    public GameObject standardTrophyOn;
    public GameObject standardTrophyOff;
    public TextMeshProUGUI expertHighscoreLevel;
    public TextMeshProUGUI expertHighscore;
    public TextMeshProUGUI expertDevscoreLevel;
    public TextMeshProUGUI expertDevscore;
    public GameObject expertTrophyOn;
    public GameObject expertTrophyOff;
    public Button standardButton;
    public TextMeshProUGUI standardButtonText;
    public Button expertButton;
    public TextMeshProUGUI expertButtonText;
    public TextMeshProUGUI standardRequirenment;
    public TextMeshProUGUI expertRequirenment;
    public GameObject DifficultyManu;
    public GameObject normalButtons;
    public GameObject secretButtons;

    // hardcoded dev highscores
    public static int beginnerDevscoreLevelV = 18;
    public static int beginnerDevscoreV = 34173;
    public static int standardDevscoreLevelV = 8;
    public static int standardDevscoreV = 10988;
    public static int expertDevscoreLevelV = 4;
    public static int expertDevscoreV = 4498;

    private int beginnerHighscoreLevelV = -1;
    private int beginnerHighscoreV = -1;
    private int standardHighscoreLevelV = -1;
    private int standardHighscoreV = -1;
    private int expertHighscoreLevelV = -1;
    private int expertHighscoreV = -1;

    private string inputString = "";
    private bool kacperMode = false;

    public void Start()
    {
        startingGame = false;
        soundSystem = GameObject.Find("SoundSystem").GetComponent<SoundSystem>();
        graphicsSettings = GameObject.Find("GraphicSettings").GetComponent<GraphicSettings>();
        gameState = GameObject.Find("GameState").GetComponent<GameState>();
        gameState.state = GameState.State.MainMenu;
        musicVolumeSlider.value = soundSystem.musicVolume;
        soundVolumeSlider.value = soundSystem.soundVolume;
        musicVolumeText.text = soundSystem.musicVolume.ToString();
        soundVolumeText.text = soundSystem.soundVolume.ToString();
        musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);
        soundVolumeSlider.onValueChanged.AddListener(ChangeSoundVolume);
        graphicsSettingsDropdown.value = (int) graphicsSettings.currentSetting;
        graphicsSettingsDropdown.onValueChanged.AddListener(ChangeGraphicsSettings);

        // highscores
        beginnerHighscoreLevelV = -1;
        beginnerHighscoreV = -1;
        standardHighscoreLevelV = -1;
        standardHighscoreV = -1;
        expertHighscoreLevelV = -1;
        expertHighscoreV = -1;

        if (PlayerPrefs.HasKey(Difficulty.DifficultySetting.Beginner.ToString() + "BestLevel"))
            beginnerHighscoreLevelV = PlayerPrefs.GetInt(Difficulty.DifficultySetting.Beginner.ToString() + "BestLevel");
        if (PlayerPrefs.HasKey(Difficulty.DifficultySetting.Beginner.ToString() + "BestScore"))
            beginnerHighscoreV = PlayerPrefs.GetInt(Difficulty.DifficultySetting.Beginner.ToString() + "BestScore");
        if (PlayerPrefs.HasKey(Difficulty.DifficultySetting.Standard.ToString() + "BestLevel"))
            standardHighscoreLevelV = PlayerPrefs.GetInt(Difficulty.DifficultySetting.Standard.ToString() + "BestLevel");
        if (PlayerPrefs.HasKey(Difficulty.DifficultySetting.Standard.ToString() + "BestScore"))
            standardHighscoreV = PlayerPrefs.GetInt(Difficulty.DifficultySetting.Standard.ToString() + "BestScore");
        if (PlayerPrefs.HasKey(Difficulty.DifficultySetting.Expert.ToString() + "BestLevel"))
            expertHighscoreLevelV = PlayerPrefs.GetInt(Difficulty.DifficultySetting.Expert.ToString() + "BestLevel");
        if (PlayerPrefs.HasKey(Difficulty.DifficultySetting.Expert.ToString() + "BestScore"))
            expertHighscoreV = PlayerPrefs.GetInt(Difficulty.DifficultySetting.Expert.ToString() + "BestScore");

        if (beginnerHighscoreLevelV >= 0)
            beginnerHighscoreLevel.text = "Level " + beginnerHighscoreLevelV;
        else beginnerHighscoreLevel.text = "Level ---";
        if (beginnerHighscoreV >= 0)
            beginnerHighscore.text = "Score " + beginnerHighscoreV;
        else beginnerHighscore.text = "Score ---";
        if (standardHighscoreLevelV >= 0)
            standardHighscoreLevel.text = "Level " + standardHighscoreLevelV;
        else standardHighscoreLevel.text = "Level ---";
        if (standardHighscoreV >= 0)
            standardHighscore.text = "Score " + standardHighscoreV;
        else standardHighscore.text = "Score ---";
        if (expertHighscoreLevelV >= 0)
            expertHighscoreLevel.text = "Level " + expertHighscoreLevelV;
        else expertHighscoreLevel.text = "Level ---";
        if (expertHighscoreV >= 0)
            expertHighscore.text = "Score " + expertHighscoreV;
        else expertHighscore.text = "Score ---";

        beginnerDevscoreLevel.text = "Level " + beginnerDevscoreLevelV;
        beginnerDevscore.text = "Score " + beginnerDevscoreV;
        standardDevscoreLevel.text = "Level " + standardDevscoreLevelV;
        standardDevscore.text = "Score " + standardDevscoreV;
        expertDevscoreLevel.text = "Level " + expertDevscoreLevelV;
        expertDevscore.text = "Score " + expertDevscoreV;

        if (beginnerHighscoreV > beginnerDevscoreV)
        {
            beginnerTrophyOn.SetActive(true);
            beginnerTrophyOff.SetActive(false);
        }
        if (standardHighscoreV > standardDevscoreV)
        {
            standardTrophyOn.SetActive(true);
            standardTrophyOff.SetActive(false);
        }
        if (expertHighscoreV > expertDevscoreV)
        {
            expertTrophyOn.SetActive(true);
            expertTrophyOff.SetActive(false);
        }

        if (beginnerHighscoreLevelV < Difficulty.standardRequiredLevel)
        {
            standardRequirenment.gameObject.SetActive(true);
            standardRequirenment.text = "Reach level " + Difficulty.standardRequiredLevel + " in beginner difficulty to unlock";
            standardButton.interactable = false;
            standardButton.gameObject.GetComponent<ButtonScript>().enabled = false;
            standardButtonText.color = new Color(standardButtonText.color.r, standardButtonText.color.g, standardButtonText.color.b, 0.5f);
        }
        if (standardHighscoreLevelV < Difficulty.expertRequiredLevel)
        {
            expertRequirenment.gameObject.SetActive(true);
            expertRequirenment.text = "Reach level " + Difficulty.expertRequiredLevel + " in standard difficulty to unlock";
            expertButton.interactable = false;
            expertButton.gameObject.GetComponent<ButtonScript>().enabled = false;
            expertButtonText.color = new Color(expertButtonText.color.r, expertButtonText.color.g, expertButtonText.color.b, 0.5f);
        }
    }

    public void PlayGameBeginner()
    {
        if (!startingGame)
        {
            startingGame = true;
            gameState.difficultySetting = Difficulty.DifficultySetting.Beginner;
            StartCoroutine(LoadNextScene());
        }
    }

    public void PlayGameStandard()
    {
        if (!startingGame && beginnerHighscoreLevelV >= Difficulty.standardRequiredLevel)
        {
            startingGame = true;
            gameState.difficultySetting = Difficulty.DifficultySetting.Standard;
            StartCoroutine(LoadNextScene());
        }
    }

    public void PlayGameExpert()
    {
        if (!startingGame && standardHighscoreLevelV >= Difficulty.expertRequiredLevel)
        {
            startingGame = true;
            gameState.difficultySetting = Difficulty.DifficultySetting.Expert;
            StartCoroutine(LoadNextScene());
        }
    }

    public void PlayGameKacper()
    {
        if (!startingGame)
        {
            startingGame = true;
            gameState.difficultySetting = Difficulty.DifficultySetting.Kacper;
            StartCoroutine(LoadNextScene());
        }
    }

    IEnumerator LoadNextScene()
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        soundSystem.RestartMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
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
        graphicsSettings.ChangeSetting(value, true);
    }

    void Update()
    {
        if (DifficultyManu.activeSelf && !kacperMode)
        {
            if (Input.GetKeyDown(KeyCode.K)) inputString = (inputString.Length > 5 ? inputString.Substring(1) : inputString) + "k";
            else if (Input.GetKeyDown(KeyCode.A)) inputString = (inputString.Length > 5 ? inputString.Substring(1) : inputString) + "a";
            else if (Input.GetKeyDown(KeyCode.C)) inputString = (inputString.Length > 5 ? inputString.Substring(1) : inputString) + "c";
            else if (Input.GetKeyDown(KeyCode.P)) inputString = (inputString.Length > 5 ? inputString.Substring(1) : inputString) + "p";
            else if (Input.GetKeyDown(KeyCode.E)) inputString = (inputString.Length > 5 ? inputString.Substring(1) : inputString) + "e";
            else if (Input.GetKeyDown(KeyCode.R)) inputString = (inputString.Length > 5 ? inputString.Substring(1) : inputString) + "r";
            
            if (inputString.Equals("kacper"))
            {
                kacperMode = true;
                normalButtons.SetActive(false);
                secretButtons.SetActive(true);
            }
        }
    }
}

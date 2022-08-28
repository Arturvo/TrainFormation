using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState instance;
    public int currentScore = 0;
    public int currentLevel = 1;
    public int bestScore = 0;
    public int bestLevel = 1;
    public Difficulty.DifficultySetting difficultySetting = Difficulty.DifficultySetting.Beginner;
    public State state = State.MainMenu;

    private UIManager uiManager;

    public float gameStartDelay = 1.5f;
    public bool FPSCapped = false;
    public bool cheating = false;

    public enum State
    {
        MainMenu,
        GameStarting,
        GameActive,
        GamePlayerPaused,
        GameTutorialPaused,
        GameOver
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

        // limit frame rate to 240. Otherwise it goes to 1000 for no reason
        if (!PlayerPrefs.HasKey("FPSCapped") || PlayerPrefs.GetInt("FPSCapped") == 1)
        {
            Application.targetFrameRate = 150;
            FPSCapped = true;
        }        
    }

    public void StartByScheduler(UIManager uiManager)
    {
        state = State.GameStarting;
        currentScore = 0;
        currentLevel = 1;
        this.uiManager = uiManager;
        SetLevel(currentLevel);
        SetScore(currentScore);

        if (PlayerPrefs.HasKey(difficultySetting.ToString() + "BestLevel")) bestLevel = PlayerPrefs.GetInt(difficultySetting.ToString() + "BestLevel");
        if (PlayerPrefs.HasKey(difficultySetting.ToString() + "BestScore")) bestScore = PlayerPrefs.GetInt(difficultySetting.ToString() + "BestScore");

        StartCoroutine(ActivateGame());
    }

    private IEnumerator ActivateGame()
    {
        yield return new WaitForSeconds(gameStartDelay);
        state = State.GameActive;
        uiManager.CheckForFirstInstruction();
    }

    public void SetLevel(int level, bool animation = false)
    {
        currentLevel = level;
        uiManager.SetLevel(level, animation);

        Achievements achievements = FindObjectOfType<Achievements>();
        if (level == 2 && difficultySetting == Difficulty.DifficultySetting.Beginner)
        {
            achievements.GrantAchievement(Achievements.AchievementApiName.ACH_REACH_LEVEL_2);
        }
        if (level == 3 && difficultySetting == Difficulty.DifficultySetting.Beginner)
        {
            achievements.GrantAchievement(Achievements.AchievementApiName.ACH_REACH_LEVEL_3);
        }
        if (level - 1 < Difficulty.difficultyMapping[difficultySetting].levelConfiguration.Length 
            && Difficulty.difficultyMapping[difficultySetting].levelConfiguration[level - 1].maxElevation == 4)
        {
            achievements.GrantAchievement(Achievements.AchievementApiName.ACH_REACH_SNOW);
        }
    }

    public void SetScore(int score)
    {
        currentScore = score;
        uiManager.SetScore(score);
    }

    public void GameOver()
    {
        Achievements achievements = FindObjectOfType<Achievements>();
        achievements.GrantAchievement(Achievements.AchievementApiName.ACH_FIRST_GAME_OVER);

        state = State.GameOver;
        bool newBestLevel = false;
        bool newBestScore = false;
        
        if (!cheating)
        {
            if (currentLevel > bestLevel || !PlayerPrefs.HasKey(difficultySetting.ToString() + "BestLevel"))
            {
                bestLevel = currentLevel;
                newBestLevel = true;
                PlayerPrefs.SetInt(difficultySetting.ToString() + "BestLevel", currentLevel);

                if (difficultySetting == Difficulty.DifficultySetting.Beginner && currentLevel >= Difficulty.standardRequiredLevel)
                {
                    achievements.GrantAchievement(Achievements.AchievementApiName.ACH_UNLOCK_STANDARD_DIFF);
                }
                if (difficultySetting == Difficulty.DifficultySetting.Standard && currentLevel >= Difficulty.expertRequiredLevel)
                {
                    achievements.GrantAchievement(Achievements.AchievementApiName.ACH_UNLOCK_EXPERT_DIFF);
                }
            }
            if (currentScore > bestScore || !PlayerPrefs.HasKey(difficultySetting.ToString() + "BestScore"))
            {
                bestScore = currentScore;
                newBestScore = true;
                PlayerPrefs.SetInt(difficultySetting.ToString() + "BestScore", currentScore);

                if ((difficultySetting == Difficulty.DifficultySetting.Beginner && currentScore > Menu.beginnerDevscoreV) ||
                    (difficultySetting == Difficulty.DifficultySetting.Standard && currentScore > Menu.standardDevscoreV) ||
                    (difficultySetting == Difficulty.DifficultySetting.Expert && currentScore > Menu.expertDevscoreV))
                {
                    achievements.GrantAchievement(Achievements.AchievementApiName.ACH_BEAT_THE_DEV);
                }
            }
        } 
        uiManager.GameOver(newBestLevel, newBestScore);
    }

    public void PauseGame()
    {
        if (state == State.GameActive)
        {
            state = State.GamePlayerPaused;
            uiManager.PauseGame(UIManager.PouseType.PlayerPouse);
        }
    }

    public void UnpauseGame()
    {
        if (state == State.GamePlayerPaused || state == State.GameTutorialPaused)
        {
            state = State.GameActive;
            uiManager.UnpauseGame();
        }
    }

    void Update()
    {
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F3))
        {
            PlayerPrefs.SetInt(Difficulty.DifficultySetting.Beginner.ToString() + "BestLevel", Difficulty.standardRequiredLevel);
            PlayerPrefs.SetInt(Difficulty.DifficultySetting.Standard.ToString() + "BestLevel", Difficulty.expertRequiredLevel);
        }
        else if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F4))
        {
            foreach (Difficulty.DifficultySetting difficulty in Difficulty.DifficultySetting.GetValues(typeof(Difficulty.DifficultySetting)))
            {
                if (PlayerPrefs.HasKey(difficulty.ToString() + "BestLevel")) PlayerPrefs.DeleteKey(difficulty.ToString() + "BestLevel");
                if (PlayerPrefs.HasKey(difficulty.ToString() + "BestScore")) PlayerPrefs.DeleteKey(difficulty.ToString() + "BestScore");
            }
        }
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F2))
        {
            if (FPSCapped)
            {
                FPSCapped = false;
                Application.targetFrameRate = -1;
                PlayerPrefs.SetInt("FPSCapped", 0);
            }
            else
            {
                FPSCapped = true;
                Application.targetFrameRate = 240;
                PlayerPrefs.SetInt("FPSCapped", 1);
            }
        }
    }
}

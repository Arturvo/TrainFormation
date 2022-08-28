using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PowerupSystem : MonoBehaviour
{
    public TextMeshProUGUI slowTimeCountText;
    public TextMeshProUGUI moreRailsCountText;
    public TextMeshProUGUI moveStationCountText;
    public MapGenerator mapGenerator;
    private SoundSystem soundSystem;
    private GameState gameState;

    public Button slowTimeCountButton;
    public Button moreRailsCountButton;
    public Button moveStationCountButton;

    public GameObject slowTimeDisableImage;
    public GameObject moreRailsDisableImage;
    public GameObject moveStationDisableImage;
    private Image slowTimeDisableImageComponent;

    public int slowTimeCount = 0;
    public int moreRailsCount = 0;
    public int moveStationCount = 0;

    public float slowTimeDuration = 5;
    public float slowTimeAmount = 0.1f;
    public bool slowingTime = false;
    public bool canMoveStation = false;
    public int moreRailsNumber = 10;
    public int maxNewRailDistance = 5;

    private float slowTimePassed = 0;
    private float fixedDeltaTime;

    // used to track achievement
    private int slowTimeUsedInARow = 0;

    public enum PowerupType
    {
        NoPowerup,
        SlowTimePowerup,
        MoreRailsPowerup,
        MoveStationPowerup
    }

    private void Awake()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
        slowTimeCountText.text = slowTimeCount.ToString();
        moreRailsCountText.text = moreRailsCount.ToString();
        moveStationCountText.text = moveStationCount.ToString();
        slowTimeDisableImageComponent = slowTimeDisableImage.GetComponent<Image>();
        gameState = GameObject.Find("GameState").GetComponent<GameState>();
        soundSystem = GameObject.Find("SoundSystem").GetComponent<SoundSystem>();
    }

    public void SlowTimePowerup()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (slowTimeCount == 0 || slowingTime || gameState.state != GameState.State.GameActive) 
        {
            soundSystem.PlaySound("NoPowerup");
            return;
        }

        slowTimeUsedInARow += 1;
        if (slowTimeUsedInARow == 3)
        {
            Achievements achievements = FindObjectOfType<Achievements>();
            achievements.GrantAchievement(Achievements.AchievementApiName.ACH_3_SLOW_TIME_IN_A_ROW);
        }

        soundSystem.PlaySound("SlowTime");
        slowTimePassed = 0;
        slowingTime = true;
        slowTimeCount -= 1;
        slowTimeCountText.text = slowTimeCount.ToString();
        Time.timeScale = slowTimeAmount;
        Time.fixedDeltaTime = fixedDeltaTime * slowTimeAmount;
        slowTimeCountButton.interactable = false;
        slowTimeDisableImage.SetActive(true);
        StartCoroutine(ReturnTimeSpeed(slowTimeDuration * slowTimeAmount));
    }
    public void MoreRailsPowerup()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (moreRailsCount == 0 || gameState.state != GameState.State.GameActive)
        {
            soundSystem.PlaySound("NoPowerup");
            return;
        }

        slowTimeUsedInARow = 0;
        soundSystem.PlaySound("MoreRails");
        moreRailsCount -= 1;
        moreRailsCountText.text = moreRailsCount.ToString();
        mapGenerator.GenerateMoreRails(moreRailsNumber, maxNewRailDistance);
    }

    public void MoveStationPowerup()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (moveStationCount == 0 || canMoveStation || gameState.state != GameState.State.GameActive)
        {
            soundSystem.PlaySound("NoPowerup");
            return;
        }

        slowTimeUsedInARow = 0;
        soundSystem.PlaySound("MoveStation");
        canMoveStation = true;
        moveStationCount -= 1;
        moveStationCountText.text = moveStationCount.ToString();
        moveStationCountButton.interactable = false;
        moveStationDisableImage.SetActive(true);
    }

    public void CancelSlowTimeSpeed()
    {
        slowTimeCountButton.interactable = true;
        slowTimeDisableImage.SetActive(false);
        slowingTime = false;
        Time.timeScale = 1;
        Time.fixedDeltaTime = fixedDeltaTime;
        slowTimeDisableImageComponent.fillAmount = 1;

    }

    private IEnumerator ReturnTimeSpeed(float time)
    {
        yield return new WaitForSeconds(time);
        if (slowingTime)
        {
            soundSystem.PlaySound("ReturnTime");
            slowTimeCountButton.interactable = true;
            slowTimeDisableImage.SetActive(false);
            slowingTime = false;
            Time.timeScale = 1;
            Time.fixedDeltaTime = fixedDeltaTime;
            slowTimeDisableImageComponent.fillAmount = 1;
        }
    }

    public void StationMoved()
    {
        moveStationCountButton.interactable = true;
        moveStationDisableImage.SetActive(false);
        canMoveStation = false;
    }

    public void DisableButtons()
    {
        slowTimeCountButton.interactable = false;
        moreRailsCountButton.interactable = false;
        moveStationCountButton.interactable = false;
    }

    public void EnableButtons()
    {
        slowTimeCountButton.interactable = true;
        moreRailsCountButton.interactable = true;
        moveStationCountButton.interactable = true;
    }

    public void HideButtons()
    {
        DisableButtons();
        slowTimeCountButton.gameObject.SetActive(false);
        moreRailsCountButton.gameObject.SetActive(false);
        moveStationCountButton.gameObject.SetActive(false);
    }

    public void CollectPowerup(Tile tile)
    {
        switch (tile.powerup)
        {
            case PowerupType.SlowTimePowerup:
                slowTimeCount += 1;
                slowTimeCountText.text = slowTimeCount.ToString();
                break;
            case PowerupType.MoreRailsPowerup:
                moreRailsCount += 1;
                moreRailsCountText.text = moreRailsCount.ToString();
                break;
            case PowerupType.MoveStationPowerup:
                moveStationCount += 1;
                moveStationCountText.text = moveStationCount.ToString();
                break;
        }

        tile.powerup = PowerupType.NoPowerup;
        Destroy(tile.powerupRef);
        soundSystem.PlaySound("CollectPowerup");
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState.state == GameState.State.GameActive)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                SlowTimePowerup();
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                MoreRailsPowerup();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                MoveStationPowerup();
            }
        }
        if (slowingTime)
        {
            slowTimePassed += Time.deltaTime * (1/slowTimeAmount);
            slowTimeDisableImageComponent.fillAmount = Mathf.Lerp(1, 0, slowTimePassed / slowTimeDuration);
        }
    }
}

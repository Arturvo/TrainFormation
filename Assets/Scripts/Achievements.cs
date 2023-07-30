using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievements : MonoBehaviour
{
    public enum AchievementApiName
    {
        ACH_FIRST_GAME_OVER,
        ACH_REACH_LEVEL_2,
        ACH_REACH_LEVEL_3,
        ACH_3_SLOW_TIME_IN_A_ROW,
        ACH_UNLOCK_STANDARD_DIFF,
        ACH_REACH_SNOW,
        ACH_UNLOCK_EXPERT_DIFF,
        ACH_BEAT_THE_DEV
    }

    public void GrantAchievement(AchievementApiName achievementApiName)
    {
        if (!SteamManager.Initialized) return;

        SteamUserStats.SetAchievement(achievementApiName.ToString());
        SteamUserStats.StoreStats();

        //Debug.Log("Granting achievement: " + achievementApiName.ToString());
    }
}

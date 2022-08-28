using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Difficulty
{
    // requirenemnt to unlock difficulties
    public static int standardRequiredLevel = 4;
    public static int expertRequiredLevel = 4;

    public static Dictionary<DifficultySetting, DifficultyProperties> difficultyMapping = new Dictionary<DifficultySetting, DifficultyProperties>()
    {
        {DifficultySetting.Beginner, new DifficultyProperties()
            {
                railsChance = 0.7f,
                stationStopDuration = 4f,
                trainSpeedIncrease = 0.002f,
                trainSpeedIncreaseIncrease = 0.00025f,
                trainStartSpeed = 2f,
                trainStartSpeedBonus = 0.2f,
                fistSegmentSpeedSlow = 0.5f,
                carriageNumber = 8,
                repeatLastLevels = 2,
                segmentStartLength = 15,
                segmentLengthIncrease = 5,
                kacperMode = false,
                levelConfiguration =  new MapGenerator.LevelConfiguration[]
                {
                    new MapGenerator.LevelConfiguration(2, false),
                    new MapGenerator.LevelConfiguration(3, false),
                    new MapGenerator.LevelConfiguration(2, true),
                }
            }
        },
        {DifficultySetting.Standard, new DifficultyProperties()
            {
                railsChance = 0.65f,
                stationStopDuration = 3.5f,
                trainSpeedIncrease = 0.004f,
                trainSpeedIncreaseIncrease = 0.00033f,
                trainStartSpeed = 3f,
                trainStartSpeedBonus = 0.3f,
                fistSegmentSpeedSlow = 1f,
                carriageNumber = 6,
                repeatLastLevels = 3,
                segmentStartLength = 25,
                segmentLengthIncrease = 6,
                kacperMode = false,
                levelConfiguration =  new MapGenerator.LevelConfiguration[]
                {
                    new MapGenerator.LevelConfiguration(3, false),
                    new MapGenerator.LevelConfiguration(2, true),
                    new MapGenerator.LevelConfiguration(3, false),
                    new MapGenerator.LevelConfiguration(3, true),
                    new MapGenerator.LevelConfiguration(4, false),
                    new MapGenerator.LevelConfiguration(3, true),
                    new MapGenerator.LevelConfiguration(3, false),
                }
            }
        },
        {DifficultySetting.Expert, new DifficultyProperties()
            {
                railsChance = 0.6f,
                stationStopDuration = 3f,
                trainSpeedIncrease = 0.007f,
                trainSpeedIncreaseIncrease = 0.0005f,
                trainStartSpeed = 3.5f,
                trainStartSpeedBonus = 0.5f,
                fistSegmentSpeedSlow = 1f,
                carriageNumber = 4,
                repeatLastLevels = 2,
                segmentStartLength = 30,
                segmentLengthIncrease = 8,
                kacperMode = false,
                levelConfiguration =  new MapGenerator.LevelConfiguration[]
                {
                    new MapGenerator.LevelConfiguration(3, false),
                    new MapGenerator.LevelConfiguration(3, true),
                    new MapGenerator.LevelConfiguration(4, false),
                    new MapGenerator.LevelConfiguration(3, true),
                    new MapGenerator.LevelConfiguration(4, false),
                    new MapGenerator.LevelConfiguration(4, true),
                }
            }
        },
        {DifficultySetting.Kacper, new DifficultyProperties()
            {
                railsChance = 0.5f,
                stationStopDuration = 3f,
                trainSpeedIncrease = 0f,
                trainSpeedIncreaseIncrease = 0f,
                trainStartSpeed = 4f,
                trainStartSpeedBonus = 0f,
                fistSegmentSpeedSlow = 1f,
                carriageNumber = 10,
                repeatLastLevels = 1,
                segmentStartLength = 20,
                segmentLengthIncrease = 0,
                kacperMode = true,
                levelConfiguration =  new MapGenerator.LevelConfiguration[]
                {
                    new MapGenerator.LevelConfiguration(2, false)
                }
            }
        }
    };

    public enum DifficultySetting
    {
        Kacper,
        Beginner,
        Standard,
        Expert
    }

    public class DifficultyProperties
    {
        public float stationStopDuration;
        public float trainSpeedIncrease;
        public float trainSpeedIncreaseIncrease;
        public float trainStartSpeed;
        public float railsChance;
        // how much is added to starting train speed after the reset on each segment
        public float trainStartSpeedBonus;
        // first segement slow to counter the fact we are not starting from first hex
        public float fistSegmentSpeedSlow;
        // how many carriages does train have
        public int carriageNumber;
        // unique level configurations
        public MapGenerator.LevelConfiguration[] levelConfiguration;
        // how many last levels are repeated when run out of unique configurations
        public int repeatLastLevels;
        public int segmentStartLength = 20;
        public int segmentLengthIncrease = 5;
        public bool kacperMode;
    }
}

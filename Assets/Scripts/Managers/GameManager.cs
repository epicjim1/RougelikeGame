using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Starting State")]
    public string level;
    public string startingWeapon;
    public string modifier;
    public float maxHealth = 50f;

    [Header("Level Configurations")]
    public LevelConfiguration[] levelConfigurations;

    private Dictionary<string, LevelConfiguration> levelConfigDict;
    private LevelConfiguration currentLevelConfig;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLevelConfigurations();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLevelConfigurations()
    {
        levelConfigDict = new Dictionary<string, LevelConfiguration>();

        foreach (var config in levelConfigurations)
        {
            if (config != null)
            {
                levelConfigDict[config.levelName] = config;
            }
        }
    }

    public void SetSpinResults(string level, string startingWeapon, string modifier)
    {
        this.level = level;
        this.startingWeapon = startingWeapon;
        this.modifier = modifier;

        // Set the current level configuration
        if (levelConfigDict.ContainsKey(level))
        {
            currentLevelConfig = levelConfigDict[level];
        }
        else
        {
            Debug.LogWarning($"Level configuration not found for: {level}");
            // Fallback to first available configuration
            if (levelConfigurations.Length > 0)
            {
                currentLevelConfig = levelConfigurations[0];
            }
        }

        Debug.Log($"Spin Results -> Level: {level}, StartingWeapon: {startingWeapon}, Modifier: {modifier}");
    }

    public LevelConfiguration GetCurrentLevelConfig()
    {
        return currentLevelConfig;
    }

    public LevelConfiguration GetLevelConfig(string levelName)
    {
        if (levelConfigDict.ContainsKey(levelName))
        {
            return levelConfigDict[levelName];
        }

        Debug.LogWarning($"Level configuration not found for: {levelName}");
        return null;
    }

    // Helper methods for easy access to current level data
    public RoomTemplate[] GetCurrentRoomTemplates()
    {
        return currentLevelConfig?.roomTemplates ?? new RoomTemplate[0];
    }

    public RoomTemplate GetCurrentPlayerSpawnTemplate()
    {
        return currentLevelConfig?.playerSpawnTemplate;
    }

    public RoomTemplate GetCurrentBossRoomTemplate()
    {
        return currentLevelConfig?.bossRoomTemplate;
    }

    public RoomTemplate GetCurrentExitRoomTemplate()
    {
        return currentLevelConfig?.exitRoomTemplate;
    }
}

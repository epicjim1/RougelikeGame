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
    public int stage = 0;

    [Header("Level Configurations")]
    public LevelConfiguration[] levelConfigurations;
    private Dictionary<string, LevelConfiguration> levelConfigDict;
    private LevelConfiguration currentLevelConfig;

    [Header("Player Coins")]
    public int totalPlayerCoins;
    public int currentRunPlayerCoins;
    private const string TOTAL_COINS_PREF_KEY = "TotalPlayerCoins";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLevelConfigurations();
            LoadTotalPlayerCoins();
            if (stage == 0)
                currentRunPlayerCoins = 0;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadTotalPlayerCoins()
    {
        totalPlayerCoins = PlayerPrefs.GetInt(TOTAL_COINS_PREF_KEY, 0);
        Debug.Log($"Loaded Total Player Coins: {totalPlayerCoins}");
    }

    private void SaveTotalPlayerCoins()
    {
        PlayerPrefs.SetInt(TOTAL_COINS_PREF_KEY, totalPlayerCoins);
        PlayerPrefs.Save();
        Debug.Log($"Saved Total Player Coins: {totalPlayerCoins}");
    }

    public void AddCoinsToCurrentRun(int amount)
    {
        currentRunPlayerCoins += amount;
        Debug.Log($"Collected {amount} coins. Current Run Coins: {currentRunPlayerCoins}");
    }

    public void EndRunAndSaveCoins()
    {
        totalPlayerCoins += currentRunPlayerCoins;
        SaveTotalPlayerCoins();
        currentRunPlayerCoins = 0; // Reset current run coins for the next run
        Debug.Log("Run ended. Current run coins added to total and saved.");
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

    public RoomTemplate GetCurrentKeyRoomTemplate()
    {
        return currentLevelConfig?.keyRoomTemplate;
    }
}

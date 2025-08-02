using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Starting State")]
    public string level;
    public string startingWeapon;
    public BossType bossType;
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

    [HideInInspector] public bool GameIsPaused = false;
    [HideInInspector] public bool GameIsLost = false;

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

    public void NextStage(float currentMaxHealth)
    {
        stage += 1;
        maxHealth = currentMaxHealth;
        currentLevelConfig.gridSize = new Vector2Int(currentLevelConfig.gridSize.x + 50, currentLevelConfig.gridSize.y + 50);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    public void SetSpinResults(string level, string startingWeapon, string bt, string modifier)
    {
        this.level = level;
        this.startingWeapon = startingWeapon;
        this.bossType = Enum.TryParse<BossType>(bt, true, out var parsedBossType)
            ? parsedBossType
            : BossType.ElementalGolem;
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

        Debug.Log($"Spin Results -> Level: {level}, StartingWeapon: {startingWeapon}, Boss: {this.bossType}, Modifier: {modifier}");
    }

    public LevelConfiguration GetCurrentLevelConfig()
    {
        return currentLevelConfig;
    }

    public void ResetGame()
    {
        GameIsLost = false;
        GameIsPaused = false;
        stage = 0;
        currentRunPlayerCoins = 0;
        maxHealth = 50f;
        level = "";
        startingWeapon = "";
        modifier = "";
    }

    /*public LevelConfiguration GetLevelConfig(string levelName)
    {
        if (levelConfigDict.ContainsKey(levelName))
        {
            return levelConfigDict[levelName];
        }

        Debug.LogWarning($"Level configuration not found for: {levelName}");
        return null;
    }

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
    }*/
}

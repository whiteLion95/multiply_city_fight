using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Deslab.UI;
using Deslab.Utils;
using Deslab.Level;
//using Deslab.Scripts.Deslytics;


/// <summary>
/// Containt all player data.
/// </summary>
[Serializable]
public class PlayerData
{
    public string name;
    public int money;
    public int equippedHatID;
    public int equippedWeaponID;
    public int changeColorCounter;
    public int currentColorScheme;
    public List<PlayerUpgradeData> playerUpgradeData = new List<PlayerUpgradeData>(3);
    public bool allLevelComplete = false;
}

/// <summary>
/// Class for upgrade some stats with fixed step and increasing cost.
/// </summary>
[Serializable]
public class PlayerUpgradeData
{
    public int costMultiply = 20;
    public float stepUpgrade = 0;
    public int upgradeLevel = 1;
    public int upgradeCost = 110;

    public void Upgrade()
    {
        stepUpgrade += 0.02f;
        upgradeLevel += 1;
        costMultiply += costMultiply / 6;
        upgradeCost += costMultiply;
    }
}

/// <summary>
/// Game states
/// </summary>
public enum GameStatus
{
    Menu, Game
}

/// <summary>
/// Singleton. Manager for game states, levels, player data.
/// </summary>
public class StaticManager : Singleton<StaticManager>
{
    public PlayerData playerData;
    public GameStatus gameStatus;
    public LevelManager levelManager;

    [BoxGroup("DEBUG MODE", true, true)]
    [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
    public bool debugMode = false;
    [ShowIf("debugMode")]
    [BoxGroup("DEBUG MODE", true, true)]
    public int debugLevel = 0;

    /// <summary>
    /// Called when money count changed.
    /// </summary>
    public delegate void OnMoneyChange();
    public static event OnMoneyChange onMoneyChange;

    public static int levelID = 1;
    internal static int starsCollected = 0;
    internal static int moneyCollectedOnLevel;
    internal static int clonesCollectedOnLevel;
    internal static float currentMoneyMultiplier = 1;
    internal static float currentMultiplier;
    internal static bool reloadLevel = false;
    internal static bool victory = false;
    internal static bool reviveReload = false;
    internal static bool allLevelsReached = false;
    internal static int restartLevelID = -1;

    /// <summary>
    /// What minimum level can be loaded after completing all levels
    /// </summary>
    internal static int minLevelAfterCompleting = 0;
    internal static int minMoneyCount = 10;

    private static RandomNoRepeate randomLevelsNoRepeate = new RandomNoRepeate();
    private int uniqueLevelsCount;

    public static Action OnWin;
    public static Action OnLose;

    protected override void Awake()
    {
        base.Awake();

        uniqueLevelsCount = levelManager.Levels.Count;

        if (debugMode)
        {
            if (debugLevel > (uniqueLevelsCount))
            {
                debugLevel = uniqueLevelsCount;
                Debug.Log("Debug level was bigger than last unique level, therefore debug level equals to the last unique level number: " + debugLevel);
            }

            levelID = debugLevel;
        }
        
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;

        victory = false;
    }

    private void Start()
    {
        //DeslyticsManager.SetUniqueLevelsCount(uniqueLevelsCount);
        //SavePlayerData();
        InitRandomLevels(uniqueLevelsCount);
        LoadPlayerData();
    }

    #region Player Data

    /// <summary>
    /// Load player data from PlayerPrefs.
    /// </summary>
    public void LoadPlayerData()
    {
        //if (!instance.debugMode)
        //    if (ES3.KeyExists("LevelID"))
        //        levelID = ES3.Load<int>("LevelID");

        if (!debugMode)
            levelID = PlayerPrefs.GetInt("LevelID", 1);

        if (PlayerPrefs.HasKey("PlayerData"))
        {
            string jsonData = PlayerPrefs.GetString("PlayerData");
            playerData = JsonConvert.DeserializeObject<PlayerData>(jsonData);
        }

        levelManager.LoadLevelFromResources();
        UIManager.Instance?.SetUpgradesStats(playerData.playerUpgradeData);
        MoneyValueChanged();
    }

    /// <summary>
    /// Save player data to PlayerPrefs.
    /// </summary>
    public void SavePlayerData()
    {
        PlayerPrefs.SetInt("LevelID", levelID);

        string serializedData = JsonConvert.SerializeObject(playerData, Formatting.None, new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        
        PlayerPrefs.SetString("PlayerData", serializedData);
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SavePlayerData();
        }
    }

    private void OnDisable()
    {
        SavePlayerData();
    }
    #endregion

    #region Money

    /// <summary>
    /// Adds money count to PlayerData
    /// </summary>
    /// <param name="count">Money count</param>
    public static void AddMoney(int count)
    {
        Instance.playerData.money += count;
        MoneyValueChanged();
    }

    /// <summary>
    /// Substract money count from PlayerData
    /// </summary>
    /// <param name="count">Money count</param>
    public static void SubstractMoney(int count)
    {
        Instance.playerData.money -= count;
        MoneyValueChanged();
    }

    /// <summary>
    /// Collect money to temp variable for show collected money on level.
    /// After showing Win or Lose Screen and clicked Claim button then need to call AddMoney(int count);
    /// </summary>
    /// <param name="count"></param>
    public static void AddMoneyOnLevel(int count)
    {
        moneyCollectedOnLevel += count;
        MoneyValueChanged();
    }

    /// <summary>
    /// When we change money value we call that event for Vibrate and call all subscribed scripts
    /// </summary>
    private static void MoneyValueChanged()
    {
        if (onMoneyChange != null)
        {
            onMoneyChange();
            HapticsManager.Instance.LightVibrate();
        }
    }
    #endregion

    #region Level Management
    /// <summary>
    /// Initialize random levels list what we will load from resources if player all levels completed
    /// </summary>
    /// <param name="levelsCount">Amount of levels placed as prefab in Resources/Levels/ </param>
    private static void InitRandomLevels(int levelsCount)
    {
        randomLevelsNoRepeate.SetCount(levelsCount);
    }

    /// <summary>
    /// Get current level id. If player completed all levels return random level id 
    /// from 5 level to levelsCount setted in InitRandomLevels
    /// </summary>
    /// <returns></returns>
    public static int GetLevelID()
    {
        if (Instance.debugMode)
        {
            return levelID;
        }

        if (victory)
        {
            if (!Instance.debugMode)
            {
                levelID++;
                Instance.playerData.changeColorCounter++;
            }
        }

        if (Instance.playerData.allLevelComplete)
        {
            if (reloadLevel)
            {
                return restartLevelID;
            }

            int newID;
            if (minLevelAfterCompleting >= Instance.uniqueLevelsCount)
            {
                Debug.LogError("Min level after completing can't be higher then uniqueLevelsCount!");
                newID = Instance.uniqueLevelsCount;
            }
            else
            {
                newID = randomLevelsNoRepeate.GetAvailable(minLevelAfterCompleting) + 1;
            }

            restartLevelID = newID;
            return newID;
        }

        if (levelID < 1)
        {
            levelID = 1;
        }

        return levelID;
    }

    public delegate void OnRestart();
    public static event OnRestart onRestart;

    /// <summary>
    /// When level completed or failed we need to restart temp variables
    /// </summary>
    public static void Restart()
    {
        moneyCollectedOnLevel = 0;
        starsCollected = 0;
        currentMoneyMultiplier = 0;
        reloadLevel = true;
        onRestart?.Invoke();
    }

    public static void LoadNextLevel()
    {
        if (victory && levelID == Instance.uniqueLevelsCount)
        {
            Instance.playerData.allLevelComplete = true;
        }

        Instance.levelManager.LoadLevelFromResources();
        victory = false;
    }
    #endregion
}

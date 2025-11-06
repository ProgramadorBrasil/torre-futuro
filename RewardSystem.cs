using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Comprehensive Reward System with XP, credits, kill streaks, combos, and achievement tracking
/// Integrates with all game systems to provide meaningful progression and feedback
/// </summary>
public class RewardSystem : MonoBehaviour
{
    #region Reward Types
    public enum RewardType
    {
        Credits,
        XP,
        Achievement,
        Unlock,
        Bonus
    }

    public enum KillType
    {
        Standard,
        Elite,
        Boss,
        Headshot,
        Multikill
    }

    [Serializable]
    public class RewardData
    {
        public RewardType type;
        public int amount;
        public string description;
        public Sprite icon;
    }

    [Serializable]
    public class Achievement
    {
        public string id;
        public string name;
        public string description;
        public int targetValue;
        public int currentValue;
        public bool unlocked;
        public int rewardCredits;
        public int rewardXP;
        public Sprite icon;
    }
    #endregion

    [Header("Credit Rewards")]
    [SerializeField] private int standardKillCredits = 50;
    [SerializeField] private int eliteKillCredits = 150;
    [SerializeField] private int bossKillCredits = 500;
    [SerializeField] private int missionCompleteCredits = 300;

    [Header("XP Rewards")]
    [SerializeField] private int standardKillXP = 25;
    [SerializeField] private int eliteKillXP = 75;
    [SerializeField] private int bossKillXP = 200;
    [SerializeField] private int missionCompleteXP = 150;

    [Header("Streak System")]
    [SerializeField] private int currentKillStreak = 0;
    [SerializeField] private int maxKillStreak = 0;
    [SerializeField] private float streakTimeWindow = 5f; // Time to maintain streak
    [SerializeField] private float streakMultiplier = 0.1f; // 10% bonus per streak level
    [SerializeField] private int[] streakMilestones = { 5, 10, 20, 50, 100 };

    [Header("Combo System")]
    [SerializeField] private int currentCombo = 0;
    [SerializeField] private int maxCombo = 0;
    [SerializeField] private float comboTimeLimit = 3f;
    [SerializeField] private float comboMultiplier = 1f;

    [Header("Multipliers")]
    [SerializeField] private float difficultyMultiplier = 1f;
    [SerializeField] private float eventMultiplier = 1f; // Special events bonus
    [SerializeField] private float vipMultiplier = 1f;   // VIP/Premium bonus

    [Header("Statistics")]
    [SerializeField] private int totalKills = 0;
    [SerializeField] private int totalDeaths = 0;
    [SerializeField] private int totalCreditsEarned = 0;
    [SerializeField] private int totalXPEarned = 0;
    [SerializeField] private int totalMissionsCompleted = 0;
    [SerializeField] private float totalPlayTime = 0f;

    [Header("Achievements")]
    [SerializeField] private List<Achievement> achievements = new List<Achievement>();

    [Header("Bonus Rewards")]
    [SerializeField] private bool enableDailyBonus = true;
    [SerializeField] private int dailyBonusCredits = 100;
    [SerializeField] private DateTime lastDailyBonusClaimed;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject rewardPopupPrefab;
    [SerializeField] private GameObject streakEffectPrefab;
    [SerializeField] private GameObject achievementPanelPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip creditsEarnedSound;
    [SerializeField] private AudioClip xpEarnedSound;
    [SerializeField] private AudioClip streakSound;
    [SerializeField] private AudioClip achievementSound;
    [SerializeField] private AudioClip levelUpSound;

    // Private variables
    private float lastKillTime = 0f;
    private float lastComboTime = 0f;
    private List<RewardData> pendingRewards = new Queue<RewardData>().ToArray() as List<RewardData>;
    private Dictionary<string, Achievement> achievementDict = new Dictionary<string, Achievement>();

    // References
    private GameplayUI gameplayUI;
    private UpgradeSystem upgradeSystem;

    // Events
    public delegate void RewardEvent(int amount);
    public delegate void StreakEvent(int streak);
    public delegate void AchievementEvent(Achievement achievement);
    public event RewardEvent OnCreditsEarned;
    public event RewardEvent OnXPEarned;
    public event StreakEvent OnStreakChanged;
    public event StreakEvent OnStreakMilestone;
    public event AchievementEvent OnAchievementUnlocked;
    public event System.Action<int, int> OnComboChanged; // combo count, multiplier

    #region Initialization

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        FindReferences();
        InitializeAchievements();
        LoadPlayerStats();
    }

    private void Start()
    {
        CheckDailyBonus();
    }

    private void Update()
    {
        UpdateTimers();
        totalPlayTime += Time.deltaTime;
    }

    private void OnDestroy()
    {
        SavePlayerStats();
    }

    private void FindReferences()
    {
        if (gameplayUI == null)
            gameplayUI = FindObjectOfType<GameplayUI>();

        if (upgradeSystem == null)
            upgradeSystem = FindObjectOfType<UpgradeSystem>();
    }

    private void InitializeAchievements()
    {
        if (achievements.Count == 0)
        {
            CreateDefaultAchievements();
        }

        achievementDict.Clear();
        foreach (var achievement in achievements)
        {
            if (!achievementDict.ContainsKey(achievement.id))
            {
                achievementDict.Add(achievement.id, achievement);
            }
        }
    }

    private void CreateDefaultAchievements()
    {
        // First Blood
        achievements.Add(new Achievement
        {
            id = "first_kill",
            name = "First Blood",
            description = "Destroy your first enemy",
            targetValue = 1,
            currentValue = 0,
            unlocked = false,
            rewardCredits = 100,
            rewardXP = 50
        });

        // Kill Streak achievements
        achievements.Add(new Achievement
        {
            id = "killing_spree",
            name = "Killing Spree",
            description = "Get a 5 kill streak",
            targetValue = 5,
            currentValue = 0,
            unlocked = false,
            rewardCredits = 200,
            rewardXP = 100
        });

        achievements.Add(new Achievement
        {
            id = "rampage",
            name = "Rampage",
            description = "Get a 10 kill streak",
            targetValue = 10,
            currentValue = 0,
            unlocked = false,
            rewardCredits = 500,
            rewardXP = 250
        });

        // Total kills
        achievements.Add(new Achievement
        {
            id = "hunter",
            name = "Hunter",
            description = "Destroy 100 enemies",
            targetValue = 100,
            currentValue = 0,
            unlocked = false,
            rewardCredits = 1000,
            rewardXP = 500
        });

        achievements.Add(new Achievement
        {
            id = "veteran",
            name = "Veteran Pilot",
            description = "Destroy 1000 enemies",
            targetValue = 1000,
            currentValue = 0,
            unlocked = false,
            rewardCredits = 5000,
            rewardXP = 2500
        });

        // Credits earned
        achievements.Add(new Achievement
        {
            id = "entrepreneur",
            name = "Entrepreneur",
            description = "Earn 10,000 credits",
            targetValue = 10000,
            currentValue = 0,
            unlocked = false,
            rewardCredits = 2000,
            rewardXP = 1000
        });

        // Play time
        achievements.Add(new Achievement
        {
            id = "dedicated",
            name = "Dedicated Pilot",
            description = "Play for 10 hours",
            targetValue = 36000, // 10 hours in seconds
            currentValue = 0,
            unlocked = false,
            rewardCredits = 1500,
            rewardXP = 750
        });
    }

    #endregion

    #region Update Timers

    private void UpdateTimers()
    {
        // Check streak timeout
        if (currentKillStreak > 0 && Time.time - lastKillTime > streakTimeWindow)
        {
            ResetStreak();
        }

        // Check combo timeout
        if (currentCombo > 0 && Time.time - lastComboTime > comboTimeLimit)
        {
            ResetCombo();
        }

        // Update play time achievement
        UpdateAchievementProgress("dedicated", (int)totalPlayTime);
    }

    #endregion

    #region Kill Rewards

    public void RegisterKill(KillType killType)
    {
        int baseCredits = 0;
        int baseXP = 0;

        switch (killType)
        {
            case KillType.Standard:
                baseCredits = standardKillCredits;
                baseXP = standardKillXP;
                break;

            case KillType.Elite:
                baseCredits = eliteKillCredits;
                baseXP = eliteKillXP;
                break;

            case KillType.Boss:
                baseCredits = bossKillCredits;
                baseXP = bossKillXP;
                break;

            case KillType.Headshot:
                baseCredits = standardKillCredits * 2;
                baseXP = standardKillXP * 2;
                break;

            case KillType.Multikill:
                baseCredits = standardKillCredits * 3;
                baseXP = standardKillXP * 3;
                break;
        }

        // Apply multipliers
        float totalMultiplier = CalculateTotalMultiplier();
        int finalCredits = Mathf.RoundToInt(baseCredits * totalMultiplier);
        int finalXP = Mathf.RoundToInt(baseXP * totalMultiplier);

        // Award rewards
        AddCredits(finalCredits);
        AddXP(finalXP);

        // Update statistics
        totalKills++;
        UpdateAchievementProgress("first_kill", totalKills);
        UpdateAchievementProgress("hunter", totalKills);
        UpdateAchievementProgress("veteran", totalKills);

        // Update streak
        IncrementStreak();

        // Update combo
        IncrementCombo();

        lastKillTime = Time.time;

        Debug.Log($"Kill registered! +{finalCredits} credits, +{finalXP} XP (x{totalMultiplier:F2} multiplier)");
    }

    public void RegisterDeath()
    {
        totalDeaths++;
        ResetStreak();
        ResetCombo();

        Debug.Log("Player died. Streak and combo reset.");
    }

    #endregion

    #region Credits & XP

    public void AddCredits(int amount)
    {
        if (amount <= 0) return;

        totalCreditsEarned += amount;

        // Send to upgrade system
        if (upgradeSystem != null)
        {
            upgradeSystem.AddCredits(amount);
        }

        OnCreditsEarned?.Invoke(amount);

        // Show visual feedback
        ShowRewardPopup(RewardType.Credits, amount);
        PlaySound(creditsEarnedSound);

        // Update achievements
        UpdateAchievementProgress("entrepreneur", totalCreditsEarned);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        totalXPEarned += amount;

        // Send to upgrade system
        if (upgradeSystem != null)
        {
            upgradeSystem.AddXP(amount);
        }

        OnXPEarned?.Invoke(amount);

        // Show visual feedback
        ShowRewardPopup(RewardType.XP, amount);
        PlaySound(xpEarnedSound);
    }

    public void CompleteMission(int bonusMultiplier = 1)
    {
        totalMissionsCompleted++;

        int credits = missionCompleteCredits * bonusMultiplier;
        int xp = missionCompleteXP * bonusMultiplier;

        AddCredits(credits);
        AddXP(xp);

        Debug.Log($"Mission completed! Bonus: x{bonusMultiplier}");
    }

    #endregion

    #region Streak System

    private void IncrementStreak()
    {
        currentKillStreak++;

        if (currentKillStreak > maxKillStreak)
        {
            maxKillStreak = currentKillStreak;
        }

        OnStreakChanged?.Invoke(currentKillStreak);

        // Check for milestone achievements
        foreach (int milestone in streakMilestones)
        {
            if (currentKillStreak == milestone)
            {
                OnStreakMilestone?.Invoke(milestone);
                ShowStreakEffect(milestone);
                PlaySound(streakSound);

                // Bonus reward for milestone
                AddCredits(milestone * 10);
                AddXP(milestone * 5);

                Debug.Log($"STREAK MILESTONE: {milestone} kills!");
            }
        }

        // Update streak achievements
        UpdateAchievementProgress("killing_spree", currentKillStreak);
        UpdateAchievementProgress("rampage", currentKillStreak);
    }

    private void ResetStreak()
    {
        if (currentKillStreak > 0)
        {
            Debug.Log($"Streak ended at {currentKillStreak} kills");
        }

        currentKillStreak = 0;
        OnStreakChanged?.Invoke(currentKillStreak);
    }

    private float GetStreakMultiplier()
    {
        return 1f + (currentKillStreak * streakMultiplier);
    }

    #endregion

    #region Combo System

    private void IncrementCombo()
    {
        currentCombo++;
        lastComboTime = Time.time;

        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
        }

        // Calculate combo multiplier (1.0 + 0.1 per combo level)
        comboMultiplier = 1f + (currentCombo * 0.1f);

        OnComboChanged?.Invoke(currentCombo, (int)(comboMultiplier * 100));

        if (currentCombo > 1 && gameplayUI != null)
        {
            gameplayUI.ShowCombo(currentCombo);
        }
    }

    private void ResetCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log($"Combo ended at x{currentCombo}");
        }

        currentCombo = 0;
        comboMultiplier = 1f;
        OnComboChanged?.Invoke(currentCombo, 100);
    }

    #endregion

    #region Multipliers

    private float CalculateTotalMultiplier()
    {
        float total = 1f;

        total *= GetStreakMultiplier();
        total *= comboMultiplier;
        total *= difficultyMultiplier;
        total *= eventMultiplier;
        total *= vipMultiplier;

        return total;
    }

    public void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = Mathf.Max(0.5f, multiplier);
        Debug.Log($"Difficulty multiplier set to {difficultyMultiplier:F2}x");
    }

    public void SetEventMultiplier(float multiplier)
    {
        eventMultiplier = Mathf.Max(1f, multiplier);
        Debug.Log($"Event multiplier set to {eventMultiplier:F2}x");
    }

    #endregion

    #region Achievements

    public void UpdateAchievementProgress(string achievementId, int currentValue)
    {
        if (!achievementDict.ContainsKey(achievementId)) return;

        Achievement achievement = achievementDict[achievementId];

        if (achievement.unlocked) return;

        achievement.currentValue = currentValue;

        // Check if unlocked
        if (achievement.currentValue >= achievement.targetValue)
        {
            UnlockAchievement(achievement);
        }
    }

    private void UnlockAchievement(Achievement achievement)
    {
        if (achievement.unlocked) return;

        achievement.unlocked = true;

        // Grant rewards
        AddCredits(achievement.rewardCredits);
        AddXP(achievement.rewardXP);

        // Notify
        OnAchievementUnlocked?.Invoke(achievement);

        // Visual feedback
        ShowAchievementPanel(achievement);
        PlaySound(achievementSound);

        Debug.Log($"ACHIEVEMENT UNLOCKED: {achievement.name}!");
    }

    public List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>(achievements);
    }

    public List<Achievement> GetUnlockedAchievements()
    {
        return achievements.FindAll(a => a.unlocked);
    }

    public float GetAchievementProgress(string achievementId)
    {
        if (!achievementDict.ContainsKey(achievementId)) return 0f;

        Achievement achievement = achievementDict[achievementId];
        return (float)achievement.currentValue / achievement.targetValue;
    }

    #endregion

    #region Daily Bonus

    private void CheckDailyBonus()
    {
        if (!enableDailyBonus) return;

        DateTime now = DateTime.Now;

        // Check if it's a new day
        if (lastDailyBonusClaimed.Date < now.Date)
        {
            ClaimDailyBonus();
        }
    }

    public void ClaimDailyBonus()
    {
        AddCredits(dailyBonusCredits);
        AddXP(dailyBonusCredits / 2);

        lastDailyBonusClaimed = DateTime.Now;

        if (gameplayUI != null)
        {
            gameplayUI.ShowNotification("Daily Bonus Claimed!", $"+{dailyBonusCredits} Credits!");
        }

        Debug.Log($"Daily bonus claimed: {dailyBonusCredits} credits");
    }

    #endregion

    #region Visual Feedback

    private void ShowRewardPopup(RewardType type, int amount)
    {
        if (gameplayUI != null)
        {
            switch (type)
            {
                case RewardType.Credits:
                    gameplayUI.ShowRewardPopup($"+{amount} Credits", Color.yellow);
                    break;
                case RewardType.XP:
                    gameplayUI.ShowRewardPopup($"+{amount} XP", Color.cyan);
                    break;
            }
        }
    }

    private void ShowStreakEffect(int streak)
    {
        if (streakEffectPrefab != null)
        {
            GameObject effect = Instantiate(streakEffectPrefab, Vector3.zero, Quaternion.identity);
            Destroy(effect, 3f);
        }

        if (gameplayUI != null)
        {
            gameplayUI.ShowStreakNotification(streak);
        }
    }

    private void ShowAchievementPanel(Achievement achievement)
    {
        if (achievementPanelPrefab != null)
        {
            GameObject panel = Instantiate(achievementPanelPrefab);
            Destroy(panel, 5f);
        }

        if (gameplayUI != null)
        {
            gameplayUI.ShowAchievement(achievement.name, achievement.description);
        }
    }

    #endregion

    #region Audio

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Data Persistence

    [Serializable]
    private class SaveData
    {
        public int totalKills;
        public int totalDeaths;
        public int totalCreditsEarned;
        public int totalXPEarned;
        public int totalMissionsCompleted;
        public float totalPlayTime;
        public int maxKillStreak;
        public int maxCombo;
        public string lastDailyBonus;
        public List<AchievementSaveData> achievements = new List<AchievementSaveData>();
    }

    [Serializable]
    private class AchievementSaveData
    {
        public string id;
        public int currentValue;
        public bool unlocked;
    }

    private void SavePlayerStats()
    {
        SaveData data = new SaveData
        {
            totalKills = totalKills,
            totalDeaths = totalDeaths,
            totalCreditsEarned = totalCreditsEarned,
            totalXPEarned = totalXPEarned,
            totalMissionsCompleted = totalMissionsCompleted,
            totalPlayTime = totalPlayTime,
            maxKillStreak = maxKillStreak,
            maxCombo = maxCombo,
            lastDailyBonus = lastDailyBonusClaimed.ToString("o")
        };

        foreach (var achievement in achievements)
        {
            data.achievements.Add(new AchievementSaveData
            {
                id = achievement.id,
                currentValue = achievement.currentValue,
                unlocked = achievement.unlocked
            });
        }

        string json = JsonUtility.ToJson(data, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, "PlayerStats.json");
        System.IO.File.WriteAllText(path, json);

        Debug.Log($"Player stats saved to {path}");
    }

    private void LoadPlayerStats()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "PlayerStats.json");

        if (!System.IO.File.Exists(path))
        {
            Debug.Log("No save file found, using defaults");
            return;
        }

        try
        {
            string json = System.IO.File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            totalKills = data.totalKills;
            totalDeaths = data.totalDeaths;
            totalCreditsEarned = data.totalCreditsEarned;
            totalXPEarned = data.totalXPEarned;
            totalMissionsCompleted = data.totalMissionsCompleted;
            totalPlayTime = data.totalPlayTime;
            maxKillStreak = data.maxKillStreak;
            maxCombo = data.maxCombo;

            if (!string.IsNullOrEmpty(data.lastDailyBonus))
            {
                lastDailyBonusClaimed = DateTime.Parse(data.lastDailyBonus);
            }

            foreach (var savedAchievement in data.achievements)
            {
                if (achievementDict.ContainsKey(savedAchievement.id))
                {
                    achievementDict[savedAchievement.id].currentValue = savedAchievement.currentValue;
                    achievementDict[savedAchievement.id].unlocked = savedAchievement.unlocked;
                }
            }

            Debug.Log($"Player stats loaded from {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load player stats: {e.Message}");
        }
    }

    public void ResetAllStats()
    {
        totalKills = 0;
        totalDeaths = 0;
        totalCreditsEarned = 0;
        totalXPEarned = 0;
        totalMissionsCompleted = 0;
        totalPlayTime = 0f;
        maxKillStreak = 0;
        maxCombo = 0;
        currentKillStreak = 0;
        currentCombo = 0;

        foreach (var achievement in achievements)
        {
            achievement.currentValue = 0;
            achievement.unlocked = false;
        }

        SavePlayerStats();
    }

    #endregion

    #region Public Getters

    public int GetTotalKills() => totalKills;
    public int GetTotalDeaths() => totalDeaths;
    public float GetKDRatio() => totalDeaths > 0 ? (float)totalKills / totalDeaths : totalKills;
    public int GetCurrentStreak() => currentKillStreak;
    public int GetMaxStreak() => maxKillStreak;
    public int GetCurrentCombo() => currentCombo;
    public int GetMaxCombo() => maxCombo;
    public float GetTotalPlayTime() => totalPlayTime;
    public int GetTotalCreditsEarned() => totalCreditsEarned;
    public int GetTotalXPEarned() => totalXPEarned;

    #endregion
}

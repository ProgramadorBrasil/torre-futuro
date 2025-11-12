using UnityEngine;
using System.Collections.Generic;
using System;

namespace SpaceRPG.Systems
{
    /// <summary>
    /// Comprehensive Upgrade System with tech tree, progression tracking, and persistent upgrades
    /// Supports ship upgrades (speed, health, armor) and weapon upgrades (damage, fire rate, ammo)
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
{
    #region Upgrade Categories
    public enum UpgradeCategory
    {
        Ship,
        Weapon,
        Utility
    }

    public enum UpgradeType
    {
        // Ship upgrades
        Speed,
        Health,
        Armor,
        Energy,

        // Weapon upgrades
        LaserDamage,
        LaserFireRate,
        LaserAmmo,
        MissileDamage,
        MissileFireRate,
        MissileAmmo,
        PlasmaDamage,
        PlasmaFireRate,
        PlasmaAmmo,

        // Utility upgrades
        FuelCapacity,
        EnergyRegen,
        ShieldRecharge,

        // Weapon unlocks
        UnlockMissile,
        UnlockPlasma
    }
    #endregion

    #region Upgrade Data Structure
    [Serializable]
    public class UpgradeData
    {
        public UpgradeType type;
        public string upgradeName;
        public string description;
        public UpgradeCategory category;
        public int currentLevel;
        public int maxLevel;
        public int baseCost;
        public float costMultiplier; // Cost increase per level
        public float effectPerLevel; // Effect increase per level
        public bool isUnlocked;
        public List<UpgradeType> prerequisites; // Required upgrades

        public int GetCostForNextLevel()
        {
            if (currentLevel >= maxLevel) return 0;
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
        }

        public float GetCurrentEffect()
        {
            return 1f + (effectPerLevel * currentLevel);
        }
    }
    #endregion

    [Header("Upgrade Configuration")]
    [SerializeField] private List<UpgradeData> allUpgrades = new List<UpgradeData>();

    [Header("Player Resources")]
    [SerializeField] private int playerCredits = 0;
    [SerializeField] private int playerXP = 0;
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private int xpForNextLevel = 100;

    [Header("References")]
    [SerializeField] private SpaceshipController spaceshipController;
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private RewardSystem rewardSystem;

    [Header("Persistence")]
    [SerializeField] private bool autoSave = true;
    [SerializeField] private string saveFileName = "UpgradeData.json";

    // Events
    public delegate void UpgradeEvent(UpgradeType type, int level);
    public delegate void ResourceEvent(int amount);
    public event UpgradeEvent OnUpgradePurchased;
    public event UpgradeEvent OnUpgradeMaxed;
    public event ResourceEvent OnCreditsChanged;
    public event ResourceEvent OnXPChanged;
    public event Action<int> OnLevelUp;

    private Dictionary<UpgradeType, UpgradeData> upgradeDict = new Dictionary<UpgradeType, UpgradeData>();

    #region Initialization

    private void Awake()
    {
        InitializeUpgrades();
        LoadUpgradeData();

        // Find references if not set
        if (spaceshipController == null)
            spaceshipController = FindObjectOfType<SpaceshipController>();
        if (weaponSystem == null)
            weaponSystem = FindObjectOfType<WeaponSystem>();
        if (rewardSystem == null)
            rewardSystem = FindObjectOfType<RewardSystem>();
    }

    private void Start()
    {
        // Subscribe to reward events
        if (rewardSystem != null)
        {
            rewardSystem.OnCreditsEarned += AddCredits;
            rewardSystem.OnXPEarned += AddXP;
        }

        ApplyAllUpgrades();
    }

    private void OnDestroy()
    {
        if (autoSave)
        {
            SaveUpgradeData();
        }

        if (rewardSystem != null)
        {
            rewardSystem.OnCreditsEarned -= AddCredits;
            rewardSystem.OnXPEarned -= AddXP;
        }
    }

    private void InitializeUpgrades()
    {
        if (allUpgrades.Count == 0)
        {
            CreateDefaultUpgrades();
        }

        // Build dictionary for fast lookup
        upgradeDict.Clear();
        foreach (var upgrade in allUpgrades)
        {
            if (!upgradeDict.ContainsKey(upgrade.type))
            {
                upgradeDict.Add(upgrade.type, upgrade);
            }
        }
    }

    private void CreateDefaultUpgrades()
    {
        // Ship Speed Upgrade
        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.Speed,
            upgradeName = "Engine Boost",
            description = "Increase maximum speed by 10% per level",
            category = UpgradeCategory.Ship,
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 100,
            costMultiplier = 1.5f,
            effectPerLevel = 0.1f,
            isUnlocked = true,
            prerequisites = new List<UpgradeType>()
        });

        // Ship Health Upgrade
        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.Health,
            upgradeName = "Hull Reinforcement",
            description = "Increase maximum health by 15% per level",
            category = UpgradeCategory.Ship,
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 150,
            costMultiplier = 1.6f,
            effectPerLevel = 0.15f,
            isUnlocked = true,
            prerequisites = new List<UpgradeType>()
        });

        // Ship Armor Upgrade
        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.Armor,
            upgradeName = "Reactive Armor",
            description = "Increase armor capacity by 20% per level",
            category = UpgradeCategory.Ship,
            currentLevel = 0,
            maxLevel = 8,
            baseCost = 200,
            costMultiplier = 1.7f,
            effectPerLevel = 0.2f,
            isUnlocked = true,
            prerequisites = new List<UpgradeType>()
        });

        // Energy Capacity Upgrade
        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.Energy,
            upgradeName = "Fusion Reactor",
            description = "Increase energy capacity by 15% per level",
            category = UpgradeCategory.Ship,
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 120,
            costMultiplier = 1.5f,
            effectPerLevel = 0.15f,
            isUnlocked = true,
            prerequisites = new List<UpgradeType>()
        });

        // Laser Weapon Upgrades
        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.LaserDamage,
            upgradeName = "Laser Amplifier",
            description = "Increase laser damage by 15% per level",
            category = UpgradeCategory.Weapon,
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 100,
            costMultiplier = 1.5f,
            effectPerLevel = 0.15f,
            isUnlocked = true,
            prerequisites = new List<UpgradeType>()
        });

        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.LaserFireRate,
            upgradeName = "Rapid Fire System",
            description = "Increase laser fire rate by 10% per level",
            category = UpgradeCategory.Weapon,
            currentLevel = 0,
            maxLevel = 8,
            baseCost = 120,
            costMultiplier = 1.6f,
            effectPerLevel = 0.1f,
            isUnlocked = true,
            prerequisites = new List<UpgradeType>()
        });

        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.LaserAmmo,
            upgradeName = "Extended Magazine",
            description = "Increase laser ammo capacity by 20% per level",
            category = UpgradeCategory.Weapon,
            currentLevel = 0,
            maxLevel = 5,
            baseCost = 80,
            costMultiplier = 1.4f,
            effectPerLevel = 0.2f,
            isUnlocked = true,
            prerequisites = new List<UpgradeType>()
        });

        // Missile Unlock
        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.UnlockMissile,
            upgradeName = "Missile System",
            description = "Unlock guided missile weapons",
            category = UpgradeCategory.Weapon,
            currentLevel = 0,
            maxLevel = 1,
            baseCost = 500,
            costMultiplier = 1f,
            effectPerLevel = 1f,
            isUnlocked = true,
            prerequisites = new List<UpgradeType> { UpgradeType.LaserDamage }
        });

        // Missile Weapon Upgrades
        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.MissileDamage,
            upgradeName = "Warhead Enhancement",
            description = "Increase missile damage by 20% per level",
            category = UpgradeCategory.Weapon,
            currentLevel = 0,
            maxLevel = 8,
            baseCost = 200,
            costMultiplier = 1.7f,
            effectPerLevel = 0.2f,
            isUnlocked = false,
            prerequisites = new List<UpgradeType> { UpgradeType.UnlockMissile }
        });

        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.MissileAmmo,
            upgradeName = "Missile Storage",
            description = "Increase missile ammo capacity by 25% per level",
            category = UpgradeCategory.Weapon,
            currentLevel = 0,
            maxLevel = 5,
            baseCost = 150,
            costMultiplier = 1.6f,
            effectPerLevel = 0.25f,
            isUnlocked = false,
            prerequisites = new List<UpgradeType> { UpgradeType.UnlockMissile }
        });

        // Plasma Unlock
        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.UnlockPlasma,
            upgradeName = "Plasma Cannon",
            description = "Unlock plasma weapon with splash damage",
            category = UpgradeCategory.Weapon,
            currentLevel = 0,
            maxLevel = 1,
            baseCost = 800,
            costMultiplier = 1f,
            effectPerLevel = 1f,
            isUnlocked = true,
            prerequisites = new List<UpgradeType> { UpgradeType.UnlockMissile, UpgradeType.Energy }
        });

        // Plasma Weapon Upgrades
        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.PlasmaDamage,
            upgradeName = "Plasma Intensifier",
            description = "Increase plasma damage by 18% per level",
            category = UpgradeCategory.Weapon,
            currentLevel = 0,
            maxLevel = 8,
            baseCost = 180,
            costMultiplier = 1.65f,
            effectPerLevel = 0.18f,
            isUnlocked = false,
            prerequisites = new List<UpgradeType> { UpgradeType.UnlockPlasma }
        });

        allUpgrades.Add(new UpgradeData
        {
            type = UpgradeType.PlasmaFireRate,
            upgradeName = "Plasma Cooling",
            description = "Increase plasma fire rate by 12% per level",
            category = UpgradeCategory.Weapon,
            currentLevel = 0,
            maxLevel = 6,
            baseCost = 200,
            costMultiplier = 1.7f,
            effectPerLevel = 0.12f,
            isUnlocked = false,
            prerequisites = new List<UpgradeType> { UpgradeType.UnlockPlasma }
        });
    }

    #endregion

    #region Upgrade Purchase

    public bool CanPurchaseUpgrade(UpgradeType type)
    {
        if (!upgradeDict.ContainsKey(type)) return false;

        UpgradeData upgrade = upgradeDict[type];

        // Check if maxed
        if (upgrade.currentLevel >= upgrade.maxLevel) return false;

        // Check if unlocked
        if (!upgrade.isUnlocked) return false;

        // Check prerequisites
        if (!CheckPrerequisites(upgrade)) return false;

        // Check cost
        int cost = upgrade.GetCostForNextLevel();
        if (playerCredits < cost) return false;

        return true;
    }

    public bool PurchaseUpgrade(UpgradeType type)
    {
        if (!CanPurchaseUpgrade(type)) return false;

        UpgradeData upgrade = upgradeDict[type];
        int cost = upgrade.GetCostForNextLevel();

        // Deduct credits
        playerCredits -= cost;
        OnCreditsChanged?.Invoke(playerCredits);

        // Increase level
        upgrade.currentLevel++;

        // Check if maxed
        if (upgrade.currentLevel >= upgrade.maxLevel)
        {
            OnUpgradeMaxed?.Invoke(type, upgrade.currentLevel);
        }

        // Apply upgrade effect
        ApplyUpgrade(upgrade);

        // Unlock dependent upgrades
        UnlockDependentUpgrades(type);

        OnUpgradePurchased?.Invoke(type, upgrade.currentLevel);

        if (autoSave)
        {
            SaveUpgradeData();
        }

        return true;
    }

    private bool CheckPrerequisites(UpgradeData upgrade)
    {
        if (upgrade.prerequisites == null || upgrade.prerequisites.Count == 0)
            return true;

        foreach (var prereq in upgrade.prerequisites)
        {
            if (!upgradeDict.ContainsKey(prereq)) return false;

            UpgradeData prereqUpgrade = upgradeDict[prereq];
            if (prereqUpgrade.currentLevel <= 0) return false;
        }

        return true;
    }

    private void UnlockDependentUpgrades(UpgradeType justPurchased)
    {
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade.prerequisites != null && upgrade.prerequisites.Contains(justPurchased))
            {
                upgrade.isUnlocked = true;
            }
        }

        // Special unlocks
        if (justPurchased == UpgradeType.UnlockMissile && weaponSystem != null)
        {
            weaponSystem.UnlockWeapon(WeaponSystem.WeaponType.Missile);
        }

        if (justPurchased == UpgradeType.UnlockPlasma && weaponSystem != null)
        {
            weaponSystem.UnlockWeapon(WeaponSystem.WeaponType.Plasma);
        }
    }

    #endregion

    #region Apply Upgrades

    private void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.Speed:
            case UpgradeType.Health:
            case UpgradeType.Armor:
            case UpgradeType.Energy:
                ApplyShipUpgrades();
                break;

            case UpgradeType.LaserDamage:
            case UpgradeType.LaserFireRate:
            case UpgradeType.LaserAmmo:
                ApplyWeaponUpgrades(WeaponSystem.WeaponType.Laser);
                break;

            case UpgradeType.MissileDamage:
            case UpgradeType.MissileFireRate:
            case UpgradeType.MissileAmmo:
                ApplyWeaponUpgrades(WeaponSystem.WeaponType.Missile);
                break;

            case UpgradeType.PlasmaDamage:
            case UpgradeType.PlasmaFireRate:
            case UpgradeType.PlasmaAmmo:
                ApplyWeaponUpgrades(WeaponSystem.WeaponType.Plasma);
                break;
        }
    }

    private void ApplyAllUpgrades()
    {
        ApplyShipUpgrades();
        ApplyWeaponUpgrades(WeaponSystem.WeaponType.Laser);
        ApplyWeaponUpgrades(WeaponSystem.WeaponType.Missile);
        ApplyWeaponUpgrades(WeaponSystem.WeaponType.Plasma);
    }

    private void ApplyShipUpgrades()
    {
        if (spaceshipController == null) return;

        float speedMult = GetUpgradeEffect(UpgradeType.Speed);
        float healthMult = GetUpgradeEffect(UpgradeType.Health);
        float armorMult = GetUpgradeEffect(UpgradeType.Armor);
        float energyMult = GetUpgradeEffect(UpgradeType.Energy);

        spaceshipController.ApplyUpgrades(speedMult, healthMult, armorMult, energyMult);
    }

    private void ApplyWeaponUpgrades(WeaponSystem.WeaponType weaponType)
    {
        if (weaponSystem == null) return;

        float damageMult = 1f;
        float fireRateMult = 1f;
        float ammoMult = 1f;

        switch (weaponType)
        {
            case WeaponSystem.WeaponType.Laser:
                damageMult = GetUpgradeEffect(UpgradeType.LaserDamage);
                fireRateMult = GetUpgradeEffect(UpgradeType.LaserFireRate);
                ammoMult = GetUpgradeEffect(UpgradeType.LaserAmmo);
                break;

            case WeaponSystem.WeaponType.Missile:
                damageMult = GetUpgradeEffect(UpgradeType.MissileDamage);
                fireRateMult = GetUpgradeEffect(UpgradeType.MissileFireRate);
                ammoMult = GetUpgradeEffect(UpgradeType.MissileAmmo);
                break;

            case WeaponSystem.WeaponType.Plasma:
                damageMult = GetUpgradeEffect(UpgradeType.PlasmaDamage);
                fireRateMult = GetUpgradeEffect(UpgradeType.PlasmaFireRate);
                ammoMult = GetUpgradeEffect(UpgradeType.PlasmaAmmo);
                break;
        }

        weaponSystem.UpgradeWeapon(weaponType, damageMult, fireRateMult, ammoMult);
    }

    private float GetUpgradeEffect(UpgradeType type)
    {
        if (!upgradeDict.ContainsKey(type)) return 1f;
        return upgradeDict[type].GetCurrentEffect();
    }

    #endregion

    #region Resource Management

    public void AddCredits(int amount)
    {
        playerCredits += amount;
        OnCreditsChanged?.Invoke(playerCredits);

        if (autoSave)
        {
            SaveUpgradeData();
        }
    }

    public void AddXP(int amount)
    {
        playerXP += amount;
        OnXPChanged?.Invoke(playerXP);

        // Check for level up
        while (playerXP >= xpForNextLevel)
        {
            LevelUp();
        }

        if (autoSave)
        {
            SaveUpgradeData();
        }
    }

    private void LevelUp()
    {
        playerLevel++;
        playerXP -= xpForNextLevel;
        xpForNextLevel = Mathf.RoundToInt(xpForNextLevel * 1.5f);

        // Give bonus credits on level up
        AddCredits(100 * playerLevel);

        OnLevelUp?.Invoke(playerLevel);
    }

    public int GetCredits() => playerCredits;
    public int GetXP() => playerXP;
    public int GetLevel() => playerLevel;
    public int GetXPForNextLevel() => xpForNextLevel;

    #endregion

    #region Data Persistence

    [Serializable]
    private class SaveData
    {
        public int credits;
        public int xp;
        public int level;
        public int xpForNext;
        public List<UpgradeSaveData> upgrades = new List<UpgradeSaveData>();
    }

    [Serializable]
    private class UpgradeSaveData
    {
        public UpgradeType type;
        public int level;
        public bool unlocked;
    }

    private void SaveUpgradeData()
    {
        SaveData data = new SaveData
        {
            credits = playerCredits,
            xp = playerXP,
            level = playerLevel,
            xpForNext = xpForNextLevel
        };

        foreach (var upgrade in allUpgrades)
        {
            data.upgrades.Add(new UpgradeSaveData
            {
                type = upgrade.type,
                level = upgrade.currentLevel,
                unlocked = upgrade.isUnlocked
            });
        }

        string json = JsonUtility.ToJson(data, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
        System.IO.File.WriteAllText(path, json);

        Debug.Log($"Upgrade data saved to {path}");
    }

    private void LoadUpgradeData()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);

        if (!System.IO.File.Exists(path))
        {
            Debug.Log("No save file found, using defaults");
            return;
        }

        try
        {
            string json = System.IO.File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            playerCredits = data.credits;
            playerXP = data.xp;
            playerLevel = data.level;
            xpForNextLevel = data.xpForNext;

            foreach (var savedUpgrade in data.upgrades)
            {
                if (upgradeDict.ContainsKey(savedUpgrade.type))
                {
                    upgradeDict[savedUpgrade.type].currentLevel = savedUpgrade.level;
                    upgradeDict[savedUpgrade.type].isUnlocked = savedUpgrade.unlocked;
                }
            }

            Debug.Log($"Upgrade data loaded from {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load upgrade data: {e.Message}");
        }
    }

    public void ResetAllUpgrades()
    {
        foreach (var upgrade in allUpgrades)
        {
            upgrade.currentLevel = 0;
            upgrade.isUnlocked = upgrade.prerequisites == null || upgrade.prerequisites.Count == 0;
        }

        playerCredits = 0;
        playerXP = 0;
        playerLevel = 1;
        xpForNextLevel = 100;

        ApplyAllUpgrades();

        if (autoSave)
        {
            SaveUpgradeData();
        }
    }

    #endregion

    #region Public Getters

    public UpgradeData GetUpgradeData(UpgradeType type)
    {
        return upgradeDict.ContainsKey(type) ? upgradeDict[type] : null;
    }

    public List<UpgradeData> GetAllUpgrades()
    {
        return new List<UpgradeData>(allUpgrades);
    }

    public List<UpgradeData> GetUpgradesByCategory(UpgradeCategory category)
    {
        return allUpgrades.FindAll(u => u.category == category);
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
        return upgradeDict.ContainsKey(type) ? upgradeDict[type].currentLevel : 0;
    }

    #endregion
}
}

# 📚 API REFERENCE - SPACESHIP TOWER FUTURO

## Referência Rápida de Classes e Métodos

---

## 🚀 SpaceshipController.cs

### Propriedades Públicas
```csharp
public float speedUpgradeMultiplier = 1f;
public float healthUpgradeMultiplier = 1f;
public float armorUpgradeMultiplier = 1f;
public float energyUpgradeMultiplier = 1f;
```

### Eventos
```csharp
public event SpaceshipEvent OnHealthChanged;      // (float percentage)
public event SpaceshipEvent OnEnergyChanged;      // (float percentage)
public event SpaceshipEvent OnFuelChanged;        // (float percentage)
public event SpaceshipEvent OnArmorChanged;       // (float percentage)
public event Action OnSpaceshipDestroyed;
```

### Métodos Públicos
```csharp
// Recursos
void ConsumeEnergy(float amount)
void RestoreFuel(float amount)
void RestoreEnergy(float amount)
void Heal(float amount)
void RepairArmor(float amount)

// Dano
void TakeDamage(float damage)

// Getters
float GetHealthPercentage()
float GetEnergyPercentage()
float GetFuelPercentage()
float GetArmorPercentage()
float GetCurrentSpeed()
bool IsAlive()
Vector3 GetVelocity()
bool HasEnergy(float amount)
bool HasFuel(float amount)

// Upgrades
void ApplyUpgrades(float speedMult, float healthMult, float armorMult, float energyMult)
```

### Uso Exemplo
```csharp
SpaceshipController ship = GetComponent<SpaceshipController>();
ship.TakeDamage(25f);
ship.Heal(50f);
ship.ConsumeEnergy(10f);

if (ship.HasEnergy(20f))
{
    // Ação que requer energia
}
```

---

## 🔫 WeaponSystem.cs

### Enums
```csharp
public enum WeaponType { Laser, Missile, Plasma }
```

### Classes Serializáveis
```csharp
[Serializable]
public class WeaponConfig
{
    public WeaponType type;
    public string weaponName;
    public GameObject projectilePrefab;
    public float damage;
    public float projectileSpeed;
    public float fireRate;
    public int maxAmmo;
    public int currentAmmo;
    public float reloadTime;
    public float energyCost;
    // ... visual effects
}
```

### Eventos
```csharp
public event WeaponEvent OnAmmoChanged;        // (WeaponType, currentAmmo, maxAmmo)
public event WeaponEvent OnWeaponSwitched;     // (WeaponType, currentAmmo, maxAmmo)
public event Action OnReloadStarted;
public event Action OnReloadCompleted;
public event Action<float> OnHeatChanged;      // (heatPercentage)
```

### Métodos Públicos
```csharp
// Disparo
void TryFire()

// Gerenciamento de armas
void SwitchWeapon(int index)
void SwitchToNextWeapon()
void SwitchToPreviousWeapon()
void UnlockWeapon(WeaponType type)

// Reload
void StartReload()

// Upgrades
void UpgradeWeapon(WeaponType type, float damageMult, float fireRateMult, float ammoMult)

// Getters
int GetCurrentAmmo()
int GetMaxAmmo()
float GetHeatPercentage()
bool IsReloading()
bool IsOverheated()
WeaponType GetCurrentWeaponType()
```

### Uso Exemplo
```csharp
WeaponSystem weapons = GetComponent<WeaponSystem>();

// Trocar arma
weapons.SwitchWeapon(1); // Míssil

// Disparar
weapons.TryFire();

// Upgrade
weapons.UpgradeWeapon(WeaponType.Laser, 1.5f, 1.2f, 1.3f);

// Desbloquear
weapons.UnlockWeapon(WeaponType.Plasma);
```

---

## 🔧 UpgradeSystem.cs

### Enums
```csharp
public enum UpgradeCategory { Ship, Weapon, Utility }

public enum UpgradeType
{
    // Ship
    Speed, Health, Armor, Energy,

    // Weapons
    LaserDamage, LaserFireRate, LaserAmmo,
    MissileDamage, MissileFireRate, MissileAmmo,
    PlasmaDamage, PlasmaFireRate, PlasmaAmmo,

    // Unlocks
    UnlockMissile, UnlockPlasma
}
```

### Classes Serializáveis
```csharp
[Serializable]
public class UpgradeData
{
    public UpgradeType type;
    public string upgradeName;
    public string description;
    public int currentLevel;
    public int maxLevel;
    public int baseCost;
    public float costMultiplier;
    public float effectPerLevel;
    public bool isUnlocked;
    public List<UpgradeType> prerequisites;

    public int GetCostForNextLevel()
    public float GetCurrentEffect()
}
```

### Eventos
```csharp
public event UpgradeEvent OnUpgradePurchased;    // (UpgradeType, level)
public event UpgradeEvent OnUpgradeMaxed;        // (UpgradeType, level)
public event ResourceEvent OnCreditsChanged;     // (amount)
public event ResourceEvent OnXPChanged;          // (amount)
public event Action<int> OnLevelUp;              // (level)
```

### Métodos Públicos
```csharp
// Compra
bool CanPurchaseUpgrade(UpgradeType type)
bool PurchaseUpgrade(UpgradeType type)

// Recursos
void AddCredits(int amount)
void AddXP(int amount)

// Getters
UpgradeData GetUpgradeData(UpgradeType type)
List<UpgradeData> GetAllUpgrades()
List<UpgradeData> GetUpgradesByCategory(UpgradeCategory category)
int GetUpgradeLevel(UpgradeType type)
int GetCredits()
int GetXP()
int GetLevel()
int GetXPForNextLevel()

// Persistência
void ResetAllUpgrades()
```

### Uso Exemplo
```csharp
UpgradeSystem upgrades = FindObjectOfType<UpgradeSystem>();

// Verificar e comprar
if (upgrades.CanPurchaseUpgrade(UpgradeType.Speed))
{
    upgrades.PurchaseUpgrade(UpgradeType.Speed);
}

// Adicionar recursos
upgrades.AddCredits(100);
upgrades.AddXP(50);

// Obter dados
int currentLevel = upgrades.GetLevel();
int credits = upgrades.GetCredits();
```

---

## 🌱 PlantingSystem.cs

### Enums
```csharp
public enum PlantType
{
    EnergyFlower,  // Restaura energia
    CreditFruit,   // Dá créditos
    HealingHerb,   // Restaura vida
    RareCrystal    // Restaura tudo
}
```

### Classes Serializáveis
```csharp
[Serializable]
public class PlantData
{
    public PlantType type;
    public string plantName;
    public string description;
    public GameObject plantPrefab;
    public int seedCost;
    public float growthTime;
    public int harvestValue;
    public int harvestXP;
    public Color plantColor;
    public bool unlocked;
}
```

### Eventos
```csharp
public event PlantEvent OnPlantPlanted;         // (PlantType)
public event PlantEvent OnPlantHarvested;       // (PlantType)
public event Action<int> OnPlantCountChanged;   // (count)
```

### Métodos Públicos
```csharp
// Modo plantio
void TogglePlantingMode()
void TryPlantSeed()

// Colheita
void HarvestNearbyPlants()

// Seleção de tipo
void SelectNextPlantType()
void SelectPreviousPlantType()
void UnlockPlantType(PlantType type)

// Getters
int GetActivePlantCount()
int GetMaxPlants()
bool IsPlantingMode()
PlantType GetSelectedPlantType()
List<PlantData> GetAllPlantTypes()
```

### Uso Exemplo
```csharp
PlantingSystem planting = FindObjectOfType<PlantingSystem>();

// Ativar modo
planting.TogglePlantingMode();

// Trocar tipo
planting.SelectNextPlantType();

// Plantar (chamado automaticamente ao clicar)
planting.TryPlantSeed();

// Colher plantas próximas
planting.HarvestNearbyPlants();

// Desbloquear tipo raro
planting.UnlockPlantType(PlantType.RareCrystal);
```

---

## 👤 NPCInstructor.cs

### Enums
```csharp
public enum TutorialStage
{
    Welcome,
    Movement,
    Weapons,
    Combat,
    Planting,
    Upgrades,
    Advanced,
    Completed
}
```

### Classes Serializáveis
```csharp
[Serializable]
public class DialogueEntry
{
    public string id;
    public string speakerName;
    public string text;
    public AudioClip voiceClip;
    public float displayDuration;
    public bool requiresResponse;
    public List<string> responses;
}
```

### Eventos
```csharp
public event DialogueEvent OnDialogueStarted;           // (dialogueId)
public event DialogueEvent OnDialogueEnded;             // (dialogueId)
public event TutorialEvent OnTutorialStageCompleted;    // (stage)
public event Action OnQuestCompleted;
```

### Métodos Públicos
```csharp
// Interação
void Interact()

// Diálogo
void StartDialogue(string dialogueId)
void QueueDialogue(string dialogueId)

// Tutorial
void AdvanceTutorial()

// Quests
void OfferQuest(string description, int creditsReward, int xpReward)
void CompleteQuest()

// Getters
bool IsDialogueActive()
bool IsTutorialCompleted()
TutorialStage GetCurrentTutorialStage()
bool HasActiveQuest()
string GetQuestDescription()
```

### Uso Exemplo
```csharp
NPCInstructor instructor = FindObjectOfType<NPCInstructor>();

// Iniciar diálogo específico
instructor.StartDialogue("weapons_tutorial");

// Avançar tutorial
instructor.AdvanceTutorial();

// Oferecer quest
instructor.OfferQuest("Destroy 5 enemies", 200, 100);

// Completar quest
instructor.CompleteQuest();
```

---

## 🏆 RewardSystem.cs

### Enums
```csharp
public enum RewardType { Credits, XP, Achievement, Unlock, Bonus }
public enum KillType { Standard, Elite, Boss, Headshot, Multikill }
```

### Classes Serializáveis
```csharp
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
}
```

### Eventos
```csharp
public event RewardEvent OnCreditsEarned;          // (amount)
public event RewardEvent OnXPEarned;               // (amount)
public event StreakEvent OnStreakChanged;          // (streak)
public event StreakEvent OnStreakMilestone;        // (streak)
public event AchievementEvent OnAchievementUnlocked;
public event Action<int, int> OnComboChanged;      // (combo, multiplier)
```

### Métodos Públicos
```csharp
// Kills e Deaths
void RegisterKill(KillType killType)
void RegisterDeath()

// Recursos
void AddCredits(int amount)
void AddXP(int amount)
void CompleteMission(int bonusMultiplier = 1)

// Achievements
void UpdateAchievementProgress(string achievementId, int currentValue)
List<Achievement> GetAllAchievements()
List<Achievement> GetUnlockedAchievements()
float GetAchievementProgress(string achievementId)

// Daily Bonus
void ClaimDailyBonus()

// Multipliers
void SetDifficultyMultiplier(float multiplier)
void SetEventMultiplier(float multiplier)

// Getters
int GetTotalKills()
int GetTotalDeaths()
float GetKDRatio()
int GetCurrentStreak()
int GetMaxStreak()
int GetCurrentCombo()
int GetMaxCombo()
float GetTotalPlayTime()
int GetTotalCreditsEarned()
int GetTotalXPEarned()

// Persistência
void ResetAllStats()
```

### Uso Exemplo
```csharp
RewardSystem rewards = FindObjectOfType<RewardSystem>();

// Registrar kill
rewards.RegisterKill(RewardSystem.KillType.Standard);

// Adicionar recursos manualmente
rewards.AddCredits(100);
rewards.AddXP(50);

// Atualizar progresso de achievement
rewards.UpdateAchievementProgress("hunter", totalKills);

// Completar missão com bônus
rewards.CompleteMission(2); // 2x bonus

// Obter estatísticas
float kdr = rewards.GetKDRatio();
int streak = rewards.GetCurrentStreak();
```

---

## 🖥️ GameplayUI.cs

### Métodos Públicos
```csharp
// Diálogo
void ShowDialogue(string speaker, string text)
void HideDialogue()

// Notificações
void ShowNotification(string title, string body)
void ShowRewardPopup(string text, Color color)

// Combo e Streak
void ShowCombo(int combo)
void ShowStreakNotification(int streak)

// Achievements
void ShowAchievement(string name, string description)

// Planting UI
void UpdatePlantingUI(string plantName, int cost)

// Helpers
void UpdateAllUI()
void SetCrosshairColor(Color color)
void ShowTargetReticle(Transform target, string targetName, float healthPercent)
void HideTargetReticle()
```

### Uso Exemplo
```csharp
GameplayUI ui = FindObjectOfType<GameplayUI>();

// Mostrar notificação
ui.ShowNotification("Mission Complete!", "Great job!");

// Mostrar diálogo
ui.ShowDialogue("Commander", "Well done, pilot!");

// Popup de reward
ui.ShowRewardPopup("+100 Credits", Color.yellow);

// Atualizar tudo
ui.UpdateAllUI();
```

---

## 🎮 GameManager.cs

### Enums
```csharp
public enum GameState
{
    MainMenu,
    Tutorial,
    Gameplay,
    Paused,
    Upgrading,
    GameOver,
    Victory
}
```

### Singleton
```csharp
public static GameManager Instance { get; private set; }
```

### Eventos
```csharp
public event GameStateEvent OnGameStateChanged;       // (newState)
public event WaveEvent OnWaveStarted;                 // (waveNumber)
public event WaveEvent OnWaveCompleted;               // (waveNumber)
public event MissionEvent OnMissionProgressUpdated;   // (objective, progress, target)
public event Action OnGameOver;
public event Action OnVictory;
```

### Métodos Públicos
```csharp
// Game State
void StartGame()
void SetGameState(GameState newState)
void RestartGame()

// Enemy Management
void RegisterEnemyKilled(GameObject enemy)

// Wave System
void StartWave(int waveNumber)

// Mission System
void StartMission(string objective, int targetValue)

// Checkpoint
void SetCheckpoint(Vector3 position, Quaternion rotation)
void RespawnAtCheckpoint()

// Getters
GameState GetCurrentState()
int GetCurrentWave()
int GetTotalEnemiesKilled()
int GetActiveEnemyCount()
bool IsMissionActive()
float GetDifficultyMultiplier()
```

### Uso Exemplo
```csharp
// Singleton access
GameManager.Instance.StartGame();

// Registrar kill de inimigo
GameManager.Instance.RegisterEnemyKilled(enemyGameObject);

// Salvar checkpoint
GameManager.Instance.SetCheckpoint(transform.position, transform.rotation);

// Iniciar missão
GameManager.Instance.StartMission("Destroy 10 enemies", 10);

// Verificar estado
if (GameManager.Instance.GetCurrentState() == GameManager.GameState.Gameplay)
{
    // Gameplay ativo
}
```

---

## 🔗 INTEGRAÇÃO ENTRE SISTEMAS

### Fluxo de Eventos Típico:

```csharp
// 1. Player mata inimigo
WeaponSystem → Projectile → Enemy.TakeDamage()

// 2. Enemy morre
Enemy → GameManager.RegisterEnemyKilled()

// 3. GameManager notifica RewardSystem
GameManager → RewardSystem.RegisterKill()

// 4. RewardSystem concede rewards
RewardSystem → UpgradeSystem.AddCredits()
RewardSystem → UpgradeSystem.AddXP()

// 5. UI atualiza
UpgradeSystem.OnCreditsChanged → GameplayUI.UpdateCreditsDisplay()
RewardSystem.OnStreakChanged → GameplayUI.UpdateStreakDisplay()

// 6. Player compra upgrade
GameplayUI → UpgradeSystem.PurchaseUpgrade()

// 7. Upgrade aplica na nave
UpgradeSystem → SpaceshipController.ApplyUpgrades()
UpgradeSystem → WeaponSystem.UpgradeWeapon()
```

### Setup Típico de Referências:

```csharp
// GameManager
public SpaceshipController playerShip;
public WeaponSystem weaponSystem;
public UpgradeSystem upgradeSystem;
public RewardSystem rewardSystem;
public PlantingSystem plantingSystem;
public NPCInstructor npcInstructor;
public GameplayUI gameplayUI;

// UpgradeSystem
public SpaceshipController spaceshipController;
public WeaponSystem weaponSystem;
public RewardSystem rewardSystem;

// PlantingSystem
public UpgradeSystem upgradeSystem;
public RewardSystem rewardSystem;
public Transform playerTransform;

// GameplayUI
// Encontra referências automaticamente via FindObjectOfType
```

---

## 📝 NOTAS IMPORTANTES

### Performance
- Use **object pooling** para projéteis (reusar ao invés de Instantiate)
- Evite **Update() pesado**, use events
- Cache **GetComponent** calls
- Use **Coroutines** para operações assíncronas

### Threading
- **Não** use threads para Unity objects
- Use **Coroutines** para delays
- **Jobs System** para cálculos pesados (opcional)

### Serialização
- Todos os campos serializados usam **[SerializeField]**
- Use **[HideInInspector]** para campos públicos não editáveis
- **JsonUtility** para save/load

### Eventos
- **Subscribe** em OnEnable/Start
- **Unsubscribe** em OnDisable/OnDestroy
- Sempre verifique **null** antes de invoke

---

## 🎯 EXEMPLOS DE USO COMPLETOS

### Exemplo 1: Criar Novo Tipo de Arma

```csharp
// No WeaponSystem.cs, adicionar em InitializeDefaultWeapons():
weaponConfigs.Add(new WeaponConfig
{
    type = WeaponType.NewWeapon,
    weaponName = "Super Cannon",
    damage = 75f,
    projectileSpeed = 80f,
    fireRate = 2f,
    maxAmmo = 50,
    currentAmmo = 50,
    reloadTime = 3.5f,
    energyCost = 12f,
    range = 180f,
    unlocked = false
});
```

### Exemplo 2: Criar Novo Upgrade

```csharp
// No UpgradeSystem.cs, adicionar em CreateDefaultUpgrades():
allUpgrades.Add(new UpgradeData
{
    type = UpgradeType.NewUpgrade,
    upgradeName = "Shield Generator",
    description = "Add energy shields to your ship",
    category = UpgradeCategory.Ship,
    currentLevel = 0,
    maxLevel = 5,
    baseCost = 300,
    costMultiplier = 1.8f,
    effectPerLevel = 0.25f,
    isUnlocked = true,
    prerequisites = new List<UpgradeType> { UpgradeType.Armor }
});
```

### Exemplo 3: Adicionar Novo Achievement

```csharp
// No RewardSystem.cs, adicionar em CreateDefaultAchievements():
achievements.Add(new Achievement
{
    id = "speed_demon",
    name = "Speed Demon",
    description = "Reach max speed 100 times",
    targetValue = 100,
    currentValue = 0,
    unlocked = false,
    rewardCredits = 500,
    rewardXP = 250
});

// No SpaceshipController.cs, em Update():
if (currentSpeed >= maxSpeed * speedUpgradeMultiplier)
{
    RewardSystem rewards = FindObjectOfType<RewardSystem>();
    rewards?.UpdateAchievementProgress("speed_demon", ++speedDemonCounter);
}
```

---

## 🔍 DEBUGGING TIPS

### Verificar Eventos
```csharp
void OnEnable()
{
    upgradeSystem.OnCreditsChanged += (amount) =>
    {
        Debug.Log($"Credits changed: {amount}");
    };
}
```

### Verificar Estado
```csharp
[ContextMenu("Debug State")]
void DebugState()
{
    Debug.Log($"Health: {GetHealthPercentage() * 100}%");
    Debug.Log($"Energy: {GetEnergyPercentage() * 100}%");
    Debug.Log($"Speed: {GetCurrentSpeed()}");
}
```

### Gizmos para Visualização
```csharp
void OnDrawGizmosSelected()
{
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(transform.position, interactionRange);
}
```

---

**FIM DA API REFERENCE** 📚

Para documentação completa, consulte:
- **GUIA_COMPLETO_INTEGRACAO.md** - Setup e integração
- **CHECKLIST_TESTES.md** - Testes de validação
- **README.md** - Visão geral do projeto

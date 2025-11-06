using UnityEngine;
using SpaceRPG.Systems;
using SpaceRPG.Data;
using SpaceRPG.UI;

namespace SpaceRPG.Core
{
    /// <summary>
    /// GameManager central que coordena todos os sistemas
    /// </summary>
    public class GameManagerRPG : MonoBehaviour
    {
        private static GameManagerRPG _instance;
        public static GameManagerRPG Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManagerRPG>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManagerRPG");
                        _instance = go.AddComponent<GameManagerRPG>();
                    }
                }
                return _instance;
            }
        }

        [Header("System References")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private ShopSystem shopSystem;
        [SerializeField] private QuestSystem questSystem;
        [SerializeField] private ShipSystem shipSystem;
        [SerializeField] private MaintenanceSystem maintenanceSystem;
        [SerializeField] private PlantCareSystemAdvanced plantCareSystem;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private SaveLoadSystem saveLoadSystem;
        [SerializeField] private MenuManager menuManager;

        [Header("Game State")]
        [SerializeField] private bool isGameInitialized = false;
        [SerializeField] private bool isGamePaused = false;
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int playerXP = 0;
        [SerializeField] private int xpToNextLevel = 100;

        [Header("Settings")]
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutos
        private float autoSaveTimer = 0f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeGame();
        }

        private void Update()
        {
            if (!isGameInitialized) return;

            // Auto-save
            if (autoSaveEnabled && !isGamePaused)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    AutoSave();
                    autoSaveTimer = 0f;
                }
            }

            // Atalhos de debug
            HandleDebugInput();
        }

        /// <summary>
        /// Inicializa todos os sistemas do jogo
        /// </summary>
        private void InitializeGame()
        {
            Debug.Log("=== Initializing Space RPG Systems ===");

            // Garantir que todos os sistemas existem
            EnsureSystemsExist();

            // Inicializar Item Database primeiro
            if (ItemDatabase.Instance != null)
            {
                ItemDatabase.Instance.Initialize();
                Debug.Log("✓ Item Database initialized");
            }

            // Inicializar outros sistemas
            InitializeSystems();

            isGameInitialized = true;
            Debug.Log("=== Space RPG Initialization Complete ===");
        }

        /// <summary>
        /// Garante que todos os sistemas singleton existem
        /// </summary>
        private void EnsureSystemsExist()
        {
            itemDatabase = ItemDatabase.Instance;
            inventorySystem = InventorySystem.Instance;
            shopSystem = ShopSystem.Instance;
            questSystem = QuestSystem.Instance;
            shipSystem = ShipSystem.Instance;
            maintenanceSystem = MaintenanceSystem.Instance;
            plantCareSystem = PlantCareSystemAdvanced.Instance;
            audioManager = AudioManager.Instance;
            saveLoadSystem = SaveLoadSystem.Instance;
            menuManager = MenuManager.Instance;
        }

        /// <summary>
        /// Inicializa sistemas individuais
        /// </summary>
        private void InitializeSystems()
        {
            // Inventory System
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnInventoryFull += HandleInventoryFull;
                InventorySystem.Instance.OnWeightExceeded += HandleWeightExceeded;
                Debug.Log("✓ Inventory System initialized");
            }

            // Shop System
            if (ShopSystem.Instance != null)
            {
                ShopSystem.Instance.OnTransactionFailed += HandleTransactionFailed;
                Debug.Log("✓ Shop System initialized");
            }

            // Quest System
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.OnQuestCompleted += HandleQuestCompleted;
                Debug.Log("✓ Quest System initialized");
            }

            // Ship System
            if (ShipSystem.Instance != null)
            {
                ShipSystem.Instance.OnShipDamaged += HandleShipDamaged;
                Debug.Log("✓ Ship System initialized");
            }

            // Maintenance System
            if (MaintenanceSystem.Instance != null)
            {
                Debug.Log("✓ Maintenance System initialized");
            }

            // Plant Care System
            if (PlantCareSystemAdvanced.Instance != null)
            {
                PlantCareSystemAdvanced.Instance.OnPlantDied += HandlePlantDied;
                Debug.Log("✓ Plant Care System initialized");
            }
        }

        /// <summary>
        /// Salva o jogo automaticamente
        /// </summary>
        private void AutoSave()
        {
            if (SaveLoadSystem.Instance != null)
            {
                bool success = SaveLoadSystem.Instance.SaveGame();
                if (success && MenuManager.Instance != null)
                {
                    MenuManager.Instance.ShowNotification("Auto-saved!");
                }
            }
        }

        /// <summary>
        /// Adiciona XP ao jogador
        /// </summary>
        public void AddXP(int amount)
        {
            playerXP += amount;

            // Level up
            while (playerXP >= xpToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Aumenta o nível do jogador
        /// </summary>
        private void LevelUp()
        {
            playerLevel++;
            playerXP -= xpToNextLevel;
            xpToNextLevel = Mathf.CeilToInt(xpToNextLevel * 1.5f);

            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowNotification($"LEVEL UP! Now level {playerLevel}");
            }

            Debug.Log($"Player leveled up to {playerLevel}!");
        }

        // Event Handlers
        private void HandleInventoryFull()
        {
            if (MenuManager.Instance != null)
                MenuManager.Instance.ShowNotification("Inventory is full!");
        }

        private void HandleWeightExceeded()
        {
            if (MenuManager.Instance != null)
                MenuManager.Instance.ShowNotification("Weight limit exceeded!");
        }

        private void HandleTransactionFailed(string reason)
        {
            if (MenuManager.Instance != null)
                MenuManager.Instance.ShowNotification($"Transaction failed: {reason}");
        }

        private void HandleQuestCompleted(Quest quest)
        {
            AddXP(quest.xpReward);

            if (MenuManager.Instance != null)
                MenuManager.Instance.ShowNotification($"Quest completed: {quest.questName}");
        }

        private void HandleShipDamaged(float damage)
        {
            // Avisar se a saúde está crítica
            if (ShipSystem.Instance != null)
            {
                float health = ShipSystem.Instance.GetCurrentHealth();
                if (health < 20f && MenuManager.Instance != null)
                {
                    MenuManager.Instance.ShowNotification("WARNING: Hull integrity critical!");
                }
            }
        }

        private void HandlePlantDied(Plant plant)
        {
            if (MenuManager.Instance != null)
                MenuManager.Instance.ShowNotification($"Plant died: {plant.plantName}");
        }

        /// <summary>
        /// Input de debug para testes
        /// </summary>
        private void HandleDebugInput()
        {
            // F1 - Adicionar 1000 créditos
            if (Input.GetKeyDown(KeyCode.F1))
            {
                InventorySystem.Instance?.AddCredits(1000);
                Debug.Log("Debug: Added 1000 credits");
            }

            // F2 - Restaurar saúde da nave
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ShipSystem.Instance?.RepairShip(1000f);
                Debug.Log("Debug: Ship fully repaired");
            }

            // F3 - Adicionar XP
            if (Input.GetKeyDown(KeyCode.F3))
            {
                AddXP(100);
                Debug.Log("Debug: Added 100 XP");
            }

            // F4 - Completar quest ativa
            if (Input.GetKeyDown(KeyCode.F4))
            {
                var activeQuests = QuestSystem.Instance?.GetActiveQuests();
                if (activeQuests != null && activeQuests.Count > 0)
                {
                    var quest = activeQuests[0];
                    QuestSystem.Instance.UpdateQuestProgress(quest.questID, quest.targetAmount);
                    Debug.Log($"Debug: Completed quest {quest.questName}");
                }
            }

            // F5 - Quick Save
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveLoadSystem.Instance?.SaveGame();
                Debug.Log("Debug: Quick save");
            }

            // F9 - Quick Load
            if (Input.GetKeyDown(KeyCode.F9))
            {
                SaveLoadSystem.Instance?.LoadGame();
                Debug.Log("Debug: Quick load");
            }
        }

        /// <summary>
        /// Retorna estatísticas gerais do jogo
        /// </summary>
        public string GetGameStats()
        {
            string stats = "=== GAME STATISTICS ===\n\n";

            stats += $"Player Level: {playerLevel}\n";
            stats += $"Player XP: {playerXP}/{xpToNextLevel}\n\n";

            if (InventorySystem.Instance != null)
                stats += InventorySystem.Instance.GetInventoryStats() + "\n\n";

            if (ShopSystem.Instance != null)
                stats += ShopSystem.Instance.GetShopStats() + "\n\n";

            if (QuestSystem.Instance != null)
                stats += QuestSystem.Instance.GetQuestStats() + "\n\n";

            if (ShipSystem.Instance != null)
                stats += ShipSystem.Instance.GetShipStats() + "\n\n";

            if (MaintenanceSystem.Instance != null)
                stats += MaintenanceSystem.Instance.GetMaintenanceStats() + "\n\n";

            if (PlantCareSystemAdvanced.Instance != null)
                stats += PlantCareSystemAdvanced.Instance.GetPlantCareStats() + "\n";

            return stats;
        }

        // Getters públicos
        public bool IsGameInitialized() => isGameInitialized;
        public bool IsGamePaused() => isGamePaused;
        public int GetPlayerLevel() => playerLevel;
        public int GetPlayerXP() => playerXP;
        public int GetXPToNextLevel() => xpToNextLevel;

        private void OnApplicationQuit()
        {
            if (autoSaveEnabled && SaveLoadSystem.Instance != null)
            {
                SaveLoadSystem.Instance.SaveGame();
                Debug.Log("Game auto-saved on quit");
            }
        }
    }
}

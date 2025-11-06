using UnityEngine;
using System.Collections.Generic;
using SpaceRPG.Data;
using System;

namespace SpaceRPG.Systems
{
    public enum MaintenanceType { Quick, Standard, Full, Emergency }
    public enum DamageType { None, Light, Moderate, Heavy, Critical }

    [System.Serializable]
    public class MaintenanceRecord
    {
        public string recordID;
        public string dateTime;
        public MaintenanceType type;
        public float healthRestored;
        public int creditsCost;
        public string notes;

        public MaintenanceRecord(MaintenanceType maintenanceType, float health, int cost)
        {
            recordID = Guid.NewGuid().ToString();
            dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            type = maintenanceType;
            healthRestored = health;
            creditsCost = cost;
        }
    }

    public class MaintenanceSystem : MonoBehaviour
    {
        private static MaintenanceSystem _instance;
        public static MaintenanceSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MaintenanceSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MaintenanceSystem");
                        _instance = go.AddComponent<MaintenanceSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Maintenance Costs")]
        [SerializeField] private int quickRepairCost = 100;
        [SerializeField] private int standardRepairCost = 300;
        [SerializeField] private int fullRepairCost = 500;
        [SerializeField] private int emergencyRepairCost = 800;

        [Header("Repair Amounts")]
        [SerializeField] private float quickRepairAmount = 25f;
        [SerializeField] private float standardRepairAmount = 50f;
        [SerializeField] private float fullRepairAmount = 100f;
        [SerializeField] private float emergencyRepairAmount = 100f;

        [Header("Tool Requirements")]
        [SerializeField] private bool requireRepairKit = true;
        [SerializeField] private string knifeToolID = "tool_knife"; // Canivete

        [Header("Maintenance Records")]
        [SerializeField] private List<MaintenanceRecord> maintenanceHistory = new List<MaintenanceRecord>();
        [SerializeField] private int maxHistoryRecords = 50;

        [Header("Statistics")]
        [SerializeField] private int totalRepairs = 0;
        [SerializeField] private int totalCreditsSpent = 0;
        [SerializeField] private float totalHealthRestored = 0f;

        [Header("Durability Upgrades")]
        [SerializeField] private int durabilityLevel = 1;
        [SerializeField] private float durabilityMultiplier = 1f;
        [SerializeField] private int durabilityUpgradeCost = 1000;

        public event Action<MaintenanceType, float> OnShipRepaired;
        public event Action<MaintenanceRecord> OnMaintenanceRecordAdded;
        public event Action<int> OnDurabilityUpgraded;
        public event Action<string> OnMaintenanceFailed;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Reparo rápido - restaura 25% da saúde
        /// </summary>
        public bool PerformQuickRepair()
        {
            return PerformRepair(MaintenanceType.Quick, quickRepairAmount, quickRepairCost);
        }

        /// <summary>
        /// Reparo padrão - restaura 50% da saúde
        /// </summary>
        public bool PerformStandardRepair()
        {
            return PerformRepair(MaintenanceType.Standard, standardRepairAmount, standardRepairCost);
        }

        /// <summary>
        /// Reparo completo - restaura 100% da saúde
        /// </summary>
        public bool PerformFullRepair()
        {
            return PerformRepair(MaintenanceType.Full, fullRepairAmount, fullRepairCost);
        }

        /// <summary>
        /// Reparo de emergência com canivete
        /// </summary>
        public bool PerformEmergencyRepair()
        {
            // Verificar se tem canivete
            if (!HasRequiredTool(knifeToolID))
            {
                OnMaintenanceFailed?.Invoke("Knife tool required for emergency repair");
                return false;
            }

            // Usar canivete (reduzir durabilidade)
            if (InventorySystem.Instance != null)
            {
                var knife = InventorySystem.Instance.FindItemByID(knifeToolID);
                if (knife != null)
                {
                    knife.durability -= 10f; // Degrada ao usar
                    if (knife.IsBroken())
                    {
                        Debug.LogWarning("Knife broke during repair!");
                        InventorySystem.Instance.RemoveItem(knifeToolID, 1);
                    }
                }
            }

            return PerformRepair(MaintenanceType.Emergency, emergencyRepairAmount, emergencyRepairCost);
        }

        /// <summary>
        /// Lógica geral de reparo
        /// </summary>
        private bool PerformRepair(MaintenanceType type, float repairAmount, int cost)
        {
            if (ShipSystem.Instance == null)
            {
                OnMaintenanceFailed?.Invoke("Ship system not available");
                return false;
            }

            ShipData currentShip = ShipSystem.Instance.GetCurrentShip();
            if (currentShip == null)
            {
                OnMaintenanceFailed?.Invoke("No ship equipped");
                return false;
            }

            // Verificar se precisa de reparo
            float currentHealth = ShipSystem.Instance.GetCurrentHealth();
            if (currentHealth >= currentShip.maxHealth)
            {
                OnMaintenanceFailed?.Invoke("Ship is already at full health");
                return false;
            }

            // Verificar créditos
            if (cost > 0 && (InventorySystem.Instance == null || InventorySystem.Instance.GetCurrentCredits() < cost))
            {
                OnMaintenanceFailed?.Invoke("Not enough credits for repair");
                return false;
            }

            // Verificar kit de reparo se necessário
            if (requireRepairKit && type != MaintenanceType.Emergency)
            {
                ItemData repairKit = ItemDatabase.Instance?.GetRepairKit();
                if (repairKit != null && !InventorySystem.Instance.HasItem(repairKit.itemID))
                {
                    OnMaintenanceFailed?.Invoke("Repair kit required");
                    return false;
                }

                // Usar kit de reparo
                InventorySystem.Instance.RemoveItem(repairKit.itemID, 1);
            }

            // Aplicar multiplicador de durabilidade
            float effectiveRepairAmount = repairAmount * durabilityMultiplier;

            // Executar reparo
            if (cost > 0)
            {
                InventorySystem.Instance.RemoveCredits(cost);
            }

            ShipSystem.Instance.RepairShip(effectiveRepairAmount);

            // Registrar manutenção
            MaintenanceRecord record = new MaintenanceRecord(type, effectiveRepairAmount, cost)
            {
                notes = $"Repaired {currentShip.shipName} using {type} maintenance"
            };
            AddMaintenanceRecord(record);

            // Atualizar estatísticas
            totalRepairs++;
            totalCreditsSpent += cost;
            totalHealthRestored += effectiveRepairAmount;

            OnShipRepaired?.Invoke(type, effectiveRepairAmount);
            Debug.Log($"Ship repaired: {type} (+{effectiveRepairAmount} HP, -{cost} credits)");

            // Atualizar missões
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.UpdateQuestProgressByType(QuestType.Repair);
            }

            return true;
        }

        /// <summary>
        /// Verifica se tem ferramenta necessária
        /// </summary>
        private bool HasRequiredTool(string toolID)
        {
            if (InventorySystem.Instance == null) return false;
            return InventorySystem.Instance.HasItem(toolID);
        }

        /// <summary>
        /// Adiciona registro ao histórico
        /// </summary>
        private void AddMaintenanceRecord(MaintenanceRecord record)
        {
            maintenanceHistory.Insert(0, record);

            // Limitar tamanho do histórico
            if (maintenanceHistory.Count > maxHistoryRecords)
            {
                maintenanceHistory.RemoveAt(maintenanceHistory.Count - 1);
            }

            OnMaintenanceRecordAdded?.Invoke(record);
        }

        /// <summary>
        /// Upgrade de durabilidade
        /// </summary>
        public bool UpgradeDurability()
        {
            int cost = GetDurabilityUpgradeCost();

            if (InventorySystem.Instance == null || InventorySystem.Instance.GetCurrentCredits() < cost)
            {
                OnMaintenanceFailed?.Invoke("Not enough credits for durability upgrade");
                return false;
            }

            InventorySystem.Instance.RemoveCredits(cost);
            durabilityLevel++;
            durabilityMultiplier += 0.1f; // +10% eficiência por nível

            OnDurabilityUpgraded?.Invoke(durabilityLevel);
            Debug.Log($"Durability upgraded to level {durabilityLevel} (x{durabilityMultiplier:F1})");

            return true;
        }

        /// <summary>
        /// Calcula custo do próximo upgrade de durabilidade
        /// </summary>
        public int GetDurabilityUpgradeCost()
        {
            return durabilityUpgradeCost * durabilityLevel;
        }

        /// <summary>
        /// Retorna tipo de dano baseado na saúde
        /// </summary>
        public DamageType GetDamageType()
        {
            if (ShipSystem.Instance == null) return DamageType.None;

            float damageLevel = ShipSystem.Instance.GetDamageLevel();

            if (damageLevel >= 80f) return DamageType.Critical;
            if (damageLevel >= 60f) return DamageType.Heavy;
            if (damageLevel >= 40f) return DamageType.Moderate;
            if (damageLevel >= 20f) return DamageType.Light;
            return DamageType.None;
        }

        /// <summary>
        /// Retorna custo recomendado de reparo
        /// </summary>
        public int GetRecommendedRepairCost()
        {
            DamageType damage = GetDamageType();

            switch (damage)
            {
                case DamageType.Light:
                    return quickRepairCost;
                case DamageType.Moderate:
                    return standardRepairCost;
                case DamageType.Heavy:
                case DamageType.Critical:
                    return fullRepairCost;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Retorna descrição do dano
        /// </summary>
        public string GetDamageDescription()
        {
            DamageType damage = GetDamageType();

            switch (damage)
            {
                case DamageType.None:
                    return "Ship is in perfect condition";
                case DamageType.Light:
                    return "Minor scratches and dents. Quick repair recommended.";
                case DamageType.Moderate:
                    return "Visible damage to hull. Standard repair needed.";
                case DamageType.Heavy:
                    return "Significant structural damage. Full repair required.";
                case DamageType.Critical:
                    return "CRITICAL DAMAGE! Immediate full repair necessary!";
                default:
                    return "Unknown damage status";
            }
        }

        // Getters públicos
        public List<MaintenanceRecord> GetMaintenanceHistory() => new List<MaintenanceRecord>(maintenanceHistory);
        public int GetTotalRepairs() => totalRepairs;
        public int GetTotalCreditsSpent() => totalCreditsSpent;
        public float GetTotalHealthRestored() => totalHealthRestored;
        public int GetDurabilityLevel() => durabilityLevel;
        public float GetDurabilityMultiplier() => durabilityMultiplier;

        /// <summary>
        /// Retorna estatísticas de manutenção
        /// </summary>
        public string GetMaintenanceStats()
        {
            return $"Total Repairs: {totalRepairs}\n" +
                   $"Credits Spent: {totalCreditsSpent}\n" +
                   $"Health Restored: {totalHealthRestored:F0}\n" +
                   $"Durability Level: {durabilityLevel}\n" +
                   $"Efficiency: x{durabilityMultiplier:F1}\n" +
                   $"Current Damage: {GetDamageType()}\n" +
                   $"{GetDamageDescription()}";
        }

        /// <summary>
        /// Limpa histórico de manutenção
        /// </summary>
        public void ClearHistory()
        {
            maintenanceHistory.Clear();
            Debug.Log("Maintenance history cleared");
        }
    }
}

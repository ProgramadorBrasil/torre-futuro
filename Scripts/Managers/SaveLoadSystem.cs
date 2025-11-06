using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SpaceRPG.Data;
using SpaceRPG.Systems;
using System.Collections.Generic;

namespace SpaceRPG.Core
{
    [System.Serializable]
    public class SaveData
    {
        public int playerLevel;
        public int currentCredits;
        public float currentWeight;

        public List<InventoryItemSave> inventoryItems = new List<InventoryItemSave>();
        public List<QuestSave> activeQuests = new List<QuestSave>();
        public List<string> completedQuestIDs = new List<string>();

        public string currentShipID;
        public List<string> ownedShipIDs = new List<string>();

        public int totalPurchases;
        public int totalSales;
        public List<string> wishlist = new List<string>();

        public float musicVolume;
        public float sfxVolume;
        public float ambientVolume;

        public string saveDate;
        public float playTime;
    }

    [System.Serializable]
    public class InventoryItemSave
    {
        public string itemID;
        public int quantity;
        public bool isEquipped;
        public float durability;
    }

    [System.Serializable]
    public class QuestSave
    {
        public string questID;
        public int currentAmount;
        public float timeRemaining;
    }

    public class SaveLoadSystem : MonoBehaviour
    {
        private static SaveLoadSystem _instance;
        public static SaveLoadSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SaveLoadSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SaveLoadSystem");
                        _instance = go.AddComponent<SaveLoadSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "spacerpg_save.dat";
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutos

        private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
        private float autoSaveTimer;
        private float playTime;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            playTime += Time.deltaTime;

            if (autoSave)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    SaveGame();
                    autoSaveTimer = 0f;
                }
            }
        }

        public bool SaveGame(string customFileName = null)
        {
            try
            {
                SaveData data = new SaveData
                {
                    saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    playTime = playTime
                };

                // Salvar dados de inventário
                if (InventorySystem.Instance != null)
                {
                    data.currentCredits = InventorySystem.Instance.GetCurrentCredits();
                    data.currentWeight = InventorySystem.Instance.GetCurrentWeight();

                    foreach (var item in InventorySystem.Instance.GetAllItems())
                    {
                        data.inventoryItems.Add(new InventoryItemSave
                        {
                            itemID = item.itemData.itemID,
                            quantity = item.quantity,
                            isEquipped = item.isEquipped,
                            durability = item.durability
                        });
                    }
                }

                // Salvar dados de missões
                if (QuestSystem.Instance != null)
                {
                    foreach (var quest in QuestSystem.Instance.GetActiveQuests())
                    {
                        data.activeQuests.Add(new QuestSave
                        {
                            questID = quest.questID,
                            currentAmount = quest.currentAmount,
                            timeRemaining = quest.timeRemaining
                        });
                    }

                    foreach (var quest in QuestSystem.Instance.GetCompletedQuests())
                    {
                        data.completedQuestIDs.Add(quest.questID);
                    }
                }

                // Salvar dados de loja
                if (ShopSystem.Instance != null)
                {
                    data.totalPurchases = ShopSystem.Instance.GetTotalPurchases();
                    data.totalSales = ShopSystem.Instance.GetTotalSales();
                }

                // Salvar dados de áudio
                if (AudioManager.Instance != null)
                {
                    // data.musicVolume = AudioManager.Instance.GetMusicVolume();
                    // data.sfxVolume = AudioManager.Instance.GetSFXVolume();
                    // data.ambientVolume = AudioManager.Instance.GetAmbientVolume();
                }

                // Serializar e salvar
                string path = string.IsNullOrEmpty(customFileName) ? SavePath : Path.Combine(Application.persistentDataPath, customFileName);
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    formatter.Serialize(stream, data);
                }

                Debug.Log($"Game saved successfully to {path}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                return false;
            }
        }

        public bool LoadGame(string customFileName = null)
        {
            try
            {
                string path = string.IsNullOrEmpty(customFileName) ? SavePath : Path.Combine(Application.persistentDataPath, customFileName);

                if (!File.Exists(path))
                {
                    Debug.LogWarning($"Save file not found: {path}");
                    return false;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                SaveData data;

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    data = formatter.Deserialize(stream) as SaveData;
                }

                if (data == null)
                {
                    Debug.LogError("Failed to deserialize save data");
                    return false;
                }

                // Carregar dados de inventário
                if (InventorySystem.Instance != null)
                {
                    InventorySystem.Instance.ClearInventory();

                    foreach (var itemSave in data.inventoryItems)
                    {
                        ItemData itemData = ItemDatabase.Instance.GetItemByID(itemSave.itemID);
                        if (itemData != null)
                        {
                            InventorySystem.Instance.AddItem(itemData, itemSave.quantity);
                            var item = InventorySystem.Instance.FindItemByID(itemSave.itemID);
                            if (item != null)
                            {
                                item.durability = itemSave.durability;
                                if (itemSave.isEquipped)
                                {
                                    InventorySystem.Instance.EquipItem(itemSave.itemID);
                                }
                            }
                        }
                    }

                    // Restaurar créditos (remover os créditos adicionados e definir o valor correto)
                    int currentCredits = InventorySystem.Instance.GetCurrentCredits();
                    InventorySystem.Instance.RemoveCredits(currentCredits);
                    InventorySystem.Instance.AddCredits(data.currentCredits);
                }

                // Carregar dados de missões
                if (QuestSystem.Instance != null)
                {
                    // Implementar carregamento de missões
                }

                playTime = data.playTime;

                Debug.Log($"Game loaded successfully from {path}");
                Debug.Log($"Save date: {data.saveDate}, Play time: {playTime:F0}s");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return false;
            }
        }

        public bool SaveExists(string customFileName = null)
        {
            string path = string.IsNullOrEmpty(customFileName) ? SavePath : Path.Combine(Application.persistentDataPath, customFileName);
            return File.Exists(path);
        }

        public void DeleteSave(string customFileName = null)
        {
            string path = string.IsNullOrEmpty(customFileName) ? SavePath : Path.Combine(Application.persistentDataPath, customFileName);

            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Save file deleted: {path}");
            }
        }

        public string GetSaveInfo(string customFileName = null)
        {
            if (!SaveExists(customFileName))
                return "No save file found";

            try
            {
                string path = string.IsNullOrEmpty(customFileName) ? SavePath : Path.Combine(Application.persistentDataPath, customFileName);
                BinaryFormatter formatter = new BinaryFormatter();
                SaveData data;

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    data = formatter.Deserialize(stream) as SaveData;
                }

                if (data == null)
                    return "Invalid save file";

                return $"Save Date: {data.saveDate}\n" +
                       $"Play Time: {FormatTime(data.playTime)}\n" +
                       $"Level: {data.playerLevel}\n" +
                       $"Credits: {data.currentCredits}\n" +
                       $"Items: {data.inventoryItems.Count}\n" +
                       $"Active Quests: {data.activeQuests.Count}\n" +
                       $"Completed Quests: {data.completedQuestIDs.Count}";
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to read save info: {e.Message}");
                return "Error reading save file";
            }
        }

        private string FormatTime(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600f);
            int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{hours:00}:{minutes:00}:{secs:00}";
        }

        public float GetPlayTime() => playTime;
    }
}

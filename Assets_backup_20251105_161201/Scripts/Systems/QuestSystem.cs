using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SpaceRPG.Data;
using System;

namespace SpaceRPG.Systems
{
    public enum QuestType { Combat, Exploration, PlantCare, Harvest, Repair, Delivery, Collection }
    public enum QuestStatus { Locked, Available, InProgress, Completed, Failed }
    public enum QuestDifficulty { Easy, Medium, Hard, Elite, Legendary }

    [System.Serializable]
    public class Quest
    {
        public string questID;
        public string questName;
        public string description;
        public Sprite icon;
        public QuestType type;
        public QuestStatus status;
        public QuestDifficulty difficulty;

        public int targetAmount;
        public int currentAmount;

        public int creditsReward;
        public int xpReward;
        public List<ItemData> itemRewards = new List<ItemData>();

        public float timeLimit; // -1 = sem limite
        public float timeRemaining;
        public bool isDailyQuest;
        public bool isWeeklyQuest;

        public List<string> prerequisites = new List<string>();
        public int levelRequirement;

        public string targetID; // ID do inimigo, planta, local, etc

        public Quest(string id, string name, QuestType questType)
        {
            questID = id;
            questName = name;
            type = questType;
            status = QuestStatus.Available;
            difficulty = QuestDifficulty.Easy;
            targetAmount = 1;
            currentAmount = 0;
            timeLimit = -1;
        }

        public float GetProgress() => targetAmount > 0 ? (float)currentAmount / targetAmount : 0f;
        public bool IsComplete() => currentAmount >= targetAmount;
        public bool IsTimedOut() => timeLimit > 0 && timeRemaining <= 0;
    }

    public class QuestSystem : MonoBehaviour
    {
        private static QuestSystem _instance;
        public static QuestSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<QuestSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("QuestSystem");
                        _instance = go.AddComponent<QuestSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Quest Configuration")]
        [SerializeField] private int maxActiveQuests = 5;
        [SerializeField] private int maxDailyQuests = 3;

        [Header("Quest Lists")]
        [SerializeField] private List<Quest> allQuests = new List<Quest>();
        [SerializeField] private List<Quest> activeQuests = new List<Quest>();
        [SerializeField] private List<Quest> completedQuests = new List<Quest>();
        [SerializeField] private List<Quest> dailyQuests = new List<Quest>();

        [Header("Player Progress")]
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int totalQuestsCompleted = 0;
        [SerializeField] private int totalCreditsEarned = 0;

        public event Action<Quest> OnQuestAccepted;
        public event Action<Quest> OnQuestCompleted;
        public event Action<Quest> OnQuestFailed;
        public event Action<Quest> OnQuestProgressUpdated;
        public event Action OnQuestsRefreshed;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeQuests();
            GenerateDailyQuests();
        }

        private void Update()
        {
            UpdateQuestTimers();
        }

        private void InitializeQuests()
        {
            // Criar missões de exemplo
            CreateSampleQuests();
            Debug.Log($"Quest System initialized with {allQuests.Count} quests");
        }

        private void CreateSampleQuests()
        {
            // Missão de combate
            Quest combatQuest = new Quest("combat_001", "Space Cleaner", QuestType.Combat)
            {
                description = "Defeat 10 enemy ships in the sector",
                targetAmount = 10,
                creditsReward = 500,
                xpReward = 100,
                difficulty = QuestDifficulty.Easy
            };
            allQuests.Add(combatQuest);

            // Missão de plantas
            Quest plantQuest = new Quest("plant_001", "Green Thumb", QuestType.PlantCare)
            {
                description = "Water 5 plants successfully",
                targetAmount = 5,
                creditsReward = 300,
                xpReward = 50,
                difficulty = QuestDifficulty.Easy
            };
            allQuests.Add(plantQuest);

            // Missão de reparo
            Quest repairQuest = new Quest("repair_001", "Mechanic", QuestType.Repair)
            {
                description = "Repair your ship 3 times",
                targetAmount = 3,
                creditsReward = 400,
                xpReward = 75,
                difficulty = QuestDifficulty.Medium
            };
            allQuests.Add(repairQuest);

            // Missão de exploração
            Quest exploreQuest = new Quest("explore_001", "Explorer", QuestType.Exploration)
            {
                description = "Visit 5 different sectors",
                targetAmount = 5,
                creditsReward = 600,
                xpReward = 120,
                difficulty = QuestDifficulty.Medium
            };
            allQuests.Add(exploreQuest);
        }

        public bool AcceptQuest(string questID)
        {
            Quest quest = allQuests.FirstOrDefault(q => q.questID == questID);
            if (quest == null || quest.status != QuestStatus.Available) return false;

            if (activeQuests.Count >= maxActiveQuests)
            {
                Debug.LogWarning("Max active quests reached");
                return false;
            }

            if (quest.levelRequirement > playerLevel)
            {
                Debug.LogWarning("Level requirement not met");
                return false;
            }

            quest.status = QuestStatus.InProgress;
            quest.currentAmount = 0;
            quest.timeRemaining = quest.timeLimit;
            activeQuests.Add(quest);

            OnQuestAccepted?.Invoke(quest);
            Debug.Log($"Quest accepted: {quest.questName}");
            return true;
        }

        public void UpdateQuestProgress(string questID, int amount = 1)
        {
            Quest quest = activeQuests.FirstOrDefault(q => q.questID == questID);
            if (quest == null) return;

            quest.currentAmount = Mathf.Min(quest.currentAmount + amount, quest.targetAmount);
            OnQuestProgressUpdated?.Invoke(quest);

            if (quest.IsComplete())
            {
                CompleteQuest(questID);
            }
        }

        public void UpdateQuestProgressByType(QuestType type, string targetID = "", int amount = 1)
        {
            var relevantQuests = activeQuests.Where(q => q.type == type);

            if (!string.IsNullOrEmpty(targetID))
                relevantQuests = relevantQuests.Where(q => string.IsNullOrEmpty(q.targetID) || q.targetID == targetID);

            foreach (var quest in relevantQuests)
            {
                UpdateQuestProgress(quest.questID, amount);
            }
        }

        private void CompleteQuest(string questID)
        {
            Quest quest = activeQuests.FirstOrDefault(q => q.questID == questID);
            if (quest == null) return;

            quest.status = QuestStatus.Completed;
            activeQuests.Remove(quest);
            completedQuests.Add(quest);

            // Dar recompensas
            GiveRewards(quest);

            totalQuestsCompleted++;
            OnQuestCompleted?.Invoke(quest);
            Debug.Log($"Quest completed: {quest.questName}");
        }

        private void GiveRewards(Quest quest)
        {
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddCredits(quest.creditsReward);
                totalCreditsEarned += quest.creditsReward;

                foreach (var item in quest.itemRewards)
                {
                    InventorySystem.Instance.AddItem(item, 1);
                }
            }
        }

        public void AbandonQuest(string questID)
        {
            Quest quest = activeQuests.FirstOrDefault(q => q.questID == questID);
            if (quest == null) return;

            quest.status = QuestStatus.Available;
            quest.currentAmount = 0;
            activeQuests.Remove(quest);
            Debug.Log($"Quest abandoned: {quest.questName}");
        }

        private void UpdateQuestTimers()
        {
            foreach (var quest in activeQuests.ToList())
            {
                if (quest.timeLimit > 0)
                {
                    quest.timeRemaining -= Time.deltaTime;
                    if (quest.IsTimedOut())
                    {
                        FailQuest(quest.questID);
                    }
                }
            }
        }

        private void FailQuest(string questID)
        {
            Quest quest = activeQuests.FirstOrDefault(q => q.questID == questID);
            if (quest == null) return;

            quest.status = QuestStatus.Failed;
            activeQuests.Remove(quest);

            OnQuestFailed?.Invoke(quest);
            Debug.Log($"Quest failed: {quest.questName}");
        }

        private void GenerateDailyQuests()
        {
            dailyQuests.Clear();

            var availableQuests = allQuests.Where(q =>
                q.status == QuestStatus.Available &&
                !activeQuests.Contains(q) &&
                !completedQuests.Contains(q)
            ).OrderBy(x => UnityEngine.Random.value).Take(maxDailyQuests).ToList();

            foreach (var quest in availableQuests)
            {
                quest.isDailyQuest = true;
                dailyQuests.Add(quest);
            }

            Debug.Log($"Generated {dailyQuests.Count} daily quests");
        }

        public List<Quest> GetAvailableQuests() => allQuests.Where(q => q.status == QuestStatus.Available).ToList();
        public List<Quest> GetActiveQuests() => new List<Quest>(activeQuests);
        public List<Quest> GetCompletedQuests() => new List<Quest>(completedQuests);
        public List<Quest> GetDailyQuests() => new List<Quest>(dailyQuests);
        public Quest GetQuestByID(string questID) => allQuests.FirstOrDefault(q => q.questID == questID);

        public string GetQuestStats()
        {
            return $"Active Quests: {activeQuests.Count}/{maxActiveQuests}\n" +
                   $"Completed: {totalQuestsCompleted}\n" +
                   $"Credits Earned: {totalCreditsEarned}\n" +
                   $"Player Level: {playerLevel}";
        }
    }
}

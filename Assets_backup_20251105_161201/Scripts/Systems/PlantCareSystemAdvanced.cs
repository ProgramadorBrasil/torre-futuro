using UnityEngine;
using System.Collections.Generic;
using SpaceRPG.Data;
using System;

namespace SpaceRPG.Systems
{
    public enum PlantState { Seed, Sprout, Growing, Mature, Flowering, Harvestable, Withered, Dead }
    public enum PlantNeed { Water, Fertilizer, Pesticide, Sunlight }

    [System.Serializable]
    public class Plant
    {
        public string plantID;
        public string plantName;
        public GameObject plantPrefab;
        public PlantState state;

        [Header("Health & Needs")]
        public float health = 100f;
        public float waterLevel = 100f;
        public float nutrientLevel = 100f;
        public bool hasPests = false;

        [Header("Growth")]
        public float growthProgress = 0f;
        public float growthRate = 1f;
        public float timeToMature = 120f; // segundos

        [Header("Harvest")]
        public bool canBeHarvested = false;
        public ItemData harvestItem;
        public int harvestAmount = 1;

        [Header("Care History")]
        public int timesWatered = 0;
        public int timesFertilized = 0;
        public int timesPesticideUsed = 0;
        public float lastCareTime = 0f;

        public Plant(string id, string name)
        {
            plantID = id;
            plantName = name;
            state = PlantState.Seed;
            health = 100f;
            waterLevel = 100f;
            nutrientLevel = 100f;
        }

        public float GetHealthPercentage() => health / 100f;
        public bool IsHealthy() => health > 70f && waterLevel > 30f && nutrientLevel > 30f && !hasPests;
        public bool IsDead() => health <= 0f || state == PlantState.Dead;
    }

    public class PlantCareSystemAdvanced : MonoBehaviour
    {
        private static PlantCareSystemAdvanced _instance;
        public static PlantCareSystemAdvanced Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PlantCareSystemAdvanced>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PlantCareSystemAdvanced");
                        _instance = go.AddComponent<PlantCareSystemAdvanced>();
                    }
                }
                return _instance;
            }
        }

        [Header("Plant Management")]
        [SerializeField] private List<Plant> allPlants = new List<Plant>();
        [SerializeField] private Transform plantContainer;

        [Header("Care Settings")]
        [SerializeField] private float waterDepletionRate = 5f; // por minuto
        [SerializeField] private float nutrientDepletionRate = 3f;
        [SerializeField] private float pestChance = 0.1f; // 10% por minuto
        [SerializeField] private float healthDecayRate = 2f; // quando precisa de cuidados

        [Header("Care Amounts")]
        [SerializeField] private float waterRestoreAmount = 40f;
        [SerializeField] private float fertilizerRestoreAmount = 50f;
        [SerializeField] private float healthRestoreAmount = 20f;

        [Header("Statistics")]
        [SerializeField] private int totalPlantsGrown = 0;
        [SerializeField] private int totalHarvests = 0;
        [SerializeField] private int totalPlantDeaths = 0;

        public event Action<Plant> OnPlantAdded;
        public event Action<Plant> OnPlantWatered;
        public event Action<Plant> OnPlantFertilized;
        public event Action<Plant> OnPesticidetUsed;
        public event Action<Plant> OnPlantHarvested;
        public event Action<Plant> OnPlantDied;
        public event Action<Plant> OnPlantStateChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            UpdatePlants();
        }

        private void UpdatePlants()
        {
            float deltaMinutes = Time.deltaTime / 60f;

            foreach (var plant in allPlants.ToArray())
            {
                if (plant.IsDead()) continue;

                // Depletar recursos
                plant.waterLevel = Mathf.Max(0, plant.waterLevel - waterDepletionRate * deltaMinutes);
                plant.nutrientLevel = Mathf.Max(0, plant.nutrientLevel - nutrientDepletionRate * deltaMinutes);

                // Chance de pragas
                if (UnityEngine.Random.value < pestChance * deltaMinutes && !plant.hasPests)
                {
                    plant.hasPests = true;
                    Debug.Log($"Plant {plant.plantName} got pests!");
                }

                // Reduzir saúde se precisar de cuidados
                if (plant.waterLevel < 20f || plant.nutrientLevel < 20f || plant.hasPests)
                {
                    plant.health = Mathf.Max(0, plant.health - healthDecayRate * deltaMinutes);

                    if (plant.health <= 0)
                    {
                        KillPlant(plant);
                    }
                }

                // Crescer se estiver saudável
                if (plant.IsHealthy() && plant.state != PlantState.Harvestable)
                {
                    plant.growthProgress += plant.growthRate * Time.deltaTime;

                    if (plant.growthProgress >= plant.timeToMature)
                    {
                        MaturePlant(plant);
                    }
                    else
                    {
                        UpdatePlantState(plant);
                    }
                }
            }
        }

        public Plant PlantSeed(ItemData seedItem, Vector3 position)
        {
            if (seedItem == null || seedItem.itemType != ItemType.Seed)
            {
                Debug.LogWarning("Invalid seed item");
                return null;
            }

            Plant plant = new Plant(Guid.NewGuid().ToString(), seedItem.itemName);
            plant.state = PlantState.Seed;

            // Instanciar prefab se disponível
            if (seedItem.prefab != null)
            {
                plant.plantPrefab = Instantiate(seedItem.prefab, position, Quaternion.identity, plantContainer);
                plant.plantPrefab.name = plant.plantName;
            }

            allPlants.Add(plant);
            totalPlantsGrown++;

            OnPlantAdded?.Invoke(plant);
            Debug.Log($"Planted: {plant.plantName}");

            // Atualizar missões
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.UpdateQuestProgressByType(QuestType.PlantCare, seedItem.itemID);
            }

            return plant;
        }

        public bool WaterPlant(string plantID)
        {
            Plant plant = GetPlantByID(plantID);
            if (plant == null || plant.IsDead())
            {
                Debug.LogWarning("Cannot water plant");
                return false;
            }

            // Verificar se tem item de água no inventário
            ItemData waterItem = ItemDatabase.Instance?.GetWaterItem();
            if (waterItem != null && !InventorySystem.Instance.HasItem(waterItem.itemID))
            {
                Debug.LogWarning("No water item in inventory");
                return false;
            }

            // Usar água
            if (waterItem != null)
            {
                InventorySystem.Instance.RemoveItem(waterItem.itemID, 1);
            }

            // Regar planta
            plant.waterLevel = Mathf.Min(100f, plant.waterLevel + waterRestoreAmount);
            plant.health = Mathf.Min(100f, plant.health + healthRestoreAmount * 0.5f);
            plant.timesWatered++;
            plant.lastCareTime = Time.time;

            OnPlantWatered?.Invoke(plant);
            Debug.Log($"Watered plant: {plant.plantName}");

            // Atualizar missões
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.UpdateQuestProgressByType(QuestType.PlantCare, plantID);
            }

            return true;
        }

        public bool FertilizePlant(string plantID)
        {
            Plant plant = GetPlantByID(plantID);
            if (plant == null || plant.IsDead()) return false;

            // Verificar fertilizante no inventário
            ItemData fertilizerItem = ItemDatabase.Instance?.GetFertilizerItem();
            if (fertilizerItem != null && !InventorySystem.Instance.HasItem(fertilizerItem.itemID))
            {
                Debug.LogWarning("No fertilizer in inventory");
                return false;
            }

            // Usar fertilizante
            if (fertilizerItem != null)
            {
                InventorySystem.Instance.RemoveItem(fertilizerItem.itemID, 1);
            }

            // Fertilizar
            plant.nutrientLevel = Mathf.Min(100f, plant.nutrientLevel + fertilizerRestoreAmount);
            plant.health = Mathf.Min(100f, plant.health + healthRestoreAmount * 0.3f);
            plant.growthRate += 0.1f; // Boost de crescimento
            plant.timesFertilized++;
            plant.lastCareTime = Time.time;

            OnPlantFertilized?.Invoke(plant);
            Debug.Log($"Fertilized plant: {plant.plantName}");

            return true;
        }

        public bool UsePesticide(string plantID)
        {
            Plant plant = GetPlantByID(plantID);
            if (plant == null || plant.IsDead() || !plant.hasPests) return false;

            // Verificar pesticida no inventário
            // ItemData pesticideItem = ItemDatabase.Instance?.GetPesticideItem();

            plant.hasPests = false;
            plant.health = Mathf.Min(100f, plant.health + healthRestoreAmount);
            plant.timesPesticideUsed++;
            plant.lastCareTime = Time.time;

            OnPesticidetUsed?.Invoke(plant);
            Debug.Log($"Used pesticide on: {plant.plantName}");

            return true;
        }

        public bool HarvestPlant(string plantID)
        {
            Plant plant = GetPlantByID(plantID);
            if (plant == null || !plant.canBeHarvested) return false;

            // Dar recompensas
            if (plant.harvestItem != null && InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(plant.harvestItem, plant.harvestAmount);
            }

            totalHarvests++;

            OnPlantHarvested?.Invoke(plant);
            Debug.Log($"Harvested: {plant.plantName}");

            // Atualizar missões
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.UpdateQuestProgressByType(QuestType.Harvest, plantID);
            }

            // Remover planta
            RemovePlant(plantID);

            return true;
        }

        private void UpdatePlantState(Plant plant)
        {
            PlantState oldState = plant.state;
            float progress = plant.growthProgress / plant.timeToMature;

            if (progress < 0.2f)
                plant.state = PlantState.Seed;
            else if (progress < 0.4f)
                plant.state = PlantState.Sprout;
            else if (progress < 0.6f)
                plant.state = PlantState.Growing;
            else if (progress < 0.8f)
                plant.state = PlantState.Mature;
            else if (progress < 1.0f)
                plant.state = PlantState.Flowering;

            if (plant.state != oldState)
            {
                OnPlantStateChanged?.Invoke(plant);
            }
        }

        private void MaturePlant(Plant plant)
        {
            plant.state = PlantState.Harvestable;
            plant.canBeHarvested = true;

            OnPlantStateChanged?.Invoke(plant);
            Debug.Log($"Plant ready for harvest: {plant.plantName}");
        }

        private void KillPlant(Plant plant)
        {
            plant.state = PlantState.Dead;
            plant.health = 0f;
            totalPlantDeaths++;

            OnPlantDied?.Invoke(plant);
            Debug.Log($"Plant died: {plant.plantName}");
        }

        private void RemovePlant(string plantID)
        {
            Plant plant = GetPlantByID(plantID);
            if (plant == null) return;

            if (plant.plantPrefab != null)
            {
                Destroy(plant.plantPrefab);
            }

            allPlants.Remove(plant);
        }

        public Plant GetPlantByID(string plantID) => allPlants.Find(p => p.plantID == plantID);
        public List<Plant> GetAllPlants() => new List<Plant>(allPlants);
        public List<Plant> GetHealthyPlants() => allPlants.FindAll(p => p.IsHealthy());
        public List<Plant> GetHarvestablePlants() => allPlants.FindAll(p => p.canBeHarvested);

        public string GetPlantCareStats()
        {
            return $"Total Plants: {allPlants.Count}\n" +
                   $"Healthy: {GetHealthyPlants().Count}\n" +
                   $"Harvestable: {GetHarvestablePlants().Count}\n" +
                   $"Total Grown: {totalPlantsGrown}\n" +
                   $"Total Harvests: {totalHarvests}\n" +
                   $"Total Deaths: {totalPlantDeaths}";
        }
    }
}

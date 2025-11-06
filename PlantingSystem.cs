using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Advanced Planting System with procedural plant growth, multiple plant types, and reward integration
/// Supports planting, growth animation, harvesting, and economic integration
/// </summary>
public class PlantingSystem : MonoBehaviour
{
    #region Plant Types
    public enum PlantType
    {
        EnergyFlower,    // Restores energy when harvested
        CreditFruit,     // Gives credits when harvested
        HealingHerb,     // Restores health when harvested
        RareCrystal      // High value, long growth time
    }

    [System.Serializable]
    public class PlantData
    {
        public PlantType type;
        public string plantName;
        public string description;
        public GameObject plantPrefab;
        public int seedCost;
        public float growthTime; // Seconds to fully grow
        public int harvestValue; // Credits earned
        public int harvestXP;    // XP earned
        public Color plantColor;
        public bool unlocked;

        // Visual stages
        public GameObject[] growthStagePrefabs; // Optional pre-made growth stages
        public Vector3[] growthStageScales;    // Scale per growth stage
    }
    #endregion

    [Header("Plant Configuration")]
    [SerializeField] private List<PlantData> plantTypes = new List<PlantData>();
    [SerializeField] private PlantType selectedPlantType = PlantType.EnergyFlower;

    [Header("Planting Settings")]
    [SerializeField] private LayerMask plantableGround;
    [SerializeField] private float plantingRange = 5f;
    [SerializeField] private float minPlantSpacing = 2f;
    [SerializeField] private int maxPlantsPerPlayer = 50;

    [Header("Growth Settings")]
    [SerializeField] private int growthStages = 5;
    [SerializeField] private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useRealTimeGrowth = false; // Continue growing when game is closed

    [Header("Visual Effects")]
    [SerializeField] private GameObject plantEffectPrefab;
    [SerializeField] private GameObject harvestEffectPrefab;
    [SerializeField] private ParticleSystem growthParticles;
    [SerializeField] private Material plantMaterial;

    [Header("Audio")]
    [SerializeField] private AudioClip plantSound;
    [SerializeField] private AudioClip growthSound;
    [SerializeField] private AudioClip harvestSound;
    [SerializeField] private AudioSource audioSource;

    [Header("UI Settings")]
    [SerializeField] private GameObject plantingReticle;
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;

    [Header("References")]
    [SerializeField] private UpgradeSystem upgradeSystem;
    [SerializeField] private RewardSystem rewardSystem;
    [SerializeField] private Transform playerTransform;

    // Private variables
    private List<PlantInstance> activePlants = new List<PlantInstance>();
    private bool isPlantingMode = false;
    private Vector3 plantingPosition;
    private bool canPlant = false;
    private Camera mainCamera;
    private Dictionary<PlantType, PlantData> plantDict = new Dictionary<PlantType, PlantData>();

    // Events
    public delegate void PlantEvent(PlantType type);
    public event PlantEvent OnPlantPlanted;
    public event PlantEvent OnPlantHarvested;
    public event System.Action<int> OnPlantCountChanged;

    #region Plant Instance Class
    private class PlantInstance
    {
        public GameObject gameObject;
        public PlantType type;
        public float plantedTime;
        public float growthDuration;
        public bool isFullyGrown;
        public bool isHarvested;
        public int currentStage;
        public Vector3 finalScale;
        public Material instanceMaterial;
    }
    #endregion

    #region Initialization

    private void Awake()
    {
        mainCamera = Camera.main;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        InitializePlantTypes();

        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        if (upgradeSystem == null)
        {
            upgradeSystem = FindObjectOfType<UpgradeSystem>();
        }

        if (rewardSystem == null)
        {
            rewardSystem = FindObjectOfType<RewardSystem>();
        }
    }

    private void Start()
    {
        if (plantingReticle != null)
        {
            plantingReticle.SetActive(false);
        }
    }

    private void InitializePlantTypes()
    {
        if (plantTypes.Count == 0)
        {
            CreateDefaultPlantTypes();
        }

        plantDict.Clear();
        foreach (var plant in plantTypes)
        {
            if (!plantDict.ContainsKey(plant.type))
            {
                plantDict.Add(plant.type, plant);

                // Initialize growth stage scales if not set
                if (plant.growthStageScales == null || plant.growthStageScales.Length == 0)
                {
                    plant.growthStageScales = new Vector3[growthStages];
                    for (int i = 0; i < growthStages; i++)
                    {
                        float t = (i + 1) / (float)growthStages;
                        plant.growthStageScales[i] = Vector3.one * t;
                    }
                }
            }
        }
    }

    private void CreateDefaultPlantTypes()
    {
        // Energy Flower
        plantTypes.Add(new PlantData
        {
            type = PlantType.EnergyFlower,
            plantName = "Energy Blossom",
            description = "Restores 30 energy when harvested",
            seedCost = 50,
            growthTime = 30f, // 30 seconds
            harvestValue = 100,
            harvestXP = 20,
            plantColor = Color.cyan,
            unlocked = true
        });

        // Credit Fruit
        plantTypes.Add(new PlantData
        {
            type = PlantType.CreditFruit,
            plantName = "Golden Fruit",
            description = "Yields 200 credits when harvested",
            seedCost = 100,
            growthTime = 60f, // 1 minute
            harvestValue = 200,
            harvestXP = 40,
            plantColor = Color.yellow,
            unlocked = true
        });

        // Healing Herb
        plantTypes.Add(new PlantData
        {
            type = PlantType.HealingHerb,
            plantName = "Regeneration Herb",
            description = "Restores 50 health when harvested",
            seedCost = 150,
            growthTime = 45f,
            harvestValue = 150,
            harvestXP = 30,
            plantColor = Color.green,
            unlocked = true
        });

        // Rare Crystal
        plantTypes.Add(new PlantData
        {
            type = PlantType.RareCrystal,
            plantName = "Quantum Crystal",
            description = "High value crystal, takes time to grow",
            seedCost = 300,
            growthTime = 120f, // 2 minutes
            harvestValue = 500,
            harvestXP = 100,
            plantColor = Color.magenta,
            unlocked = false
        });
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        HandleInput();
        UpdatePlantingReticle();
        UpdatePlantGrowth();
    }

    private void HandleInput()
    {
        // Toggle planting mode (P key)
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePlantingMode();
        }

        // Cycle through plant types ([ ] keys)
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            SelectPreviousPlantType();
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            SelectNextPlantType();
        }

        // Plant seed (Left Click when in planting mode)
        if (isPlantingMode && Input.GetMouseButtonDown(0))
        {
            TryPlantSeed();
        }

        // Harvest nearby plants (H key)
        if (Input.GetKeyDown(KeyCode.H))
        {
            HarvestNearbyPlants();
        }
    }

    private void UpdatePlantingReticle()
    {
        if (!isPlantingMode || plantingReticle == null) return;

        // Raycast to find planting position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, plantingRange, plantableGround))
        {
            plantingPosition = hit.point;
            plantingReticle.transform.position = plantingPosition;

            // Check if position is valid
            canPlant = IsValidPlantingPosition(plantingPosition);

            // Update reticle color
            Renderer reticleRenderer = plantingReticle.GetComponent<Renderer>();
            if (reticleRenderer != null)
            {
                reticleRenderer.material.color = canPlant ? validPlacementColor : invalidPlacementColor;
            }
        }
        else
        {
            canPlant = false;
        }
    }

    private void UpdatePlantGrowth()
    {
        float currentTime = useRealTimeGrowth ? Time.realtimeSinceStartup : Time.time;

        foreach (var plant in activePlants)
        {
            if (plant.isFullyGrown || plant.isHarvested) continue;

            float elapsedTime = currentTime - plant.plantedTime;
            float growthProgress = Mathf.Clamp01(elapsedTime / plant.growthDuration);

            // Apply growth curve
            float curvedProgress = growthCurve.Evaluate(growthProgress);

            // Update plant scale
            Vector3 targetScale = plant.finalScale * curvedProgress;
            plant.gameObject.transform.localScale = targetScale;

            // Update growth stage
            int newStage = Mathf.FloorToInt(growthProgress * growthStages);
            if (newStage != plant.currentStage && newStage > 0)
            {
                plant.currentStage = newStage;
                OnGrowthStageChanged(plant);
            }

            // Check if fully grown
            if (growthProgress >= 1f && !plant.isFullyGrown)
            {
                plant.isFullyGrown = true;
                OnPlantFullyGrown(plant);
            }

            // Update material emission (glow when ready to harvest)
            if (plant.instanceMaterial != null && plant.isFullyGrown)
            {
                float pulseIntensity = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f;
                PlantData plantData = GetPlantData(plant.type);
                if (plantData != null)
                {
                    Color emissionColor = plantData.plantColor * pulseIntensity * 2f;
                    plant.instanceMaterial.SetColor("_EmissionColor", emissionColor);
                }
            }
        }
    }

    #endregion

    #region Planting

    public void TogglePlantingMode()
    {
        isPlantingMode = !isPlantingMode;

        if (plantingReticle != null)
        {
            plantingReticle.SetActive(isPlantingMode);
        }

        Debug.Log($"Planting mode: {(isPlantingMode ? "ON" : "OFF")}");
    }

    public void TryPlantSeed()
    {
        if (!canPlant) return;

        PlantData plantData = GetPlantData(selectedPlantType);
        if (plantData == null) return;

        // Check if player has enough credits
        if (upgradeSystem != null && upgradeSystem.GetCredits() < plantData.seedCost)
        {
            Debug.Log("Not enough credits to plant seed!");
            return;
        }

        // Check plant limit
        if (activePlants.Count >= maxPlantsPerPlayer)
        {
            Debug.Log("Maximum plant limit reached!");
            return;
        }

        // Check if unlocked
        if (!plantData.unlocked)
        {
            Debug.Log($"{plantData.plantName} is not unlocked yet!");
            return;
        }

        // Deduct seed cost
        if (upgradeSystem != null)
        {
            upgradeSystem.AddCredits(-plantData.seedCost);
        }

        // Plant the seed
        PlantSeed(plantData, plantingPosition);

        OnPlantPlanted?.Invoke(selectedPlantType);
    }

    private void PlantSeed(PlantData plantData, Vector3 position)
    {
        GameObject plantObject;

        if (plantData.plantPrefab != null)
        {
            plantObject = Instantiate(plantData.plantPrefab, position, Quaternion.identity);
        }
        else
        {
            // Create procedural plant
            plantObject = CreateProceduralPlant(plantData);
            plantObject.transform.position = position;
        }

        // Create plant instance
        PlantInstance instance = new PlantInstance
        {
            gameObject = plantObject,
            type = plantData.type,
            plantedTime = useRealTimeGrowth ? Time.realtimeSinceStartup : Time.time,
            growthDuration = plantData.growthTime,
            isFullyGrown = false,
            isHarvested = false,
            currentStage = 0,
            finalScale = plantObject.transform.localScale
        };

        // Start from zero scale
        plantObject.transform.localScale = Vector3.zero;

        // Store material instance for color changes
        Renderer renderer = plantObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            instance.instanceMaterial = renderer.material;
        }

        activePlants.Add(instance);
        OnPlantCountChanged?.Invoke(activePlants.Count);

        // Visual effects
        if (plantEffectPrefab != null)
        {
            GameObject effect = Instantiate(plantEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        PlaySound(plantSound);

        Debug.Log($"Planted {plantData.plantName} at {position}");
    }

    private GameObject CreateProceduralPlant(PlantData plantData)
    {
        GameObject plant = new GameObject($"Plant_{plantData.type}");

        // Create stem
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.transform.SetParent(plant.transform);
        stem.transform.localPosition = Vector3.up * 0.5f;
        stem.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
        stem.GetComponent<Renderer>().material.color = Color.green;

        // Create flower/fruit
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        top.transform.SetParent(plant.transform);
        top.transform.localPosition = Vector3.up * 1.2f;
        top.transform.localScale = Vector3.one * 0.5f;

        Renderer topRenderer = top.GetComponent<Renderer>();
        topRenderer.material.color = plantData.plantColor;

        // Enable emission
        topRenderer.material.EnableKeyword("_EMISSION");
        topRenderer.material.SetColor("_EmissionColor", plantData.plantColor * 0.5f);

        // Add collider for harvesting
        SphereCollider collider = plant.AddComponent<SphereCollider>();
        collider.radius = 0.8f;
        collider.center = Vector3.up * 1.2f;
        collider.isTrigger = true;

        // Tag for identification
        plant.tag = "Plant";

        return plant;
    }

    private bool IsValidPlantingPosition(Vector3 position)
    {
        // Check distance from player
        if (Vector3.Distance(position, playerTransform.position) > plantingRange)
            return false;

        // Check spacing from other plants
        foreach (var plant in activePlants)
        {
            if (plant.isHarvested) continue;

            float distance = Vector3.Distance(position, plant.gameObject.transform.position);
            if (distance < minPlantSpacing)
                return false;
        }

        return true;
    }

    #endregion

    #region Harvesting

    public void HarvestNearbyPlants()
    {
        int harvestedCount = 0;

        for (int i = activePlants.Count - 1; i >= 0; i--)
        {
            PlantInstance plant = activePlants[i];

            if (plant.isHarvested || !plant.isFullyGrown) continue;

            float distance = Vector3.Distance(playerTransform.position, plant.gameObject.transform.position);

            if (distance <= plantingRange)
            {
                HarvestPlant(plant);
                harvestedCount++;
            }
        }

        if (harvestedCount > 0)
        {
            Debug.Log($"Harvested {harvestedCount} plants!");
        }
        else
        {
            Debug.Log("No plants ready to harvest nearby.");
        }
    }

    private void HarvestPlant(PlantInstance plant)
    {
        PlantData plantData = GetPlantData(plant.type);
        if (plantData == null) return;

        plant.isHarvested = true;

        // Grant rewards
        if (rewardSystem != null)
        {
            rewardSystem.AddCredits(plantData.harvestValue);
            rewardSystem.AddXP(plantData.harvestXP);
        }
        else if (upgradeSystem != null)
        {
            upgradeSystem.AddCredits(plantData.harvestValue);
            upgradeSystem.AddXP(plantData.harvestXP);
        }

        // Special plant effects
        ApplyPlantEffect(plantData);

        // Visual effects
        if (harvestEffectPrefab != null)
        {
            GameObject effect = Instantiate(harvestEffectPrefab,
                plant.gameObject.transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        PlaySound(harvestSound);

        // Remove plant
        if (plant.gameObject != null)
        {
            Destroy(plant.gameObject, 0.5f);
        }

        activePlants.Remove(plant);
        OnPlantCountChanged?.Invoke(activePlants.Count);
        OnPlantHarvested?.Invoke(plant.type);

        Debug.Log($"Harvested {plantData.plantName} - Earned {plantData.harvestValue} credits and {plantData.harvestXP} XP");
    }

    private void ApplyPlantEffect(PlantData plantData)
    {
        SpaceshipController ship = FindObjectOfType<SpaceshipController>();
        if (ship == null) return;

        switch (plantData.type)
        {
            case PlantType.EnergyFlower:
                ship.RestoreEnergy(30f);
                Debug.Log("Restored 30 energy!");
                break;

            case PlantType.HealingHerb:
                ship.Heal(50f);
                Debug.Log("Restored 50 health!");
                break;

            case PlantType.CreditFruit:
                // Already handled by harvest value
                break;

            case PlantType.RareCrystal:
                // Bonus effect: restore everything
                ship.RestoreEnergy(50f);
                ship.Heal(30f);
                ship.RestoreFuel(50f);
                Debug.Log("Quantum Crystal restored energy, health, and fuel!");
                break;
        }
    }

    #endregion

    #region Plant Type Selection

    public void SelectNextPlantType()
    {
        int currentIndex = plantTypes.FindIndex(p => p.type == selectedPlantType);
        currentIndex = (currentIndex + 1) % plantTypes.Count;

        // Find next unlocked plant
        for (int i = 0; i < plantTypes.Count; i++)
        {
            if (plantTypes[currentIndex].unlocked)
            {
                selectedPlantType = plantTypes[currentIndex].type;
                Debug.Log($"Selected: {plantTypes[currentIndex].plantName}");
                return;
            }
            currentIndex = (currentIndex + 1) % plantTypes.Count;
        }
    }

    public void SelectPreviousPlantType()
    {
        int currentIndex = plantTypes.FindIndex(p => p.type == selectedPlantType);
        currentIndex = currentIndex - 1;
        if (currentIndex < 0) currentIndex = plantTypes.Count - 1;

        // Find previous unlocked plant
        for (int i = 0; i < plantTypes.Count; i++)
        {
            if (plantTypes[currentIndex].unlocked)
            {
                selectedPlantType = plantTypes[currentIndex].type;
                Debug.Log($"Selected: {plantTypes[currentIndex].plantName}");
                return;
            }
            currentIndex--;
            if (currentIndex < 0) currentIndex = plantTypes.Count - 1;
        }
    }

    public void UnlockPlantType(PlantType type)
    {
        PlantData plantData = GetPlantData(type);
        if (plantData != null)
        {
            plantData.unlocked = true;
            Debug.Log($"Unlocked {plantData.plantName}!");
        }
    }

    #endregion

    #region Growth Events

    private void OnGrowthStageChanged(PlantInstance plant)
    {
        // Play growth sound
        if (growthSound != null && plant.currentStage % 2 == 0)
        {
            PlaySound(growthSound);
        }

        // Spawn growth particles
        if (growthParticles != null)
        {
            ParticleSystem particles = Instantiate(growthParticles,
                plant.gameObject.transform.position, Quaternion.identity);
            Destroy(particles.gameObject, 2f);
        }
    }

    private void OnPlantFullyGrown(PlantInstance plant)
    {
        Debug.Log($"Plant {plant.type} is fully grown and ready to harvest!");

        // Visual indicator (add glow or particle effect)
        if (plant.instanceMaterial != null)
        {
            plant.instanceMaterial.EnableKeyword("_EMISSION");
        }
    }

    #endregion

    #region Helper Methods

    private PlantData GetPlantData(PlantType type)
    {
        return plantDict.ContainsKey(type) ? plantDict[type] : null;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public int GetActivePlantCount() => activePlants.Count;
    public int GetMaxPlants() => maxPlantsPerPlayer;
    public bool IsPlantingMode() => isPlantingMode;
    public PlantType GetSelectedPlantType() => selectedPlantType;

    public List<PlantData> GetAllPlantTypes()
    {
        return new List<PlantData>(plantTypes);
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw planting range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerTransform != null ? playerTransform.position : transform.position, plantingRange);

        // Draw plant spacing
        Gizmos.color = Color.yellow;
        foreach (var plant in activePlants)
        {
            if (plant.gameObject != null)
            {
                Gizmos.DrawWireSphere(plant.gameObject.transform.position, minPlantSpacing);
            }
        }
    }

    #endregion
}

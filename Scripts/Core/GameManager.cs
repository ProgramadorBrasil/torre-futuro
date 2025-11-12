using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceRPG.Core
{
    /// <summary>
    /// Central Game Manager coordinating all systems, game states, enemy spawning, and level progression
    /// Acts as the main hub connecting SpaceshipController, WeaponSystem, UpgradeSystem, RewardSystem, etc.
    /// </summary>
    public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    #region Game States
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

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    [SerializeField] private bool autoStartGame = true;
    #endregion

    #region System References
    [Header("Core Systems")]
    [SerializeField] private SpaceshipController playerShip;
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private UpgradeSystem upgradeSystem;
    [SerializeField] private RewardSystem rewardSystem;
    [SerializeField] private PlantingSystem plantingSystem;
    [SerializeField] private NPCInstructor npcInstructor;
    [SerializeField] private GameplayUI gameplayUI;
    #endregion

    #region Enemy Spawning
    [Header("Enemy Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxEnemiesAtOnce = 10;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float spawnDistanceFromPlayer = 50f;
    [SerializeField] private bool enableDynamicSpawning = true;
    #endregion

    #region Wave System
    [Header("Wave System")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private int enemiesRemainingInWave = 0;
    [SerializeField] private float timeBetweenWaves = 15f;
    [SerializeField] private float waveMultiplier = 1.2f; // Difficulty increase per wave
    [SerializeField] private bool isWaveActive = false;
    #endregion

    #region Mission System
    [Header("Mission System")]
    [SerializeField] private string currentMissionObjective = "Destroy all enemies";
    [SerializeField] private int missionProgress = 0;
    [SerializeField] private int missionTarget = 10;
    [SerializeField] private bool missionActive = false;
    #endregion

    #region Checkpoints & Save
    [Header("Checkpoints")]
    [SerializeField] private Vector3 lastCheckpointPosition;
    [SerializeField] private Quaternion lastCheckpointRotation;
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 60f; // Every minute
    #endregion

    #region Difficulty
    [Header("Difficulty Settings")]
    [SerializeField] private float difficultyMultiplier = 1f;
    [SerializeField] private bool adaptiveDifficulty = true;
    [SerializeField] private float playerPerformanceRating = 0.5f; // 0-1 scale
    #endregion

    #region Audio
    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverMusic;
    #endregion

    // Private variables
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float nextSpawnTime = 0f;
    private float nextAutoSaveTime = 0f;
    private int totalEnemiesKilled = 0;
    private bool gameStarted = false;

    // Events
    public delegate void GameStateEvent(GameState newState);
    public delegate void WaveEvent(int waveNumber);
    public delegate void MissionEvent(string objective, int progress, int target);
    public event GameStateEvent OnGameStateChanged;
    public event WaveEvent OnWaveStarted;
    public event WaveEvent OnWaveCompleted;
    public event MissionEvent OnMissionProgressUpdated;
    public event System.Action OnGameOver;
    public event System.Action OnVictory;

    #region Initialization

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        FindAllReferences();
        InitializeSystems();
    }

    private void Start()
    {
        if (autoStartGame)
        {
            StartCoroutine(DelayedGameStart());
        }
    }

    private void FindAllReferences()
    {
        // Find player
        if (playerShip == null)
        {
            playerShip = FindObjectOfType<SpaceshipController>();
        }

        // Find systems
        if (weaponSystem == null)
            weaponSystem = FindObjectOfType<WeaponSystem>();

        if (upgradeSystem == null)
            upgradeSystem = FindObjectOfType<UpgradeSystem>();

        if (rewardSystem == null)
            rewardSystem = FindObjectOfType<RewardSystem>();

        if (plantingSystem == null)
            plantingSystem = FindObjectOfType<PlantingSystem>();

        if (npcInstructor == null)
            npcInstructor = FindObjectOfType<NPCInstructor>();

        if (gameplayUI == null)
            gameplayUI = FindObjectOfType<GameplayUI>();

        // Find or create music source
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f;
        }
    }

    private void InitializeSystems()
    {
        // Set initial checkpoint
        if (playerShip != null)
        {
            lastCheckpointPosition = playerShip.transform.position;
            lastCheckpointRotation = playerShip.transform.rotation;
        }

        // Find spawn points if not assigned
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
            spawnPoints = new Transform[spawnPointObjects.Length];
            for (int i = 0; i < spawnPointObjects.Length; i++)
            {
                spawnPoints[i] = spawnPointObjects[i].transform;
            }
        }

        // Subscribe to player death
        if (playerShip != null)
        {
            playerShip.OnSpaceshipDestroyed += OnPlayerDied;
        }
    }

    private IEnumerator DelayedGameStart()
    {
        yield return new WaitForSeconds(1f);
        StartGame();
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        if (currentState != GameState.Gameplay) return;

        UpdateEnemySpawning();
        UpdateAutoSave();
        UpdateAdaptiveDifficulty();
        CleanupDeadEnemies();
    }

    #endregion

    #region Game State Management

    public void StartGame()
    {
        SetGameState(GameState.Gameplay);
        gameStarted = true;

        // Start first wave
        StartWave(1);

        // Start background music
        PlayMusic(gameplayMusic);

        // Give starting tutorial if needed
        if (npcInstructor != null && !npcInstructor.IsTutorialCompleted())
        {
            SetGameState(GameState.Tutorial);
        }

        Debug.Log("Game Started!");
    }

    public void SetGameState(GameState newState)
    {
        GameState previousState = currentState;
        currentState = newState;

        OnGameStateChanged?.Invoke(newState);

        // Handle state transitions
        switch (newState)
        {
            case GameState.Gameplay:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.GameOver:
                HandleGameOver();
                break;

            case GameState.Victory:
                HandleVictory();
                break;
        }

        Debug.Log($"Game state changed: {previousState} -> {newState}");
    }

    #endregion

    #region Enemy Spawning

    private void UpdateEnemySpawning()
    {
        if (!enableDynamicSpawning || !isWaveActive) return;

        if (Time.time >= nextSpawnTime && activeEnemies.Count < maxEnemiesAtOnce)
        {
            if (enemiesRemainingInWave > 0)
            {
                SpawnEnemy();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned!");
            return;
        }

        // Select random enemy type
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Get spawn position
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Scale enemy stats based on wave
        ScaleEnemyDifficulty(enemy);

        activeEnemies.Add(enemy);
        enemiesRemainingInWave--;

        Debug.Log($"Enemy spawned! Remaining in wave: {enemiesRemainingInWave}");
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Try to use spawn points first
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position;
        }

        // Otherwise, spawn around player at random position
        if (playerShip != null)
        {
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            randomDirection.y = Random.Range(-10f, 10f); // Some vertical variation
            return playerShip.transform.position + randomDirection * spawnDistanceFromPlayer;
        }

        return Vector3.zero;
    }

    private void ScaleEnemyDifficulty(GameObject enemy)
    {
        // Apply wave multiplier to enemy stats
        float scaleFactor = Mathf.Pow(waveMultiplier, currentWave - 1) * difficultyMultiplier;

        // This assumes enemies have a standardized component
        // You would need to implement EnemyController with these properties
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.ScaleStats(scaleFactor);
        }
    }

    private void CleanupDeadEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);

        // Check if wave is complete
        if (isWaveActive && enemiesRemainingInWave <= 0 && activeEnemies.Count == 0)
        {
            CompleteWave();
        }
    }

    public void RegisterEnemyKilled(GameObject enemy)
    {
        totalEnemiesKilled++;
        missionProgress++;

        // Reward player
        if (rewardSystem != null)
        {
            rewardSystem.RegisterKill(RewardSystem.KillType.Standard);
        }

        // Update mission progress
        OnMissionProgressUpdated?.Invoke(currentMissionObjective, missionProgress, missionTarget);

        // Remove from active list
        activeEnemies.Remove(enemy);

        // Check mission completion
        if (missionActive && missionProgress >= missionTarget)
        {
            CompleteMission();
        }
    }

    #endregion

    #region Wave System

    public void StartWave(int waveNumber)
    {
        currentWave = waveNumber;
        isWaveActive = true;

        // Calculate enemies for this wave
        enemiesRemainingInWave = Mathf.RoundToInt(enemiesPerWave * Mathf.Pow(waveMultiplier, waveNumber - 1));

        OnWaveStarted?.Invoke(currentWave);

        if (gameplayUI != null)
        {
            gameplayUI.ShowNotification($"WAVE {currentWave}", $"Enemies: {enemiesRemainingInWave}");
        }

        Debug.Log($"Wave {currentWave} started! Enemies to spawn: {enemiesRemainingInWave}");
    }

    private void CompleteWave()
    {
        isWaveActive = false;

        OnWaveCompleted?.Invoke(currentWave);

        // Grant wave completion bonus
        if (rewardSystem != null)
        {
            int bonusCredits = currentWave * 100;
            int bonusXP = currentWave * 50;
            rewardSystem.AddCredits(bonusCredits);
            rewardSystem.AddXP(bonusXP);
        }

        if (gameplayUI != null)
        {
            gameplayUI.ShowNotification($"WAVE {currentWave} COMPLETE!", "Next wave incoming...");
        }

        Debug.Log($"Wave {currentWave} completed!");

        // Start next wave after delay
        StartCoroutine(StartNextWaveAfterDelay());
    }

    private IEnumerator StartNextWaveAfterDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        if (currentState == GameState.Gameplay)
        {
            StartWave(currentWave + 1);
        }
    }

    #endregion

    #region Mission System

    public void StartMission(string objective, int targetValue)
    {
        missionActive = true;
        currentMissionObjective = objective;
        missionTarget = targetValue;
        missionProgress = 0;

        OnMissionProgressUpdated?.Invoke(currentMissionObjective, missionProgress, missionTarget);

        if (gameplayUI != null)
        {
            gameplayUI.ShowNotification("NEW MISSION", objective);
        }

        Debug.Log($"Mission started: {objective} ({missionProgress}/{missionTarget})");
    }

    private void CompleteMission()
    {
        missionActive = false;

        // Grant mission rewards
        if (rewardSystem != null)
        {
            rewardSystem.CompleteMission(currentWave); // Bonus based on wave
        }

        if (gameplayUI != null)
        {
            gameplayUI.ShowNotification("MISSION COMPLETE!", "Objective achieved!");
        }

        Debug.Log("Mission completed!");
    }

    #endregion

    #region Checkpoint & Save System

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        lastCheckpointPosition = position;
        lastCheckpointRotation = rotation;

        if (gameplayUI != null)
        {
            gameplayUI.ShowNotification("CHECKPOINT", "Progress saved");
        }

        Debug.Log("Checkpoint saved!");
    }

    public void RespawnAtCheckpoint()
    {
        if (playerShip != null)
        {
            playerShip.transform.position = lastCheckpointPosition;
            playerShip.transform.rotation = lastCheckpointRotation;

            // Reset player state
            playerShip.Heal(100f);
            playerShip.RestoreEnergy(100f);
            playerShip.RestoreFuel(100f);
            playerShip.RepairArmor(100f);
        }

        Debug.Log("Respawned at checkpoint");
    }

    private void UpdateAutoSave()
    {
        if (!autoSaveEnabled) return;

        if (Time.time >= nextAutoSaveTime)
        {
            AutoSave();
            nextAutoSaveTime = Time.time + autoSaveInterval;
        }
    }

    private void AutoSave()
    {
        // Save current checkpoint
        if (playerShip != null)
        {
            SetCheckpoint(playerShip.transform.position, playerShip.transform.rotation);
        }

        // Systems auto-save themselves via their own mechanisms
        Debug.Log("Auto-save completed");
    }

    #endregion

    #region Adaptive Difficulty

    private void UpdateAdaptiveDifficulty()
    {
        if (!adaptiveDifficulty) return;

        // Calculate player performance
        float kdr = rewardSystem != null ? rewardSystem.GetKDRatio() : 1f;
        int streak = rewardSystem != null ? rewardSystem.GetCurrentStreak() : 0;

        // Adjust performance rating
        if (kdr > 2f && streak > 5)
        {
            playerPerformanceRating = Mathf.Min(1f, playerPerformanceRating + 0.01f * Time.deltaTime);
        }
        else if (kdr < 0.5f)
        {
            playerPerformanceRating = Mathf.Max(0f, playerPerformanceRating - 0.01f * Time.deltaTime);
        }

        // Adjust difficulty multiplier
        difficultyMultiplier = 0.5f + (playerPerformanceRating * 1.5f);

        // Update reward system difficulty
        if (rewardSystem != null)
        {
            rewardSystem.SetDifficultyMultiplier(difficultyMultiplier);
        }
    }

    #endregion

    #region Game Over & Victory

    private void OnPlayerDied()
    {
        if (rewardSystem != null)
        {
            rewardSystem.RegisterDeath();
        }

        // Option 1: Respawn
        StartCoroutine(RespawnPlayer());

        // Option 2: Game Over (uncomment to use)
        // SetGameState(GameState.GameOver);
    }

    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(3f);

        if (currentState == GameState.Gameplay)
        {
            RespawnAtCheckpoint();
        }
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0f;
        PlayMusic(gameOverMusic);

        OnGameOver?.Invoke();

        if (gameplayUI != null)
        {
            gameplayUI.ShowNotification("GAME OVER", "Press R to restart");
        }

        Debug.Log("Game Over!");
    }

    private void HandleVictory()
    {
        Time.timeScale = 0f;
        PlayMusic(victoryMusic);

        OnVictory?.Invoke();

        // Grant victory bonus
        if (rewardSystem != null)
        {
            rewardSystem.AddCredits(5000);
            rewardSystem.AddXP(2000);
        }

        if (gameplayUI != null)
        {
            gameplayUI.ShowNotification("VICTORY!", "All objectives completed!");
        }

        Debug.Log("Victory achieved!");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Clear enemies
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();

        // Reset systems
        currentWave = 0;
        missionProgress = 0;
        totalEnemiesKilled = 0;

        // Respawn player
        RespawnAtCheckpoint();

        // Restart game
        StartGame();
    }

    #endregion

    #region Audio

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    #endregion

    #region Public Getters

    public GameState GetCurrentState() => currentState;
    public int GetCurrentWave() => currentWave;
    public int GetTotalEnemiesKilled() => totalEnemiesKilled;
    public int GetActiveEnemyCount() => activeEnemies.Count;
    public bool IsMissionActive() => missionActive;
    public float GetDifficultyMultiplier() => difficultyMultiplier;

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 2f);
                }
            }
        }

        // Draw checkpoint
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(lastCheckpointPosition, Vector3.one * 2f);

        // Draw active enemies
        Gizmos.color = Color.yellow;
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawLine(lastCheckpointPosition, enemy.transform.position);
            }
        }
    }

    [ContextMenu("Start Wave 1")]
    public void DebugStartWave1() => StartWave(1);

    [ContextMenu("Complete Current Wave")]
    public void DebugCompleteWave() => CompleteWave();

    [ContextMenu("Spawn Enemy")]
    public void DebugSpawnEnemy() => SpawnEnemy();

    #endregion
    }
}

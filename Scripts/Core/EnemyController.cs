using UnityEngine;
using SpaceRPG.Systems;

namespace SpaceRPG.Core
{
    /// <summary>
    /// Controlador básico para inimigos no jogo
    /// Gerencia stats, dificuldade e comportamento base
    /// </summary>
    public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float baseHealth = 100f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float baseArmor = 0f;

    [Header("Current Stats")]
    private float currentHealth;
    private float currentDamage;
    private float currentSpeed;
    private float currentArmor;

    [Header("Difficulty")]
    private float difficultyMultiplier = 1f;

    [Header("Rewards")]
    [SerializeField] private int creditsOnKill = 50;
    [SerializeField] private int xpOnKill = 25;

    // Events
    public System.Action<EnemyController> OnEnemyDeath;
    public System.Action<float> OnEnemyDamaged;

    private void Awake()
    {
        InitializeStats();
    }

    private void Start()
    {
        ApplyDifficulty();
    }

    /// <summary>
    /// Inicializa os stats atuais com os valores base
    /// </summary>
    private void InitializeStats()
    {
        currentHealth = baseHealth;
        currentDamage = baseDamage;
        currentSpeed = baseSpeed;
        currentArmor = baseArmor;
    }

    /// <summary>
    /// Define o multiplicador de dificuldade
    /// </summary>
    public void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = multiplier;
        ApplyDifficulty();
    }

    /// <summary>
    /// Escala os stats baseado no multiplicador (para waves)
    /// </summary>
    public void ScaleStats(float scaleFactor)
    {
        currentHealth = baseHealth * scaleFactor;
        currentDamage = baseDamage * scaleFactor;
        currentArmor = baseArmor * scaleFactor;
        // Speed normalmente não escala tanto
        currentSpeed = baseSpeed * Mathf.Sqrt(scaleFactor);
    }

    /// <summary>
    /// Aplica o multiplicador de dificuldade aos stats
    /// </summary>
    private void ApplyDifficulty()
    {
        if (difficultyMultiplier != 1f)
        {
            currentHealth *= difficultyMultiplier;
            currentDamage *= difficultyMultiplier;
            currentArmor *= difficultyMultiplier;
        }
    }

    /// <summary>
    /// Aplica dano ao inimigo
    /// </summary>
    public void TakeDamage(float damage)
    {
        // Aplicar armadura (reduz dano)
        float actualDamage = Mathf.Max(damage - currentArmor, damage * 0.1f); // Mínimo 10% do dano

        currentHealth -= actualDamage;

        OnEnemyDamaged?.Invoke(actualDamage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Cura o inimigo
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, GetMaxHealth());
    }

    /// <summary>
    /// Mata o inimigo
    /// </summary>
    private void Die()
    {
        OnEnemyDeath?.Invoke(this);

        // Dar recompensas ao jogador
        if (RewardSystem.Instance != null)
        {
            RewardSystem.Instance.AddCredits(Mathf.RoundToInt(creditsOnKill * difficultyMultiplier));
            RewardSystem.Instance.AddXP(Mathf.RoundToInt(xpOnKill * difficultyMultiplier));
        }

        // Notificar GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemyKilled(gameObject);
        }

        // Destruir inimigo
        Destroy(gameObject);
    }

    // Getters
    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => baseHealth * difficultyMultiplier;
    public float GetHealthPercent() => currentHealth / GetMaxHealth();
    public float GetDamage() => currentDamage;
    public float GetSpeed() => currentSpeed;
    public float GetArmor() => currentArmor;
    public float GetDifficultyMultiplier() => difficultyMultiplier;

    // Debug
    private void OnDrawGizmos()
    {
        // Desenhar barra de vida acima do inimigo
        if (Application.isPlaying)
        {
            Vector3 position = transform.position + Vector3.up * 2f;
            float healthPercent = GetHealthPercent();

            Gizmos.color = Color.Lerp(Color.red, Color.green, healthPercent);
            Gizmos.DrawWireCube(position, new Vector3(healthPercent * 2f, 0.2f, 0.1f));
        }
    }

    /// <summary>
    /// Sistema de spawn de inimigos
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private int maxEnemiesAtOnce = 5;
        [SerializeField] private float spawnRadius = 10f;

        [Header("Difficulty")]
        private float spawnRateMultiplier = 1f;

        private float nextSpawnTime;
        private int currentEnemyCount = 0;

        private void Update()
        {
            if (Time.time >= nextSpawnTime && currentEnemyCount < maxEnemiesAtOnce)
            {
                SpawnEnemy();
                nextSpawnTime = Time.time + (spawnInterval / spawnRateMultiplier);
            }
        }

        /// <summary>
        /// Spawna um inimigo aleatório
        /// </summary>
        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.y = transform.position.y; // Manter mesma altura

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            var controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.OnEnemyDeath += OnEnemyKilled;
            }

            currentEnemyCount++;
        }

        /// <summary>
        /// Callback quando inimigo morre
        /// </summary>
        private void OnEnemyKilled(EnemyController enemy)
        {
            currentEnemyCount--;
        }

        /// <summary>
        /// Define o multiplicador de taxa de spawn
        /// </summary>
        public void SetSpawnRateMultiplier(float multiplier)
        {
            spawnRateMultiplier = multiplier;
        }

        /// <summary>
        /// Para de spawnar inimigos
        /// </summary>
        public void StopSpawning()
        {
            enabled = false;
        }

        /// <summary>
        /// Retoma spawn de inimigos
        /// </summary>
        public void ResumeSpawning()
        {
            enabled = true;
            nextSpawnTime = Time.time + spawnInterval;
        }

        private void OnDrawGizmosSelected()
        {
            // Desenhar área de spawn
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
}

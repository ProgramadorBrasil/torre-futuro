using UnityEngine;
using System.Collections;

namespace SpaceRPG.Core
{
    /// <summary>
    /// Advanced Spaceship Controller with realistic physics, WASD+Mouse controls, and energy management
    /// Supports 360-degree movement with smooth acceleration and deceleration
    /// </summary>
    public class SpaceshipController : MonoBehaviour
    {
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float turnSpeed = 100f;
    [SerializeField] private float rollSpeed = 80f;
    [SerializeField] private float pitchSpeed = 60f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Physics Settings")]
    [SerializeField] private float drag = 0.5f;
    [SerializeField] private float angularDrag = 5f;
    [SerializeField] private bool useGravity = false;

    [Header("Health & Energy")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float energyRegenRate = 5f;
    [SerializeField] private float energyDrainPerSecond = 2f;

    [Header("Armor & Shields")]
    [SerializeField] private float maxArmor = 50f;
    [SerializeField] private float currentArmor = 50f;
    [SerializeField] private float armorDamageReduction = 0.3f; // 30% damage reduction

    [Header("Fuel System")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float currentFuel = 100f;
    [SerializeField] private float fuelConsumptionRate = 1f;
    [SerializeField] private bool unlimitedFuel = false;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraHeight = 3f;
    [SerializeField] private float cameraSmoothing = 5f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem[] engineThrusters;
    [SerializeField] private TrailRenderer[] wingTrails;
    [SerializeField] private Light[] engineLights;
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private GameObject explosionPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private AudioSource thrusterSound;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip lowEnergyClip;

    [Header("Input Settings")]
    [SerializeField] private KeyCode boostKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode brakeKey = KeyCode.LeftControl;
    [SerializeField] private float boostMultiplier = 2f;

    // Private variables
    private Rigidbody rb;
    private Vector3 currentVelocity;
    private float currentSpeed;
    private bool isAlive = true;
    private bool isBoosting = false;
    private bool isBraking = false;
    private float pitchInput;
    private float yawInput;
    private float rollInput;
    private Camera mainCamera;

    // Upgrade multipliers (modified by UpgradeSystem)
    [HideInInspector] public float speedUpgradeMultiplier = 1f;
    [HideInInspector] public float healthUpgradeMultiplier = 1f;
    [HideInInspector] public float armorUpgradeMultiplier = 1f;
    [HideInInspector] public float energyUpgradeMultiplier = 1f;

    // Events
    public delegate void SpaceshipEvent(float value);
    public event SpaceshipEvent OnHealthChanged;
    public event SpaceshipEvent OnEnergyChanged;
    public event SpaceshipEvent OnFuelChanged;
    public event SpaceshipEvent OnArmorChanged;
    public event System.Action OnSpaceshipDestroyed;

    #region Initialization

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        ConfigureRigidbody();
        mainCamera = Camera.main;

        // Initialize values
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentFuel = maxFuel;
        currentArmor = maxArmor;
    }

    private void Start()
    {
        InitializeEffects();
        if (engineSound != null)
        {
            engineSound.Play();
        }
    }

    private void ConfigureRigidbody()
    {
        rb.useGravity = useGravity;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void InitializeEffects()
    {
        if (engineThrusters != null)
        {
            foreach (var thruster in engineThrusters)
            {
                if (thruster != null)
                {
                    thruster.Play();
                }
            }
        }

        if (wingTrails != null)
        {
            foreach (var trail in wingTrails)
            {
                if (trail != null)
                {
                    trail.emitting = false;
                }
            }
        }
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        if (!isAlive) return;

        HandleInput();
        UpdateEnergy();
        UpdateFuel();
        UpdateVisuals();
        UpdateAudio();
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;

        ApplyMovement();
        ApplyRotation();
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        // Movement input
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // Mouse input for pitch and yaw
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Roll input (Q/E keys)
        float roll = 0f;
        if (Input.GetKey(KeyCode.Q)) roll = -1f;
        if (Input.GetKey(KeyCode.E)) roll = 1f;

        // Store inputs
        yawInput = horizontal + mouseX;
        pitchInput = vertical - mouseY; // Inverted for natural feel
        rollInput = roll;

        // Boost and brake
        isBoosting = Input.GetKey(boostKey) && currentEnergy > 10f;
        isBraking = Input.GetKey(brakeKey);

        // Right-click for alternative control mode
        if (Input.GetMouseButton(1))
        {
            yawInput = mouseX;
            pitchInput = -mouseY;
        }
    }

    #endregion

    #region Movement & Physics

    private void ApplyMovement()
    {
        // Calculate target speed
        float targetSpeed = maxSpeed * speedUpgradeMultiplier;

        if (isBoosting)
        {
            targetSpeed *= boostMultiplier;
            ConsumeEnergy(10f * Time.fixedDeltaTime);
        }

        if (isBraking)
        {
            targetSpeed *= 0.3f;
        }

        // Check fuel
        if (!unlimitedFuel && currentFuel <= 0f)
        {
            targetSpeed *= 0.2f; // Emergency speed with no fuel
        }

        // Forward thrust
        float thrust = Input.GetKey(KeyCode.W) ? 1f : 0f;

        if (thrust > 0f)
        {
            Vector3 forwardForce = transform.forward * acceleration * thrust;
            rb.AddForce(forwardForce, ForceMode.Acceleration);
        }
        else
        {
            // Apply deceleration when no thrust
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        // Clamp velocity to max speed
        if (rb.velocity.magnitude > targetSpeed)
        {
            rb.velocity = rb.velocity.normalized * targetSpeed;
        }

        currentSpeed = rb.velocity.magnitude;
        currentVelocity = rb.velocity;
    }

    private void ApplyRotation()
    {
        // Apply yaw (left/right)
        if (Mathf.Abs(yawInput) > 0.01f)
        {
            Quaternion yawRotation = Quaternion.Euler(0, yawInput * turnSpeed * Time.fixedDeltaTime, 0);
            rb.MoveRotation(rb.rotation * yawRotation);
        }

        // Apply pitch (up/down)
        if (Mathf.Abs(pitchInput) > 0.01f)
        {
            Quaternion pitchRotation = Quaternion.Euler(pitchInput * pitchSpeed * Time.fixedDeltaTime, 0, 0);
            rb.MoveRotation(rb.rotation * pitchRotation);
        }

        // Apply roll (Q/E)
        if (Mathf.Abs(rollInput) > 0.01f)
        {
            Quaternion rollRotation = Quaternion.Euler(0, 0, rollInput * rollSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * rollRotation);
        }

        // Bank on turns (automatic roll based on yaw)
        float bankAngle = -yawInput * 30f;
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 0, bankAngle * Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 2f));
    }

    #endregion

    #region Energy & Fuel Management

    private void UpdateEnergy()
    {
        // Energy regeneration
        if (currentEnergy < maxEnergy * energyUpgradeMultiplier)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy * energyUpgradeMultiplier);
            OnEnergyChanged?.Invoke(GetEnergyPercentage());
        }

        // Passive energy drain when moving
        if (currentSpeed > 0.1f)
        {
            ConsumeEnergy(energyDrainPerSecond * Time.deltaTime);
        }
    }

    private void UpdateFuel()
    {
        if (unlimitedFuel) return;

        // Consume fuel when moving
        if (currentSpeed > 0.1f && currentFuel > 0f)
        {
            float consumption = fuelConsumptionRate * Time.deltaTime;
            if (isBoosting)
            {
                consumption *= boostMultiplier;
            }

            currentFuel -= consumption;
            currentFuel = Mathf.Max(currentFuel, 0f);
            OnFuelChanged?.Invoke(GetFuelPercentage());

            // Warning when low on fuel
            if (currentFuel <= 10f && currentFuel > 0f && Time.frameCount % 60 == 0)
            {
                PlaySound(lowEnergyClip);
            }
        }
    }

    public void ConsumeEnergy(float amount)
    {
        currentEnergy -= amount;
        currentEnergy = Mathf.Max(currentEnergy, 0f);
        OnEnergyChanged?.Invoke(GetEnergyPercentage());
    }

    public void RestoreFuel(float amount)
    {
        currentFuel += amount;
        currentFuel = Mathf.Min(currentFuel, maxFuel);
        OnFuelChanged?.Invoke(GetFuelPercentage());
    }

    public void RestoreEnergy(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy * energyUpgradeMultiplier);
        OnEnergyChanged?.Invoke(GetEnergyPercentage());
    }

    #endregion

    #region Health & Damage

    public void TakeDamage(float damage)
    {
        if (!isAlive) return;

        // Armor absorbs damage first
        if (currentArmor > 0f)
        {
            float reducedDamage = damage * (1f - armorDamageReduction);
            float armorDamage = reducedDamage * 0.5f; // Half damage goes to armor
            float healthDamage = reducedDamage * 0.5f;

            currentArmor -= armorDamage;
            if (currentArmor < 0f)
            {
                healthDamage += Mathf.Abs(currentArmor); // Overflow damage to health
                currentArmor = 0f;
            }

            currentHealth -= healthDamage;
            OnArmorChanged?.Invoke(GetArmorPercentage());
        }
        else
        {
            // No armor, full damage to health
            currentHealth -= damage;
        }

        currentHealth = Mathf.Max(currentHealth, 0f);
        OnHealthChanged?.Invoke(GetHealthPercentage());

        // Visual/audio feedback
        PlaySound(damageClip);
        SpawnDamageEffect();

        // Check for death
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth * healthUpgradeMultiplier);
        OnHealthChanged?.Invoke(GetHealthPercentage());
    }

    public void RepairArmor(float amount)
    {
        currentArmor += amount;
        currentArmor = Mathf.Min(currentArmor, maxArmor * armorUpgradeMultiplier);
        OnArmorChanged?.Invoke(GetArmorPercentage());
    }

    private void Die()
    {
        isAlive = false;
        OnSpaceshipDestroyed?.Invoke();

        // Spawn explosion
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        PlaySound(explosionClip);

        // Disable components
        StopAllEffects();

        // Optional: Destroy after delay or trigger respawn
        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(3f);

        // Reset values
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentFuel = maxFuel;
        currentArmor = maxArmor;
        isAlive = true;

        // Reset position (find spawn point or use default)
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        InitializeEffects();

        OnHealthChanged?.Invoke(GetHealthPercentage());
        OnEnergyChanged?.Invoke(GetEnergyPercentage());
        OnFuelChanged?.Invoke(GetFuelPercentage());
        OnArmorChanged?.Invoke(GetArmorPercentage());
    }

    #endregion

    #region Visuals & Audio

    private void UpdateVisuals()
    {
        // Update thruster intensity based on speed
        float thrusterIntensity = currentSpeed / (maxSpeed * speedUpgradeMultiplier);

        if (engineThrusters != null)
        {
            foreach (var thruster in engineThrusters)
            {
                if (thruster != null)
                {
                    var emission = thruster.emission;
                    emission.rateOverTime = 50f + (thrusterIntensity * 200f);
                }
            }
        }

        // Enable trails when boosting
        if (wingTrails != null)
        {
            foreach (var trail in wingTrails)
            {
                if (trail != null)
                {
                    trail.emitting = isBoosting;
                }
            }
        }

        // Engine lights intensity
        if (engineLights != null)
        {
            foreach (var light in engineLights)
            {
                if (light != null)
                {
                    light.intensity = 1f + (thrusterIntensity * 3f);
                }
            }
        }
    }

    private void UpdateAudio()
    {
        if (engineSound != null)
        {
            float pitchVariation = 0.8f + (currentSpeed / (maxSpeed * speedUpgradeMultiplier)) * 0.7f;
            engineSound.pitch = pitchVariation;
            engineSound.volume = 0.3f + (currentSpeed / (maxSpeed * speedUpgradeMultiplier)) * 0.4f;
        }

        if (thrusterSound != null)
        {
            if (isBoosting && !thrusterSound.isPlaying)
            {
                thrusterSound.Play();
            }
            else if (!isBoosting && thrusterSound.isPlaying)
            {
                thrusterSound.Stop();
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }

    private void SpawnDamageEffect()
    {
        if (damageEffectPrefab != null)
        {
            GameObject effect = Instantiate(damageEffectPrefab, transform.position, Quaternion.identity, transform);
            Destroy(effect, 2f);
        }
    }

    private void StopAllEffects()
    {
        if (engineThrusters != null)
        {
            foreach (var thruster in engineThrusters)
            {
                if (thruster != null) thruster.Stop();
            }
        }

        if (wingTrails != null)
        {
            foreach (var trail in wingTrails)
            {
                if (trail != null) trail.emitting = false;
            }
        }

        if (engineLights != null)
        {
            foreach (var light in engineLights)
            {
                if (light != null) light.enabled = false;
            }
        }

        if (engineSound != null) engineSound.Stop();
        if (thrusterSound != null) thrusterSound.Stop();
    }

    #endregion

    #region Collision Handling

    private void OnCollisionEnter(Collision collision)
    {
        if (!isAlive) return;

        // Calculate damage based on collision impact
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce > 5f)
        {
            float collisionDamage = (impactForce - 5f) * 2f;
            TakeDamage(collisionDamage);
        }

        // Check for specific collision types
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(20f);
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            TakeDamage(15f);
        }
    }

    #endregion

    #region Public Getters

    public float GetHealthPercentage() => currentHealth / (maxHealth * healthUpgradeMultiplier);
    public float GetEnergyPercentage() => currentEnergy / (maxEnergy * energyUpgradeMultiplier);
    public float GetFuelPercentage() => currentFuel / maxFuel;
    public float GetArmorPercentage() => currentArmor / (maxArmor * armorUpgradeMultiplier);
    public float GetCurrentSpeed() => currentSpeed;
    public bool IsAlive() => isAlive;
    public Vector3 GetVelocity() => currentVelocity;
    public bool HasEnergy(float amount) => currentEnergy >= amount;
    public bool HasFuel(float amount) => currentFuel >= amount || unlimitedFuel;

    #endregion

    #region Upgrade Application

    public void ApplyUpgrades(float speedMult, float healthMult, float armorMult, float energyMult)
    {
        speedUpgradeMultiplier = speedMult;
        healthUpgradeMultiplier = healthMult;
        armorUpgradeMultiplier = armorMult;
        energyUpgradeMultiplier = energyMult;

        // Update max values
        currentHealth = Mathf.Min(currentHealth, maxHealth * healthUpgradeMultiplier);
        currentArmor = Mathf.Min(currentArmor, maxArmor * armorUpgradeMultiplier);
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy * energyUpgradeMultiplier);
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw velocity vector
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, currentVelocity);

        // Draw forward direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 5f);
    }

    #endregion
}
}

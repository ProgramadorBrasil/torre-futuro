using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceRPG.Systems
{
    /// <summary>
    /// Advanced Weapon System supporting 3 weapon types: Laser, Missile, and Plasma
    /// Includes ammo management, fire rate control, visual/audio effects, and upgrade support
    /// </summary>
    public class WeaponSystem : MonoBehaviour
{
    #region Weapon Types Enum
    public enum WeaponType
    {
        Laser,      // Fast, low damage, high fire rate
        Missile,    // Slow, high damage, guided
        Plasma      // Medium speed, splash damage
    }
    #endregion

    #region Weapon Configuration
    [System.Serializable]
    public class WeaponConfig
    {
        public WeaponType type;
        public string weaponName;
        public GameObject projectilePrefab;
        public float damage;
        public float projectileSpeed;
        public float fireRate; // Shots per second
        public int maxAmmo;
        public int currentAmmo;
        public float reloadTime;
        public float energyCost;
        public float range;
        public bool unlocked;

        // Visual Effects
        public GameObject muzzleFlashPrefab;
        public ParticleSystem muzzleParticles;
        public Light muzzleLight;
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip emptySound;

        // Upgrade multipliers
        public float damageUpgradeMultiplier = 1f;
        public float fireRateUpgradeMultiplier = 1f;
        public float ammoUpgradeMultiplier = 1f;
    }
    #endregion

    [Header("Weapon Configurations")]
    [SerializeField] private List<WeaponConfig> weaponConfigs = new List<WeaponConfig>();
    [SerializeField] private WeaponType currentWeaponType = WeaponType.Laser;
    [SerializeField] private int currentWeaponIndex = 0;

    [Header("Weapon Mount Points")]
    [SerializeField] private Transform[] weaponMountPoints;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Targeting System")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float autoAimAssist = 0.2f;
    [SerializeField] private float targetingRange = 100f;
    [SerializeField] private bool autoTargeting = false;

    [Header("Heat Management")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float currentHeat = 0f;
    [SerializeField] private float heatPerShot = 5f;
    [SerializeField] private float heatDissipationRate = 10f;
    [SerializeField] private float overheatingThreshold = 90f;
    [SerializeField] private bool isOverheated = false;

    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private GameObject plasmaImpactPrefab;
    [SerializeField] private TrailRenderer projectileTrail;

    [Header("Audio")]
    [SerializeField] private AudioSource weaponAudioSource;
    [SerializeField] private AudioClip weaponSwitchSound;
    [SerializeField] private AudioClip overheatingSound;

    [Header("Ammo Display")]
    [SerializeField] private bool infiniteAmmo = false;

    // Private variables
    private float lastFireTime = 0f;
    private bool isReloading = false;
    private bool isFiring = false;
    private SpaceshipController spaceshipController;
    private Transform currentTarget;
    private List<GameObject> activeProjectiles = new List<GameObject>();

    // Events
    public delegate void WeaponEvent(WeaponType type, int ammo, int maxAmmo);
    public event WeaponEvent OnAmmoChanged;
    public event WeaponEvent OnWeaponSwitched;
    public event System.Action OnReloadStarted;
    public event System.Action OnReloadCompleted;
    public event System.Action<float> OnHeatChanged;

    #region Initialization

    private void Awake()
    {
        spaceshipController = GetComponent<SpaceshipController>();

        if (weaponAudioSource == null)
        {
            weaponAudioSource = gameObject.AddComponent<AudioSource>();
            weaponAudioSource.spatialBlend = 1f;
            weaponAudioSource.maxDistance = 50f;
        }

        InitializeDefaultWeapons();
    }

    private void Start()
    {
        // Select first unlocked weapon
        for (int i = 0; i < weaponConfigs.Count; i++)
        {
            if (weaponConfigs[i].unlocked)
            {
                currentWeaponIndex = i;
                currentWeaponType = weaponConfigs[i].type;
                break;
            }
        }

        OnWeaponSwitched?.Invoke(currentWeaponType, GetCurrentAmmo(), GetMaxAmmo());
    }

    private void InitializeDefaultWeapons()
    {
        if (weaponConfigs.Count == 0)
        {
            // Create default Laser weapon
            weaponConfigs.Add(new WeaponConfig
            {
                type = WeaponType.Laser,
                weaponName = "Pulse Laser",
                damage = 10f,
                projectileSpeed = 100f,
                fireRate = 10f, // 10 shots per second
                maxAmmo = 200,
                currentAmmo = 200,
                reloadTime = 2f,
                energyCost = 2f,
                range = 150f,
                unlocked = true
            });

            // Create default Missile weapon
            weaponConfigs.Add(new WeaponConfig
            {
                type = WeaponType.Missile,
                weaponName = "Guided Missile",
                damage = 50f,
                projectileSpeed = 40f,
                fireRate = 1f, // 1 shot per second
                maxAmmo = 30,
                currentAmmo = 30,
                reloadTime = 4f,
                energyCost = 15f,
                range = 200f,
                unlocked = false
            });

            // Create default Plasma weapon
            weaponConfigs.Add(new WeaponConfig
            {
                type = WeaponType.Plasma,
                weaponName = "Plasma Cannon",
                damage = 30f,
                projectileSpeed = 60f,
                fireRate = 3f, // 3 shots per second
                maxAmmo = 80,
                currentAmmo = 80,
                reloadTime = 3f,
                energyCost = 8f,
                range = 120f,
                unlocked = false
            });
        }
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        HandleInput();
        UpdateHeat();
        UpdateTargeting();
        CleanupProjectiles();
    }

    private void HandleInput()
    {
        // Fire weapon (Left Mouse Button or Spacebar)
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            TryFire();
        }

        // Reload (R key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }

        // Switch weapon (1, 2, 3 keys or Mouse Wheel)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);

        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel > 0f) SwitchToNextWeapon();
        if (scrollWheel < 0f) SwitchToPreviousWeapon();
    }

    private void UpdateHeat()
    {
        // Cool down weapon
        if (currentHeat > 0f)
        {
            currentHeat -= heatDissipationRate * Time.deltaTime;
            currentHeat = Mathf.Max(currentHeat, 0f);
            OnHeatChanged?.Invoke(GetHeatPercentage());
        }

        // Check for overheating recovery
        if (isOverheated && currentHeat < 30f)
        {
            isOverheated = false;
        }
    }

    private void UpdateTargeting()
    {
        if (!autoTargeting) return;

        // Find nearest enemy in range
        Collider[] targets = Physics.OverlapSphere(transform.position, targetingRange, targetLayer);
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;

        foreach (Collider target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target.transform;
            }
        }

        currentTarget = closestTarget;
    }

    #endregion

    #region Weapon Firing

    public void TryFire()
    {
        if (isReloading || isOverheated) return;

        WeaponConfig weapon = GetCurrentWeapon();
        if (weapon == null) return;

        // Check fire rate
        float fireInterval = 1f / (weapon.fireRate * weapon.fireRateUpgradeMultiplier);
        if (Time.time - lastFireTime < fireInterval) return;

        // Check ammo
        if (!infiniteAmmo && weapon.currentAmmo <= 0)
        {
            PlaySound(weapon.emptySound);
            StartReload();
            return;
        }

        // Check energy
        if (spaceshipController != null && !spaceshipController.HasEnergy(weapon.energyCost))
        {
            PlaySound(weapon.emptySound);
            return;
        }

        // Fire weapon
        Fire(weapon);
        lastFireTime = Time.time;
    }

    private void Fire(WeaponConfig weapon)
    {
        // Consume resources
        if (!infiniteAmmo)
        {
            weapon.currentAmmo--;
            OnAmmoChanged?.Invoke(weapon.type, weapon.currentAmmo,
                Mathf.RoundToInt(weapon.maxAmmo * weapon.ammoUpgradeMultiplier));
        }

        if (spaceshipController != null)
        {
            spaceshipController.ConsumeEnergy(weapon.energyCost);
        }

        // Add heat
        currentHeat += heatPerShot;
        if (currentHeat >= overheatingThreshold)
        {
            isOverheated = true;
            PlaySound(overheatingSound);
        }
        OnHeatChanged?.Invoke(GetHeatPercentage());

        // Spawn projectile
        SpawnProjectile(weapon);

        // Visual effects
        ShowMuzzleFlash(weapon);

        // Audio
        PlaySound(weapon.fireSound);

        // Camera shake (if implemented)
        CameraShake(0.1f);
    }

    private void SpawnProjectile(WeaponConfig weapon)
    {
        Transform spawnPoint = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        // Apply targeting assistance
        if (currentTarget != null && autoAimAssist > 0f)
        {
            Vector3 directionToTarget = (currentTarget.position - spawnPosition).normalized;
            Vector3 currentDirection = spawnRotation * Vector3.forward;
            Vector3 assistedDirection = Vector3.Lerp(currentDirection, directionToTarget, autoAimAssist);
            spawnRotation = Quaternion.LookRotation(assistedDirection);
        }

        // Create projectile
        GameObject projectile = null;

        switch (weapon.type)
        {
            case WeaponType.Laser:
                projectile = CreateLaserProjectile(weapon, spawnPosition, spawnRotation);
                break;

            case WeaponType.Missile:
                projectile = CreateMissileProjectile(weapon, spawnPosition, spawnRotation);
                break;

            case WeaponType.Plasma:
                projectile = CreatePlasmaProjectile(weapon, spawnPosition, spawnRotation);
                break;
        }

        if (projectile != null)
        {
            activeProjectiles.Add(projectile);
            Destroy(projectile, 10f); // Auto-cleanup after 10 seconds
        }
    }

    private GameObject CreateLaserProjectile(WeaponConfig weapon, Vector3 position, Quaternion rotation)
    {
        GameObject projectile;

        if (weapon.projectilePrefab != null)
        {
            projectile = Instantiate(weapon.projectilePrefab, position, rotation);
        }
        else
        {
            // Create default laser projectile
            projectile = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            projectile.transform.position = position;
            projectile.transform.rotation = rotation;
            projectile.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            projectile.GetComponent<Renderer>().material.color = Color.cyan;
            projectile.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            projectile.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.cyan * 2f);
        }

        // Add physics
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null) rb = projectile.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = rotation * Vector3.forward * weapon.projectileSpeed;

        // Add projectile component
        Projectile projScript = projectile.AddComponent<Projectile>();
        projScript.Initialize(weapon.damage * weapon.damageUpgradeMultiplier, weapon.range,
            ProjectileType.Laser, hitEffectPrefab);

        return projectile;
    }

    private GameObject CreateMissileProjectile(WeaponConfig weapon, Vector3 position, Quaternion rotation)
    {
        GameObject projectile;

        if (weapon.projectilePrefab != null)
        {
            projectile = Instantiate(weapon.projectilePrefab, position, rotation);
        }
        else
        {
            // Create default missile projectile
            projectile = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            projectile.transform.position = position;
            projectile.transform.rotation = rotation;
            projectile.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
            projectile.GetComponent<Renderer>().material.color = Color.red;
        }

        // Add physics
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null) rb = projectile.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = rotation * Vector3.forward * weapon.projectileSpeed;

        // Add projectile component with homing
        Projectile projScript = projectile.AddComponent<Projectile>();
        projScript.Initialize(weapon.damage * weapon.damageUpgradeMultiplier, weapon.range,
            ProjectileType.Missile, explosionEffectPrefab);
        projScript.SetTarget(currentTarget);

        return projectile;
    }

    private GameObject CreatePlasmaProjectile(WeaponConfig weapon, Vector3 position, Quaternion rotation)
    {
        GameObject projectile;

        if (weapon.projectilePrefab != null)
        {
            projectile = Instantiate(weapon.projectilePrefab, position, rotation);
        }
        else
        {
            // Create default plasma projectile
            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = position;
            projectile.transform.rotation = rotation;
            projectile.transform.localScale = Vector3.one * 0.5f;
            projectile.GetComponent<Renderer>().material.color = Color.green;
            projectile.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            projectile.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green * 3f);
        }

        // Add physics
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null) rb = projectile.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = rotation * Vector3.forward * weapon.projectileSpeed;

        // Add projectile component with splash damage
        Projectile projScript = projectile.AddComponent<Projectile>();
        projScript.Initialize(weapon.damage * weapon.damageUpgradeMultiplier, weapon.range,
            ProjectileType.Plasma, plasmaImpactPrefab);
        projScript.SetSplashRadius(5f);

        return projectile;
    }

    #endregion

    #region Weapon Management

    public void SwitchWeapon(int index)
    {
        if (index < 0 || index >= weaponConfigs.Count) return;
        if (!weaponConfigs[index].unlocked) return;
        if (index == currentWeaponIndex) return;
        if (isReloading) return;

        currentWeaponIndex = index;
        currentWeaponType = weaponConfigs[index].type;

        PlaySound(weaponSwitchSound);
        OnWeaponSwitched?.Invoke(currentWeaponType, GetCurrentAmmo(), GetMaxAmmo());
    }

    public void SwitchToNextWeapon()
    {
        int nextIndex = (currentWeaponIndex + 1) % weaponConfigs.Count;

        // Find next unlocked weapon
        for (int i = 0; i < weaponConfigs.Count; i++)
        {
            if (weaponConfigs[nextIndex].unlocked)
            {
                SwitchWeapon(nextIndex);
                return;
            }
            nextIndex = (nextIndex + 1) % weaponConfigs.Count;
        }
    }

    public void SwitchToPreviousWeapon()
    {
        int prevIndex = currentWeaponIndex - 1;
        if (prevIndex < 0) prevIndex = weaponConfigs.Count - 1;

        // Find previous unlocked weapon
        for (int i = 0; i < weaponConfigs.Count; i++)
        {
            if (weaponConfigs[prevIndex].unlocked)
            {
                SwitchWeapon(prevIndex);
                return;
            }
            prevIndex--;
            if (prevIndex < 0) prevIndex = weaponConfigs.Count - 1;
        }
    }

    public void UnlockWeapon(WeaponType type)
    {
        WeaponConfig weapon = GetWeaponByType(type);
        if (weapon != null)
        {
            weapon.unlocked = true;
        }
    }

    #endregion

    #region Reload System

    public void StartReload()
    {
        if (isReloading) return;

        WeaponConfig weapon = GetCurrentWeapon();
        if (weapon == null) return;

        int maxAmmo = Mathf.RoundToInt(weapon.maxAmmo * weapon.ammoUpgradeMultiplier);
        if (weapon.currentAmmo >= maxAmmo) return;

        StartCoroutine(ReloadCoroutine(weapon));
    }

    private IEnumerator ReloadCoroutine(WeaponConfig weapon)
    {
        isReloading = true;
        OnReloadStarted?.Invoke();
        PlaySound(weapon.reloadSound);

        yield return new WaitForSeconds(weapon.reloadTime);

        weapon.currentAmmo = Mathf.RoundToInt(weapon.maxAmmo * weapon.ammoUpgradeMultiplier);
        isReloading = false;

        OnReloadCompleted?.Invoke();
        OnAmmoChanged?.Invoke(weapon.type, weapon.currentAmmo,
            Mathf.RoundToInt(weapon.maxAmmo * weapon.ammoUpgradeMultiplier));
    }

    #endregion

    #region Visual & Audio Effects

    private void ShowMuzzleFlash(WeaponConfig weapon)
    {
        if (weapon.muzzleFlashPrefab != null && projectileSpawnPoint != null)
        {
            GameObject flash = Instantiate(weapon.muzzleFlashPrefab,
                projectileSpawnPoint.position, projectileSpawnPoint.rotation, projectileSpawnPoint);
            Destroy(flash, 0.1f);
        }

        if (weapon.muzzleParticles != null)
        {
            weapon.muzzleParticles.Play();
        }

        if (weapon.muzzleLight != null)
        {
            StartCoroutine(FlashLight(weapon.muzzleLight));
        }
    }

    private IEnumerator FlashLight(Light light)
    {
        light.enabled = true;
        yield return new WaitForSeconds(0.05f);
        light.enabled = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && weaponAudioSource != null)
        {
            weaponAudioSource.PlayOneShot(clip);
        }
    }

    private void CameraShake(float intensity)
    {
        // This would trigger camera shake if CameraController is implemented
        // For now, just a placeholder
    }

    #endregion

    #region Upgrade System

    public void UpgradeWeapon(WeaponType type, float damageMult, float fireRateMult, float ammoMult)
    {
        WeaponConfig weapon = GetWeaponByType(type);
        if (weapon != null)
        {
            weapon.damageUpgradeMultiplier = damageMult;
            weapon.fireRateUpgradeMultiplier = fireRateMult;
            weapon.ammoUpgradeMultiplier = ammoMult;
        }
    }

    #endregion

    #region Helper Methods

    private WeaponConfig GetCurrentWeapon()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < weaponConfigs.Count)
        {
            return weaponConfigs[currentWeaponIndex];
        }
        return null;
    }

    private WeaponConfig GetWeaponByType(WeaponType type)
    {
        foreach (var weapon in weaponConfigs)
        {
            if (weapon.type == type) return weapon;
        }
        return null;
    }

    private void CleanupProjectiles()
    {
        activeProjectiles.RemoveAll(p => p == null);
    }

    public int GetCurrentAmmo()
    {
        WeaponConfig weapon = GetCurrentWeapon();
        return weapon != null ? weapon.currentAmmo : 0;
    }

    public int GetMaxAmmo()
    {
        WeaponConfig weapon = GetCurrentWeapon();
        return weapon != null ? Mathf.RoundToInt(weapon.maxAmmo * weapon.ammoUpgradeMultiplier) : 0;
    }

    public float GetHeatPercentage() => currentHeat / maxHeat;
    public bool IsReloading() => isReloading;
    public bool IsOverheated() => isOverheated;
    public WeaponType GetCurrentWeaponType() => currentWeaponType;

    #endregion
}

#region Projectile Component

public enum ProjectileType { Laser, Missile, Plasma }

public class Projectile : MonoBehaviour
{
    private float damage;
    private float maxRange;
    private ProjectileType type;
    private GameObject impactEffect;
    private Vector3 startPosition;
    private Transform target;
    private float splashRadius = 0f;
    private float homingStrength = 5f;

    public void Initialize(float dmg, float range, ProjectileType projType, GameObject effect)
    {
        damage = dmg;
        maxRange = range;
        type = projType;
        impactEffect = effect;
        startPosition = transform.position;
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void SetSplashRadius(float radius)
    {
        splashRadius = radius;
    }

    private void Update()
    {
        // Check range
        if (Vector3.Distance(startPosition, transform.position) > maxRange)
        {
            Destroy(gameObject);
            return;
        }

        // Homing for missiles
        if (type == ProjectileType.Missile && target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.Lerp(rb.velocity, direction * rb.velocity.magnitude,
                    homingStrength * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(rb.velocity);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Apply damage
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        // Splash damage for plasma
        if (type == ProjectileType.Plasma && splashRadius > 0f)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, splashRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy"))
                {
                    var enemy = hitCollider.GetComponent<EnemyController>();
                    if (enemy != null)
                    {
                        float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                        float damageMultiplier = 1f - (distance / splashRadius);
                        enemy.TakeDamage(damage * damageMultiplier * 0.5f);
                    }
                }
            }
        }

        // Spawn impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}

// Placeholder for EnemyController (to be implemented)
public class EnemyController : MonoBehaviour
{
    public void TakeDamage(float damage)
    {
        // Enemy damage logic here
        Debug.Log($"Enemy took {damage} damage");
    }
}

#endregion
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace SpaceRPG.Effects
{
    /// <summary>
    /// Sistema completo de gerenciamento de efeitos visuais
    /// Integra: Free Quick Effects, Particle Pack, 3D Games Effects Pack
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        private static EffectManager _instance;
        public static EffectManager Instance => _instance;

        [System.Serializable]
        public class EffectData
        {
            public string effectName;
            public GameObject effectPrefab;
            public EffectType type;
            public float lifetime = 2f;
            public bool autoPlay = true;
            public bool usePooling = true;
        }

        public enum EffectType
        {
            LaserShot,          // Quick Effects
            LaserImpact,        // Quick Effects
            MissileTrail,       // Particle Pack
            Explosion,          // Particle Pack
            PlasmaShot,         // Effects Pack
            PlasmaImpact,       // Effects Pack
            ShipTrail,          // Particle Pack
            ShieldHit,          // Effects Pack
            Warp,               // Effects Pack
            Teleport,           // Effects Pack
            PowerUp,            // Quick Effects
            DeathExplosion,     // Particle Pack
            EngineFlare,        // Effects Pack
            Smoke,              // Particle Pack
            Sparks,             // Effects Pack
            EnergyShield,       // Effects Pack
            HealEffect,         // Quick Effects
            DamageIndicator     // Effects Pack
        }

        [Header("Effect Prefabs - Quick Effects")]
        [SerializeField] private EffectData[] quickEffects;

        [Header("Effect Prefabs - Particle Pack")]
        [SerializeField] private EffectData[] particlePackEffects;

        [Header("Effect Prefabs - 3D Games Effects")]
        [SerializeField] private EffectData[] gamesEffects;

        [Header("Trail Renderers")]
        [SerializeField] private Material laserTrailMaterial;
        [SerializeField] private Material missileTrailMaterial;
        [SerializeField] private Material shipTrailMaterial;

        [Header("Pooling Settings")]
        [SerializeField] private int defaultPoolSize = 20;
        [SerializeField] private int maxPoolSize = 100;

        [Header("Audio")]
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private AudioClip laserSound;
        [SerializeField] private AudioClip missileSound;
        [SerializeField] private AudioClip shieldHitSound;

        private Dictionary<EffectType, EffectData> effectDatabase;
        private Dictionary<EffectType, ObjectPool<GameObject>> effectPools;
        private AudioSource audioSource;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystem();
        }

        private void InitializeSystem()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f;

            effectDatabase = new Dictionary<EffectType, EffectData>();
            effectPools = new Dictionary<EffectType, ObjectPool<GameObject>>();

            LoadEffects();
            CreatePools();
        }

        private void LoadEffects()
        {
            // Carregar Quick Effects
            if (quickEffects != null)
            {
                foreach (var effect in quickEffects)
                {
                    if (!effectDatabase.ContainsKey(effect.type))
                        effectDatabase.Add(effect.type, effect);
                }
            }

            // Carregar Particle Pack
            if (particlePackEffects != null)
            {
                foreach (var effect in particlePackEffects)
                {
                    if (!effectDatabase.ContainsKey(effect.type))
                        effectDatabase.Add(effect.type, effect);
                }
            }

            // Carregar Games Effects
            if (gamesEffects != null)
            {
                foreach (var effect in gamesEffects)
                {
                    if (!effectDatabase.ContainsKey(effect.type))
                        effectDatabase.Add(effect.type, effect);
                }
            }

            Debug.Log($"Loaded {effectDatabase.Count} effects into database");
        }

        private void CreatePools()
        {
            foreach (var kvp in effectDatabase)
            {
                if (kvp.Value.usePooling)
                {
                    var pool = new ObjectPool<GameObject>(
                        createFunc: () => CreateEffect(kvp.Key),
                        actionOnGet: (obj) => OnGetFromPool(obj),
                        actionOnRelease: (obj) => OnReleaseToPool(obj),
                        actionOnDestroy: (obj) => Destroy(obj),
                        collectionCheck: false,
                        defaultCapacity: defaultPoolSize,
                        maxSize: maxPoolSize
                    );

                    effectPools.Add(kvp.Key, pool);
                }
            }
        }

        private GameObject CreateEffect(EffectType type)
        {
            if (effectDatabase.TryGetValue(type, out EffectData data))
            {
                if (data.effectPrefab != null)
                {
                    GameObject obj = Instantiate(data.effectPrefab);
                    obj.SetActive(false);
                    return obj;
                }
            }
            return null;
        }

        private void OnGetFromPool(GameObject obj)
        {
            obj.SetActive(true);

            // Resetar partículas
            var particles = obj.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Clear();
                ps.Play();
            }
        }

        private void OnReleaseToPool(GameObject obj)
        {
            obj.SetActive(false);
        }

        public GameObject PlayEffect(EffectType type, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!effectDatabase.ContainsKey(type))
            {
                Debug.LogWarning($"Effect type {type} not found in database!");
                return null;
            }

            GameObject effectObj = null;
            EffectData data = effectDatabase[type];

            // Usar pooling se habilitado
            if (data.usePooling && effectPools.ContainsKey(type))
            {
                effectObj = effectPools[type].Get();
            }
            else
            {
                effectObj = Instantiate(data.effectPrefab);
            }

            if (effectObj == null) return null;

            // Posicionar efeito
            effectObj.transform.position = position;
            effectObj.transform.rotation = rotation;

            if (parent != null)
                effectObj.transform.SetParent(parent);

            // Auto-destruir ou retornar ao pool
            if (data.lifetime > 0)
            {
                StartCoroutine(ReturnEffectToPool(effectObj, type, data.lifetime, data.usePooling));
            }

            // Play audio
            PlayEffectSound(type);

            return effectObj;
        }

        private System.Collections.IEnumerator ReturnEffectToPool(GameObject obj, EffectType type, float delay, bool usePooling)
        {
            yield return new WaitForSeconds(delay);

            if (obj != null)
            {
                if (usePooling && effectPools.ContainsKey(type))
                {
                    effectPools[type].Release(obj);
                }
                else
                {
                    Destroy(obj);
                }
            }
        }

        public GameObject PlayLaserShot(Vector3 position, Vector3 direction)
        {
            var effect = PlayEffect(EffectType.LaserShot, position, Quaternion.LookRotation(direction));

            // Adicionar trail renderer
            if (effect != null && laserTrailMaterial != null)
            {
                var trail = effect.AddComponent<TrailRenderer>();
                trail.material = laserTrailMaterial;
                trail.time = 0.3f;
                trail.startWidth = 0.2f;
                trail.endWidth = 0.01f;
                trail.colorGradient = CreateLaserGradient();
            }

            return effect;
        }

        public GameObject PlayExplosion(Vector3 position, float scale = 1f)
        {
            var effect = PlayEffect(EffectType.Explosion, position, Quaternion.identity);

            if (effect != null)
            {
                effect.transform.localScale = Vector3.one * scale;
            }

            return effect;
        }

        public GameObject PlayMissileTrail(Transform missileTransform)
        {
            var effect = PlayEffect(EffectType.MissileTrail, missileTransform.position, missileTransform.rotation, missileTransform);
            return effect;
        }

        public GameObject PlayShipEngineTrail(Transform shipTransform, Vector3 localOffset)
        {
            Vector3 position = shipTransform.position + shipTransform.TransformDirection(localOffset);
            var effect = PlayEffect(EffectType.ShipTrail, position, shipTransform.rotation, shipTransform);
            return effect;
        }

        public void PlayShieldHit(Vector3 position, Vector3 normal)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            PlayEffect(EffectType.ShieldHit, position, rotation);
        }

        public void PlayWarpEffect(Vector3 position)
        {
            PlayEffect(EffectType.Warp, position, Quaternion.identity);
        }

        public void PlayTeleportEffect(Vector3 position)
        {
            PlayEffect(EffectType.Teleport, position, Quaternion.identity);
        }

        public void PlayPowerUpEffect(Transform target)
        {
            PlayEffect(EffectType.PowerUp, target.position, Quaternion.identity, target);
        }

        public void PlayDeathExplosion(Vector3 position, float scale = 1f)
        {
            var effect = PlayExplosion(position, scale);

            // Adicionar efeitos extras
            PlayEffect(EffectType.Smoke, position, Quaternion.identity);
            PlayEffect(EffectType.Sparks, position, Quaternion.identity);
        }

        public void PlayDamageIndicator(Vector3 position, Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            PlayEffect(EffectType.DamageIndicator, position, rotation);
        }

        public void PlayHealEffect(Transform target)
        {
            PlayEffect(EffectType.HealEffect, target.position, Quaternion.identity, target);
        }

        private void PlayEffectSound(EffectType type)
        {
            AudioClip clip = null;

            switch (type)
            {
                case EffectType.Explosion:
                case EffectType.DeathExplosion:
                    clip = explosionSound;
                    break;
                case EffectType.LaserShot:
                case EffectType.LaserImpact:
                    clip = laserSound;
                    break;
                case EffectType.MissileTrail:
                    clip = missileSound;
                    break;
                case EffectType.ShieldHit:
                    clip = shieldHitSound;
                    break;
            }

            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private Gradient CreateLaserGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.cyan, 0.0f),
                    new GradientColorKey(Color.blue, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            return gradient;
        }

        public void SetEffectScale(EffectType type, float scale)
        {
            if (effectDatabase.ContainsKey(type))
            {
                // Aplicar escala aos efeitos futuros deste tipo
            }
        }

        public void ClearAllEffects()
        {
            foreach (var pool in effectPools.Values)
            {
                pool.Clear();
            }
        }

        private void OnDestroy()
        {
            ClearAllEffects();
        }
    }
}

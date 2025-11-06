using UnityEngine;
using System.Collections.Generic;

namespace SpaceRPG.Effects
{
    public class ParticleEffects : MonoBehaviour
    {
        private static ParticleEffects _instance;
        public static ParticleEffects Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ParticleEffects>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ParticleEffects");
                        _instance = go.AddComponent<ParticleEffects>();
                    }
                }
                return _instance;
            }
        }

        [Header("Effect Prefabs")]
        [SerializeField] private GameObject explosionEffect;
        [SerializeField] private GameObject impactEffect;
        [SerializeField] private GameObject healEffect;
        [SerializeField] private GameObject powerUpEffect;
        [SerializeField] private GameObject purchaseEffect;
        [SerializeField] private GameObject levelUpEffect;
        [SerializeField] private GameObject sparkEffect;
        [SerializeField] private GameObject waterEffect;
        [SerializeField] private GameObject plantGrowEffect;

        [Header("Effect Settings")]
        [SerializeField] private int poolSize = 20;
        [SerializeField] private float effectLifetime = 3f;

        private Dictionary<string, Queue<GameObject>> effectPools = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }

        private void InitializePools()
        {
            // Criar pools para cada efeito
            CreatePool("Explosion", explosionEffect);
            CreatePool("Impact", impactEffect);
            CreatePool("Heal", healEffect);
            CreatePool("PowerUp", powerUpEffect);
            CreatePool("Purchase", purchaseEffect);
            CreatePool("LevelUp", levelUpEffect);
            CreatePool("Spark", sparkEffect);
            CreatePool("Water", waterEffect);
            CreatePool("PlantGrow", plantGrowEffect);
        }

        private void CreatePool(string poolName, GameObject prefab)
        {
            if (prefab == null) return;

            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                obj.name = $"{poolName}_{i}";
                pool.Enqueue(obj);
            }

            effectPools[poolName] = pool;
        }

        private GameObject GetPooledEffect(string poolName)
        {
            if (!effectPools.ContainsKey(poolName) || effectPools[poolName].Count == 0)
                return null;

            GameObject obj = effectPools[poolName].Dequeue();
            effectPools[poolName].Enqueue(obj);

            return obj;
        }

        public void PlayEffect(string effectType, Vector3 position, Quaternion rotation = default)
        {
            if (rotation == default)
                rotation = Quaternion.identity;

            GameObject effect = GetPooledEffect(effectType);
            if (effect == null)
            {
                Debug.LogWarning($"Effect pool not found: {effectType}");
                return;
            }

            effect.transform.position = position;
            effect.transform.rotation = rotation;
            effect.SetActive(true);

            // Desativar após lifetime
            StartCoroutine(DeactivateAfterTime(effect, effectLifetime));
        }

        public void PlayExplosion(Vector3 position) => PlayEffect("Explosion", position);
        public void PlayImpact(Vector3 position) => PlayEffect("Impact", position);
        public void PlayHeal(Vector3 position) => PlayEffect("Heal", position);
        public void PlayPowerUp(Vector3 position) => PlayEffect("PowerUp", position);
        public void PlayPurchase(Vector3 position) => PlayEffect("Purchase", position);
        public void PlayLevelUp(Vector3 position) => PlayEffect("LevelUp", position);
        public void PlaySpark(Vector3 position) => PlayEffect("Spark", position);
        public void PlayWater(Vector3 position) => PlayEffect("Water", position);
        public void PlayPlantGrow(Vector3 position) => PlayEffect("PlantGrow", position);

        private System.Collections.IEnumerator DeactivateAfterTime(GameObject effect, float time)
        {
            yield return new WaitForSeconds(time);
            effect.SetActive(false);
        }
    }
}

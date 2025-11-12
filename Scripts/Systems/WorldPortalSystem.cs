using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using SpaceRPG.Core;

namespace SpaceRPG.Systems
{
    /// <summary>
    /// Sistema de Portais e Mundos usando Free Skyboxes Space
    /// Gerencia transições entre 5 galáxias diferentes com portais
    /// </summary>
    public class WorldPortalSystem : MonoBehaviour
    {
        private static WorldPortalSystem _instance;
        public static WorldPortalSystem Instance => _instance;

        [System.Serializable]
        public class GalaxyWorld
        {
            public string galaxyName;
            public Material skyboxMaterial;
            public AudioClip ambientMusic;
            public Color galaxyColor;
            public float enemyDifficultyMultiplier = 1f;
            public GameObject[] uniqueEnemyPrefabs;
            public float spawnRateMultiplier = 1f;
        }

        [Header("Galaxy Worlds")]
        [SerializeField] private GalaxyWorld[] galaxyWorlds = new GalaxyWorld[5];
        [SerializeField] private int currentWorldIndex = 0;

        [Header("Portal Settings")]
        [SerializeField] private GameObject portalPrefab;
        [SerializeField] private Vector3 portalSpawnPosition = new Vector3(0, 0, 500);
        [SerializeField] private float portalActivationDistance = 50f;
        [SerializeField] private ParticleSystem portalParticles;
        [SerializeField] private Light portalLight;

        [Header("Teleport Effects")]
        [SerializeField] private ParticleSystem teleportEffect;
        [SerializeField] private GameObject warpTunnel;
        [SerializeField] private AudioClip teleportSound;
        [SerializeField] private AudioClip portalOpenSound;
        [SerializeField] private float teleportDuration = 2f;

        [Header("UI Elements")]
        [SerializeField] private GameObject galaxyChangeUI;
        [SerializeField] private TextMeshProUGUI galaxyNameText;
        [SerializeField] private Image galaxyBanner;
        [SerializeField] private float bannerDisplayTime = 3f;

        [Header("Environment")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private GameObject[] environmentProps;

        private GameObject currentPortal;
        private AudioSource audioSource;
        private bool isTeleporting = false;
        private Transform playerShip;

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

        private void Start()
        {
            LoadWorld(currentWorldIndex);
            SpawnPortal();
        }

        private void Update()
        {
            CheckPortalProximity();
        }

        private void InitializeSystem()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            if (galaxyChangeUI != null)
                galaxyChangeUI.SetActive(false);

            // Encontrar nave do jogador
            var shipController = FindObjectOfType<SpaceshipController>();
            if (shipController != null)
                playerShip = shipController.transform;
        }

        public void LoadWorld(int worldIndex)
        {
            if (worldIndex < 0 || worldIndex >= galaxyWorlds.Length)
            {
                Debug.LogError($"World index {worldIndex} out of range!");
                return;
            }

            currentWorldIndex = worldIndex;
            GalaxyWorld world = galaxyWorlds[worldIndex];

            // Aplicar skybox
            if (world.skyboxMaterial != null)
            {
                RenderSettings.skybox = world.skyboxMaterial;
                DynamicGI.UpdateEnvironment();
            }

            // Aplicar música ambiente
            if (world.ambientMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(world.ambientMusic);
            }

            // Atualizar iluminação
            if (directionalLight != null)
            {
                directionalLight.color = world.galaxyColor;
                StartCoroutine(TweenHelper.AnimateLightIntensity(directionalLight, 1.2f, 1f));
            }

            // Atualizar cor ambiente
            RenderSettings.ambientLight = world.galaxyColor * 0.5f;

            // Spawnar inimigos apropriados ao mundo
            UpdateEnemyDifficulty(world);

            Debug.Log($"World loaded: {world.galaxyName}");
        }

        private void SpawnPortal()
        {
            if (currentPortal != null)
                Destroy(currentPortal);

            if (portalPrefab != null)
            {
                currentPortal = Instantiate(portalPrefab, portalSpawnPosition, Quaternion.identity);

                // Configurar efeitos do portal
                SetupPortalEffects(currentPortal);

                PlaySound(portalOpenSound);
            }
        }

        private void SetupPortalEffects(GameObject portal)
        {
            // Adicionar partículas
            if (portalParticles != null)
            {
                var particles = Instantiate(portalParticles, portal.transform);
                particles.transform.localPosition = Vector3.zero;
                particles.Play();

                // Cor baseada no próximo mundo
                int nextWorldIndex = (currentWorldIndex + 1) % galaxyWorlds.Length;
                var main = particles.main;
                main.startColor = galaxyWorlds[nextWorldIndex].galaxyColor;
            }

            // Adicionar luz
            if (portalLight != null)
            {
                var light = Instantiate(portalLight, portal.transform);
                light.transform.localPosition = Vector3.zero;

                int nextWorldIndex = (currentWorldIndex + 1) % galaxyWorlds.Length;
                light.color = galaxyWorlds[nextWorldIndex].galaxyColor;

                // Animação pulsante
                StartCoroutine(TweenHelper.AnimateLightIntensity(light, 3f, 1f, true));
            }

            // Rotação contínua
            StartCoroutine(TweenHelper.RotateContinuous(portal.transform, Vector3.up, 36f));
        }

        private void CheckPortalProximity()
        {
            if (currentPortal == null || playerShip == null || isTeleporting)
                return;

            float distance = Vector3.Distance(playerShip.position, currentPortal.transform.position);

            if (distance < portalActivationDistance)
            {
                // Mostrar prompt para o jogador
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TeleportToNextWorld();
                }
            }
        }

        public void TeleportToNextWorld()
        {
            if (isTeleporting) return;

            int nextWorldIndex = (currentWorldIndex + 1) % galaxyWorlds.Length;
            StartCoroutine(TeleportSequence(nextWorldIndex));
        }

        private IEnumerator TeleportSequence(int targetWorldIndex)
        {
            isTeleporting = true;

            // Efeito de entrada no portal
            if (teleportEffect != null && playerShip != null)
            {
                var effect = Instantiate(teleportEffect, playerShip.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, 3f);
            }

            PlaySound(teleportSound);

            // Ativar túnel de warp
            if (warpTunnel != null)
            {
                warpTunnel.SetActive(true);
            }

            // Fade screen ou efeito visual
            yield return new WaitForSeconds(teleportDuration * 0.3f);

            // Carregar novo mundo
            LoadWorld(targetWorldIndex);

            // Mover jogador para nova posição
            if (playerShip != null)
            {
                playerShip.position = new Vector3(0, 0, -100);
            }

            // Mostrar banner de nova galáxia
            ShowGalaxyBanner(galaxyWorlds[targetWorldIndex]);

            yield return new WaitForSeconds(teleportDuration * 0.7f);

            // Desativar túnel de warp
            if (warpTunnel != null)
            {
                warpTunnel.SetActive(false);
            }

            // Spawnar novo portal
            SpawnPortal();

            isTeleporting = false;
        }

        private void ShowGalaxyBanner(GalaxyWorld world)
        {
            if (galaxyChangeUI == null) return;

            galaxyChangeUI.SetActive(true);

            if (galaxyNameText != null)
            {
                galaxyNameText.text = $"ENTERING: {world.galaxyName.ToUpper()}";
                galaxyNameText.color = world.galaxyColor;
            }

            if (galaxyBanner != null)
            {
                galaxyBanner.color = world.galaxyColor;

                // Animação do banner
                galaxyBanner.transform.localScale = Vector3.zero;
                StartCoroutine(TweenHelper.AnimateScale(galaxyBanner.transform, Vector3.one, 0.5f, TweenHelper.EaseType.OutBack));
            }

            // Esconder banner após tempo
            StartCoroutine(TweenHelper.DelayedCall(bannerDisplayTime, () =>
            {
                if (galaxyBanner != null)
                {
                    StartCoroutine(HideBanner());
                }
            }));
        }

        private void UpdateEnemyDifficulty(GalaxyWorld world)
        {
            // Atualizar dificuldade dos inimigos
            var enemies = FindObjectsOfType<EnemyController>();
            foreach (var enemy in enemies)
            {
                // Aumentar stats baseado no multiplicador do mundo
                enemy.SetDifficultyMultiplier(world.enemyDifficultyMultiplier);
            }

            // Atualizar taxa de spawn
            var spawners = FindObjectsOfType<EnemySpawner>();
            foreach (var spawner in spawners)
            {
                spawner.SetSpawnRateMultiplier(world.spawnRateMultiplier);
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public GalaxyWorld GetCurrentWorld()
        {
            return galaxyWorlds[currentWorldIndex];
        }

        public int GetCurrentWorldIndex()
        {
            return currentWorldIndex;
        }

        public string GetCurrentWorldName()
        {
            return galaxyWorlds[currentWorldIndex].galaxyName;
        }

        public void SetPortalPosition(Vector3 position)
        {
            if (currentPortal != null)
            {
                currentPortal.transform.position = position;
            }
        }

        private IEnumerator HideBanner()
        {
            if (galaxyBanner != null)
            {
                yield return TweenHelper.AnimateScale(galaxyBanner.transform, Vector3.zero, 0.3f, TweenHelper.EaseType.InBack);
                galaxyChangeUI.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}

using UnityEngine;
using System.Collections;
using TMPro;
using SpaceRPG.Core;

namespace SpaceRPG.Systems
{
    /// <summary>
    /// Sistema de lançamento de naves usando The Courtyard
    /// Controla animações de decolagem, câmera cinematográfica e efeitos
    /// </summary>
    public class LaunchpadController : MonoBehaviour
    {
        private static LaunchpadController _instance;
        public static LaunchpadController Instance => _instance;

        [System.Serializable]
        public class ShipLaunchpad
        {
            public string shipName;
            public GameObject shipModel;
            public Transform launchPosition;
            public bool isOccupied = true;
        }

        [Header("Courtyard Environment")]
        [SerializeField] private GameObject courtyardEnvironment;
        [SerializeField] private Transform[] launchpads;
        [SerializeField] private ShipLaunchpad[] parkedShips;

        [Header("Launch Settings")]
        [SerializeField] private float launchHeight = 100f;
        [SerializeField] private float launchDuration = 3f;
        [SerializeField] private AnimationCurve launchCurve;

        [Header("Camera Settings")]
        [SerializeField] private Camera launchCamera;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float cameraFollowDuration = 2f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 5, -15);

        [Header("Launch Effects")]
        [SerializeField] private ParticleSystem[] engineFlareParticles;
        [SerializeField] private ParticleSystem groundDustEffect;
        [SerializeField] private ParticleSystem launchSmokeEffect;
        [SerializeField] private Light[] engineLights;
        [SerializeField] private GameObject exhaustTrail;

        [Header("Audio")]
        [SerializeField] private AudioClip engineStartupSound;
        [SerializeField] private AudioClip engineIdleSound;
        [SerializeField] private AudioClip launchSound;
        [SerializeField] private AudioClip sonicBoomSound;

        [Header("UI")]
        [SerializeField] private GameObject launchUI;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI statusText;

        private AudioSource audioSource;
        private Transform currentShip;
        private bool isLaunching = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            InitializeSystem();
        }

        private void Start()
        {
            SetupLaunchpads();
        }

        private void InitializeSystem()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.7f;

            if (launchUI != null)
                launchUI.SetActive(false);

            // Configurar câmera de lançamento
            if (launchCamera != null)
            {
                launchCamera.enabled = false; // Desabilitada inicialmente
            }
        }

        private void SetupLaunchpads()
        {
            // Posicionar naves nos launchpads
            for (int i = 0; i < parkedShips.Length && i < launchpads.Length; i++)
            {
                if (parkedShips[i].shipModel != null && launchpads[i] != null)
                {
                    parkedShips[i].shipModel.transform.position = launchpads[i].position;
                    parkedShips[i].shipModel.transform.rotation = launchpads[i].rotation;
                    parkedShips[i].launchPosition = launchpads[i];

                    // Adicionar efeitos de idle
                    AddIdleEffects(parkedShips[i].shipModel);
                }
            }
        }

        private void AddIdleEffects(GameObject ship)
        {
            // Luz pulsante nos motores
            var lights = ship.GetComponentsInChildren<Light>();
            foreach (var light in lights)
            {
                StartCoroutine(TweenHelper.AnimateLightIntensity(light, light.intensity * 1.5f, 1f, true));
            }

            // Vapor leve saindo dos motores
            if (engineFlareParticles != null && engineFlareParticles.Length > 0)
            {
                for (int i = 0; i < engineFlareParticles.Length; i++)
                {
                    var idle = Instantiate(engineFlareParticles[i], ship.transform);
                    var main = idle.main;
                    main.startLifetime = 2f;
                    main.startSpeed = 0.5f;
                    idle.Play();
                }
            }
        }

        public void InitiateLaunch(string shipName)
        {
            if (isLaunching)
            {
                Debug.LogWarning("Launch already in progress!");
                return;
            }

            // Encontrar nave para lançar
            ShipLaunchpad selectedShip = null;
            foreach (var ship in parkedShips)
            {
                if (ship.shipName == shipName && ship.isOccupied)
                {
                    selectedShip = ship;
                    break;
                }
            }

            if (selectedShip == null)
            {
                Debug.LogError($"Ship {shipName} not found or not available!");
                return;
            }

            StartCoroutine(LaunchSequence(selectedShip));
        }

        private IEnumerator LaunchSequence(ShipLaunchpad ship)
        {
            isLaunching = true;
            currentShip = ship.shipModel.transform;

            // Mostrar UI
            if (launchUI != null)
            {
                launchUI.SetActive(true);
            }

            // FASE 1: Preparação (Pre-Launch)
            yield return StartCoroutine(PreLaunchPhase());

            // FASE 2: Contagem Regressiva
            yield return StartCoroutine(CountdownPhase());

            // FASE 3: Ignição dos Motores
            yield return StartCoroutine(EngineIgnitionPhase());

            // FASE 4: Decolagem
            yield return StartCoroutine(TakeoffPhase());

            // FASE 5: Transição para Gameplay
            yield return StartCoroutine(TransitionToGameplay());

            isLaunching = false;
        }

        private IEnumerator PreLaunchPhase()
        {
            UpdateStatus("PREPARING FOR LAUNCH...");

            // Ativar câmera de lançamento
            if (launchCamera != null)
            {
                launchCamera.enabled = true;
                if (mainCamera != null)
                    mainCamera.enabled = false;
            }

            // Posicionar câmera para seguir nave
            if (launchCamera != null && currentShip != null)
            {
                StartCoroutine(FollowShipCamera());
            }

            // Efeitos de preparação
            PlaySound(engineStartupSound);

            yield return new WaitForSeconds(2f);
        }

        private IEnumerator CountdownPhase()
        {
            UpdateStatus("INITIATING LAUNCH SEQUENCE...");

            // Contagem regressiva
            for (int i = 5; i > 0; i--)
            {
                if (countdownText != null)
                {
                    countdownText.text = i.ToString();
                    countdownText.transform.localScale = Vector3.zero;
                    StartCoroutine(AnimateCountdown());
                }

                PlaySound(engineIdleSound);
                yield return new WaitForSeconds(1f);
            }

            if (countdownText != null)
            {
                countdownText.text = "LAUNCH!";
                countdownText.color = Color.green;
            }
        }

        private IEnumerator EngineIgnitionPhase()
        {
            UpdateStatus("ENGINE IGNITION");

            // Ativar efeitos dos motores
            if (engineFlareParticles != null)
            {
                foreach (var particle in engineFlareParticles)
                {
                    var instance = Instantiate(particle, currentShip);
                    instance.transform.localPosition = new Vector3(0, -2, 0);
                    instance.Play();

                    var main = instance.main;
                    main.startSpeed = 10f;
                    main.startSize = 3f;
                }
            }

            // Ativar luzes dos motores
            if (engineLights != null)
            {
                foreach (var light in engineLights)
                {
                    var instance = Instantiate(light, currentShip);
                    instance.transform.localPosition = new Vector3(0, -2, 0);
                    instance.intensity = 0f;
                    StartCoroutine(TweenHelper.AnimateLightIntensity(instance, 10f, 0.5f));
                }
            }

            // Efeito de poeira no chão
            if (groundDustEffect != null)
            {
                var dust = Instantiate(groundDustEffect, currentShip.position, Quaternion.identity);
                dust.Play();
                Destroy(dust.gameObject, 3f);
            }

            // Som de ignição
            PlaySound(launchSound);

            // Shake da nave
            StartCoroutine(TweenHelper.ShakeRotation(currentShip, 1f, 5f, 20));

            yield return new WaitForSeconds(1.5f);
        }

        private IEnumerator TakeoffPhase()
        {
            UpdateStatus("LIFTOFF!");

            Vector3 startPosition = currentShip.position;
            Vector3 endPosition = startPosition + Vector3.up * launchHeight;

            // Fumaça de lançamento
            if (launchSmokeEffect != null)
            {
                var smoke = Instantiate(launchSmokeEffect, startPosition, Quaternion.identity);
                smoke.Play();
                Destroy(smoke.gameObject, 5f);
            }

            // Trail de escape
            if (exhaustTrail != null)
            {
                var trail = Instantiate(exhaustTrail, currentShip);
                trail.transform.localPosition = new Vector3(0, -2, 0);
            }

            // Animação de decolagem
            float elapsed = 0f;
            while (elapsed < launchDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / launchDuration;

                // Usar curva de animação
                float curveValue = launchCurve != null ? launchCurve.Evaluate(progress) : progress;

                currentShip.position = Vector3.Lerp(startPosition, endPosition, curveValue);

                // Acelerar rotação
                currentShip.Rotate(Vector3.up, Time.deltaTime * 30f);

                // Aumentar intensidade dos efeitos
                if (engineLights != null)
                {
                    foreach (var light in engineLights)
                    {
                        light.intensity = Mathf.Lerp(5f, 15f, curveValue);
                    }
                }

                yield return null;
            }

            // Sonic boom ao sair
            PlaySound(sonicBoomSound);

            yield return new WaitForSeconds(1f);
        }

        private IEnumerator TransitionToGameplay()
        {
            UpdateStatus("ENTERING SPACE...");

            // Fade da câmera
            if (launchCamera != null)
            {
                launchCamera.enabled = false;
                if (mainCamera != null)
                    mainCamera.enabled = true;
            }

            // Esconder UI
            if (launchUI != null)
            {
                launchUI.SetActive(false);
            }

            // Mover nave para posição inicial de gameplay
            if (currentShip != null)
            {
                StartCoroutine(TweenHelper.AnimatePosition(currentShip, Vector3.zero, 1f));
                StartCoroutine(TweenHelper.AnimateRotation(currentShip, Vector3.zero, 1f));
            }

            yield return new WaitForSeconds(1f);

            // Ativar controles do jogador
            var shipController = currentShip.GetComponent<SpaceshipController>();
            if (shipController != null)
            {
                shipController.enabled = true;
            }

            UpdateStatus("LAUNCH COMPLETE - GOOD LUCK!");
            yield return new WaitForSeconds(2f);

            if (launchUI != null)
            {
                launchUI.SetActive(false);
            }
        }

        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
                Debug.Log($"[LAUNCH] {message}");
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public void AddShipToLaunchpad(GameObject ship, int launchpadIndex)
        {
            if (launchpadIndex < 0 || launchpadIndex >= launchpads.Length)
            {
                Debug.LogError("Invalid launchpad index!");
                return;
            }

            ship.transform.position = launchpads[launchpadIndex].position;
            ship.transform.rotation = launchpads[launchpadIndex].rotation;

            AddIdleEffects(ship);
        }

        public bool IsLaunching()
        {
            return isLaunching;
        }

        private IEnumerator FollowShipCamera()
        {
            while (isLaunching && currentShip != null && launchCamera != null)
            {
                Vector3 targetPosition = currentShip.position + cameraOffset;
                launchCamera.transform.position = Vector3.Lerp(launchCamera.transform.position, targetPosition, Time.deltaTime * 2f);
                launchCamera.transform.LookAt(currentShip);
                yield return null;
            }
        }

        private IEnumerator AnimateCountdown()
        {
            if (countdownText != null)
            {
                yield return TweenHelper.AnimateScale(countdownText.transform, Vector3.one * 1.5f, 0.5f, TweenHelper.EaseType.OutBack);
                yield return TweenHelper.AnimateScale(countdownText.transform, Vector3.zero, 0.5f);
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}

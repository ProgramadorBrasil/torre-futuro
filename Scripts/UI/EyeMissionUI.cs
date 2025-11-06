using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using SpaceRPG.Systems;

namespace SpaceRPG.UI
{
    /// <summary>
    /// Sistema de UI com Eye asset para missões
    /// Controla animações do olho, piscar, rastreamento de alvos
    /// </summary>
    public class EyeMissionUI : MonoBehaviour
    {
        private static EyeMissionUI _instance;
        public static EyeMissionUI Instance => _instance;

        [Header("Eye Asset")]
        [SerializeField] private GameObject eyeModel;
        [SerializeField] private Transform eyeBall;
        [SerializeField] private Transform pupil;
        [SerializeField] private Renderer eyeRenderer;
        [SerializeField] private Material eyeMaterial;

        [Header("Eye Animation")]
        [SerializeField] private float blinkInterval = 3f;
        [SerializeField] private float blinkDuration = 0.15f;
        [SerializeField] private float eyeLookSpeed = 5f;
        [SerializeField] private float maxLookAngle = 30f;

        [Header("Mission Display")]
        [SerializeField] private GameObject missionPanel;
        [SerializeField] private TextMeshProUGUI missionTitleText;
        [SerializeField] private TextMeshProUGUI missionDescriptionText;
        [SerializeField] private TextMeshProUGUI missionProgressText;
        [SerializeField] private Image missionProgressBar;

        [Header("Target Tracking")]
        [SerializeField] private GameObject targetIndicatorPrefab;
        [SerializeField] private Transform targetIndicatorContainer;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float maxTrackingDistance = 500f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem scanEffect;
        [SerializeField] private Light eyeLight;
        [SerializeField] private Color normalColor = Color.cyan;
        [SerializeField] private Color alertColor = Color.red;
        [SerializeField] private Color completeColor = Color.green;

        [Header("Audio")]
        [SerializeField] private AudioClip blinkSound;
        [SerializeField] private AudioClip targetAcquiredSound;
        [SerializeField] private AudioClip missionCompleteSound;
        [SerializeField] private AudioClip scanSound;

        private AudioSource audioSource;
        private Transform currentTarget;
        private float nextBlinkTime;
        private Vector3 originalEyeScale;
        private bool isScanning = false;

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
            if (mainCamera == null)
                mainCamera = Camera.main;

            nextBlinkTime = Time.time + blinkInterval;

            if (eyeModel != null)
                originalEyeScale = eyeModel.transform.localScale;

            UpdateMissionDisplay();
        }

        private void Update()
        {
            UpdateEyeAnimation();
            UpdateTargetTracking();
            UpdateEyeLook();

            // Auto blink
            if (Time.time >= nextBlinkTime)
            {
                Blink();
                nextBlinkTime = Time.time + blinkInterval + Random.Range(-0.5f, 0.5f);
            }
        }

        private void InitializeSystem()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            if (missionPanel != null)
                missionPanel.SetActive(true);

            if (eyeLight != null)
            {
                eyeLight.color = normalColor;
            }
        }

        private void UpdateEyeAnimation()
        {
            if (eyeBall == null) return;

            // Animação idle sutil
            float idleRotation = Mathf.Sin(Time.time * 2f) * 5f;
            eyeBall.localRotation = Quaternion.Euler(0, idleRotation, 0);

            // Pulsação da pupila
            if (pupil != null)
            {
                float scale = 1f + Mathf.Sin(Time.time * 3f) * 0.1f;
                pupil.localScale = Vector3.one * scale;
            }
        }

        private void UpdateEyeLook()
        {
            if (currentTarget == null || eyeBall == null) return;

            // Calcular direção para o alvo
            Vector3 direction = (currentTarget.position - eyeBall.position).normalized;

            // Limitar ângulo de visão
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float angle = Quaternion.Angle(eyeBall.rotation, targetRotation);

            if (angle <= maxLookAngle)
            {
                eyeBall.rotation = Quaternion.Slerp(
                    eyeBall.rotation,
                    targetRotation,
                    Time.deltaTime * eyeLookSpeed
                );
            }
        }

        public void Blink()
        {
            if (eyeModel == null) return;

            // Animação de piscar (escala Y)
            eyeModel.transform.DOScaleY(0.1f, blinkDuration * 0.5f)
                .OnComplete(() =>
                {
                    eyeModel.transform.DOScaleY(originalEyeScale.y, blinkDuration * 0.5f);
                });

            PlaySound(blinkSound);
        }

        public void SetTarget(Transform target)
        {
            currentTarget = target;

            if (target != null)
            {
                // Efeito visual de aquisição de alvo
                if (scanEffect != null)
                {
                    scanEffect.transform.position = target.position;
                    scanEffect.Play();
                }

                // Mudar cor do olho
                SetEyeColor(alertColor);

                PlaySound(targetAcquiredSound);

                // Criar indicador de alvo
                CreateTargetIndicator(target);
            }
            else
            {
                SetEyeColor(normalColor);
            }
        }

        private void CreateTargetIndicator(Transform target)
        {
            if (targetIndicatorPrefab == null || targetIndicatorContainer == null) return;

            GameObject indicator = Instantiate(targetIndicatorPrefab, targetIndicatorContainer);
            var tracker = indicator.AddComponent<TargetTracker>();
            tracker.Initialize(target, mainCamera);
        }

        private void UpdateTargetTracking()
        {
            if (currentTarget == null) return;

            float distance = Vector3.Distance(transform.position, currentTarget.position);

            if (distance > maxTrackingDistance)
            {
                // Alvo muito longe, perder tracking
                SetTarget(null);
            }
        }

        public void StartScan()
        {
            if (isScanning) return;

            isScanning = true;

            // Efeito de scan
            if (scanEffect != null)
            {
                scanEffect.Play();
            }

            // Animação do olho
            if (eyeModel != null)
            {
                eyeModel.transform.DOScale(originalEyeScale * 1.2f, 0.5f)
                    .SetLoops(6, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        eyeModel.transform.localScale = originalEyeScale;
                        isScanning = false;
                    });
            }

            // Luz pulsante
            if (eyeLight != null)
            {
                eyeLight.DOIntensity(3f, 0.3f).SetLoops(6, LoopType.Yoyo);
            }

            PlaySound(scanSound);
        }

        public void UpdateMissionDisplay()
        {
            if (QuestSystem.Instance == null) return;

            var activeQuests = QuestSystem.Instance.GetActiveQuests();

            if (activeQuests.Count > 0)
            {
                var quest = activeQuests[0];

                if (missionTitleText != null)
                    missionTitleText.text = quest.questName;

                if (missionDescriptionText != null)
                    missionDescriptionText.text = quest.questDescription;

                if (missionProgressText != null)
                    missionProgressText.text = $"{quest.currentAmount} / {quest.targetAmount}";

                if (missionProgressBar != null)
                {
                    float progress = (float)quest.currentAmount / quest.targetAmount;
                    missionProgressBar.DOFillAmount(progress, 0.5f);
                }

                // Atualizar cor baseada no progresso
                float progressPercent = (float)quest.currentAmount / quest.targetAmount;
                if (progressPercent >= 1f)
                {
                    SetEyeColor(completeColor);
                }
                else if (progressPercent >= 0.5f)
                {
                    SetEyeColor(Color.Lerp(normalColor, completeColor, (progressPercent - 0.5f) * 2f));
                }
            }
            else
            {
                if (missionTitleText != null)
                    missionTitleText.text = "No Active Mission";

                if (missionDescriptionText != null)
                    missionDescriptionText.text = "Awaiting orders...";
            }
        }

        public void OnMissionComplete()
        {
            // Animação de conclusão
            if (eyeModel != null)
            {
                eyeModel.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f, 5);
            }

            SetEyeColor(completeColor);

            // Partículas de celebração
            if (scanEffect != null)
            {
                var main = scanEffect.main;
                main.startColor = completeColor;
                scanEffect.Play();
            }

            PlaySound(missionCompleteSound);

            // Piscar várias vezes
            for (int i = 0; i < 3; i++)
            {
                DOVirtual.DelayedCall(i * 0.3f, () => Blink());
            }
        }

        private void SetEyeColor(Color color)
        {
            // Mudar cor do material do olho
            if (eyeRenderer != null)
            {
                eyeRenderer.material.DOColor(color, 0.5f);
            }

            // Mudar cor da luz
            if (eyeLight != null)
            {
                eyeLight.DOColor(color, 0.5f);
            }

            // Mudar cor da barra de progresso
            if (missionProgressBar != null)
            {
                missionProgressBar.DOColor(color, 0.5f);
            }
        }

        public void ShowMissionPanel(bool show)
        {
            if (missionPanel == null) return;

            if (show)
            {
                missionPanel.SetActive(true);
                missionPanel.transform.localScale = Vector3.zero;
                missionPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }
            else
            {
                missionPanel.transform.DOScale(0f, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => missionPanel.SetActive(false));
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        // Classe auxiliar para rastreamento de alvos na UI
        private class TargetTracker : MonoBehaviour
        {
            private Transform target;
            private Camera cam;
            private RectTransform rectTransform;

            public void Initialize(Transform targetTransform, Camera camera)
            {
                target = targetTransform;
                cam = camera;
                rectTransform = GetComponent<RectTransform>();
            }

            private void Update()
            {
                if (target == null)
                {
                    Destroy(gameObject);
                    return;
                }

                // Converter posição 3D para posição na tela
                Vector3 screenPos = cam.WorldToScreenPoint(target.position);

                if (screenPos.z > 0)
                {
                    rectTransform.position = screenPos;
                }
                else
                {
                    // Alvo atrás da câmera
                    rectTransform.gameObject.SetActive(false);
                }
            }
        }
    }
}

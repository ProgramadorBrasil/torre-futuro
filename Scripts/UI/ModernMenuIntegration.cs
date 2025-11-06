using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

namespace SpaceRPG.UI
{
    /// <summary>
    /// Sistema de integração com 3D Modern Menu Asset
    /// Gerencia menus 3D com animações, efeitos e transições
    /// </summary>
    public class ModernMenuIntegration : MonoBehaviour
    {
        private static ModernMenuIntegration _instance;
        public static ModernMenuIntegration Instance => _instance;

        [Header("3D Menu Settings")]
        [SerializeField] private GameObject modernMenuPrefab;
        [SerializeField] private Camera menuCamera;
        [SerializeField] private Canvas menuCanvas;

        [Header("Menu Panels 3D")]
        [SerializeField] private Transform mainMenuRoot;
        [SerializeField] private Transform settingsMenuRoot;
        [SerializeField] private Transform shipSelectionMenuRoot;
        [SerializeField] private Transform loadoutMenuRoot;

        [Header("Animation Settings")]
        [SerializeField] private float menuTransitionDuration = 0.8f;
        [SerializeField] private Ease menuTransitionEase = Ease.OutQuart;
        [SerializeField] private float buttonHoverScale = 1.1f;
        [SerializeField] private float buttonClickScale = 0.95f;

        [Header("3D Button Elements")]
        [SerializeField] private GameObject[] menu3DButtons;
        [SerializeField] private Material buttonDefaultMaterial;
        [SerializeField] private Material buttonHoverMaterial;
        [SerializeField] private Material buttonActiveMaterial;

        [Header("Color Picker Integration")]
        [SerializeField] private FlexibleColorPicker colorPicker;
        [SerializeField] private Image colorPreview;
        [SerializeField] private TextMeshProUGUI colorHexText;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem menuParticles;
        [SerializeField] private Light menuAccentLight;
        [SerializeField] private GameObject neonEffects;

        [Header("Audio")]
        [SerializeField] private AudioClip menuOpenSound;
        [SerializeField] private AudioClip menuCloseSound;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip menuTransitionSound;

        private AudioSource menuAudioSource;
        private Transform currentActiveMenu;
        private int currentMenuIndex = 0;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeMenuSystem();
        }

        private void Start()
        {
            Setup3DMenus();
            SetupColorPicker();
            SetupMenuButtons();
        }

        private void InitializeMenuSystem()
        {
            menuAudioSource = gameObject.AddComponent<AudioSource>();
            menuAudioSource.playOnAwake = false;
            menuAudioSource.spatialBlend = 0f;

            if (menuCamera == null)
            {
                GameObject camObj = new GameObject("MenuCamera");
                menuCamera = camObj.AddComponent<Camera>();
                menuCamera.clearFlags = CameraClearFlags.Depth;
                menuCamera.cullingMask = LayerMask.GetMask("UI");
                menuCamera.depth = 10;
            }
        }

        private void Setup3DMenus()
        {
            if (mainMenuRoot != null)
            {
                SetupMenuPanel(mainMenuRoot, Vector3.zero);
                currentActiveMenu = mainMenuRoot;
            }

            if (settingsMenuRoot != null)
                SetupMenuPanel(settingsMenuRoot, new Vector3(1000, 0, 0));

            if (shipSelectionMenuRoot != null)
                SetupMenuPanel(shipSelectionMenuRoot, new Vector3(2000, 0, 0));

            if (loadoutMenuRoot != null)
                SetupMenuPanel(loadoutMenuRoot, new Vector3(3000, 0, 0));
        }

        private void SetupMenuPanel(Transform panel, Vector3 hiddenPosition)
        {
            panel.localPosition = hiddenPosition;
            panel.localScale = Vector3.one;

            // Adicionar efeitos de partículas ao menu
            if (menuParticles != null && hiddenPosition == Vector3.zero)
            {
                var particles = Instantiate(menuParticles, panel);
                particles.transform.localPosition = Vector3.zero;
                particles.Play();
            }
        }

        private void SetupColorPicker()
        {
            if (colorPicker != null)
            {
                colorPicker.onColorChanged.AddListener(OnColorChanged);
            }
        }

        private void SetupMenuButtons()
        {
            foreach (var button in menu3DButtons)
            {
                if (button == null) continue;

                var eventTrigger = button.GetComponent<EventTrigger>();
                if (eventTrigger == null)
                    eventTrigger = button.AddComponent<EventTrigger>();

                // Hover Enter
                EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
                hoverEntry.eventID = EventTriggerType.PointerEnter;
                hoverEntry.callback.AddListener((data) => OnButtonHover(button));
                eventTrigger.triggers.Add(hoverEntry);

                // Hover Exit
                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) => OnButtonExit(button));
                eventTrigger.triggers.Add(exitEntry);

                // Click
                EventTrigger.Entry clickEntry = new EventTrigger.Entry();
                clickEntry.eventID = EventTriggerType.PointerClick;
                clickEntry.callback.AddListener((data) => OnButtonClick(button));
                eventTrigger.triggers.Add(clickEntry);
            }
        }

        private void OnButtonHover(GameObject button)
        {
            // Animação de hover
            button.transform.DOScale(buttonHoverScale, 0.2f).SetEase(Ease.OutBack);

            // Trocar material
            var renderer = button.GetComponent<Renderer>();
            if (renderer != null && buttonHoverMaterial != null)
                renderer.material = buttonHoverMaterial;

            // Efeito de luz
            if (menuAccentLight != null)
            {
                menuAccentLight.DOIntensity(1.5f, 0.2f);
            }

            // Som
            PlayMenuSound(buttonHoverSound);
        }

        private void OnButtonExit(GameObject button)
        {
            button.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);

            var renderer = button.GetComponent<Renderer>();
            if (renderer != null && buttonDefaultMaterial != null)
                renderer.material = buttonDefaultMaterial;

            if (menuAccentLight != null)
            {
                menuAccentLight.DOIntensity(1f, 0.2f);
            }
        }

        private void OnButtonClick(GameObject button)
        {
            // Animação de click
            button.transform.DOScale(buttonClickScale, 0.1f)
                .OnComplete(() => button.transform.DOScale(1f, 0.1f));

            // Material ativo temporariamente
            var renderer = button.GetComponent<Renderer>();
            if (renderer != null && buttonActiveMaterial != null)
            {
                renderer.material = buttonActiveMaterial;
                DOVirtual.DelayedCall(0.2f, () => renderer.material = buttonDefaultMaterial);
            }

            PlayMenuSound(buttonClickSound);
        }

        public void TransitionToMenu(string menuName)
        {
            Transform targetMenu = null;

            switch (menuName.ToLower())
            {
                case "main":
                    targetMenu = mainMenuRoot;
                    break;
                case "settings":
                    targetMenu = settingsMenuRoot;
                    break;
                case "shipselection":
                    targetMenu = shipSelectionMenuRoot;
                    break;
                case "loadout":
                    targetMenu = loadoutMenuRoot;
                    break;
            }

            if (targetMenu != null && currentActiveMenu != targetMenu)
            {
                TransitionMenus(currentActiveMenu, targetMenu);
            }
        }

        private void TransitionMenus(Transform from, Transform to)
        {
            PlayMenuSound(menuTransitionSound);

            // Animar menu atual saindo
            if (from != null)
            {
                from.DOLocalMove(new Vector3(-1000, 0, 0), menuTransitionDuration)
                    .SetEase(menuTransitionEase);
                from.DOScale(0.8f, menuTransitionDuration);
            }

            // Animar novo menu entrando
            if (to != null)
            {
                to.localPosition = new Vector3(1000, 0, 0);
                to.localScale = Vector3.one * 0.8f;

                to.DOLocalMove(Vector3.zero, menuTransitionDuration)
                    .SetEase(menuTransitionEase);
                to.DOScale(1f, menuTransitionDuration);
            }

            currentActiveMenu = to;
        }

        public void OpenMenu3D()
        {
            if (currentActiveMenu != null)
            {
                currentActiveMenu.localScale = Vector3.zero;
                currentActiveMenu.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

                PlayMenuSound(menuOpenSound);

                if (menuParticles != null)
                    menuParticles.Play();

                if (neonEffects != null)
                    neonEffects.SetActive(true);
            }
        }

        public void CloseMenu3D()
        {
            if (currentActiveMenu != null)
            {
                currentActiveMenu.DOScale(0f, 0.3f).SetEase(Ease.InBack);
                PlayMenuSound(menuCloseSound);

                if (neonEffects != null)
                    neonEffects.SetActive(false);
            }
        }

        private void OnColorChanged(Color color)
        {
            if (colorPreview != null)
                colorPreview.color = color;

            if (colorHexText != null)
                colorHexText.text = "#" + ColorUtility.ToHtmlStringRGB(color);

            // Aplicar cor aos elementos do menu
            ApplyColorToMenuElements(color);
        }

        private void ApplyColorToMenuElements(Color color)
        {
            // Aplicar aos botões 3D
            foreach (var button in menu3DButtons)
            {
                if (button == null) continue;

                var renderer = button.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetColor("_EmissionColor", color * 2f);
                }
            }

            // Aplicar à luz de acento
            if (menuAccentLight != null)
            {
                menuAccentLight.color = color;
            }

            // Aplicar aos efeitos neon
            if (neonEffects != null)
            {
                var neonRenderers = neonEffects.GetComponentsInChildren<Renderer>();
                foreach (var renderer in neonRenderers)
                {
                    renderer.material.SetColor("_EmissionColor", color * 3f);
                }
            }
        }

        private void PlayMenuSound(AudioClip clip)
        {
            if (clip != null && menuAudioSource != null)
            {
                menuAudioSource.PlayOneShot(clip);
            }
        }

        public void RotateMenuCamera(float angle)
        {
            if (menuCamera != null)
            {
                menuCamera.transform.DORotate(new Vector3(0, angle, 0), 1f)
                    .SetEase(Ease.InOutSine);
            }
        }

        public void SetMenuCameraDistance(float distance)
        {
            if (menuCamera != null)
            {
                menuCamera.transform.DOLocalMove(new Vector3(0, 0, -distance), 0.8f)
                    .SetEase(Ease.OutQuart);
            }
        }

        private void OnDestroy()
        {
            // Cleanup DOTween
            DOTween.Kill(this);
        }
    }
}

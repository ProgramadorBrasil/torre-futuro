using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SpaceRPG.Core;

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
        [SerializeField] private TweenHelper.EaseType menuTransitionEase = TweenHelper.EaseType.OutQuart;
        [SerializeField] private float buttonHoverScale = 1.1f;
        [SerializeField] private float buttonClickScale = 0.95f;

        [Header("3D Button Elements")]
        [SerializeField] private GameObject[] menu3DButtons;
        [SerializeField] private Material buttonDefaultMaterial;
        [SerializeField] private Material buttonHoverMaterial;
        [SerializeField] private Material buttonActiveMaterial;

        [Header("Color Picker Integration")]
        [SerializeField] private Slider redSlider;
        [SerializeField] private Slider greenSlider;
        [SerializeField] private Slider blueSlider;
        [SerializeField] private Image colorPreview;
        [SerializeField] private TextMeshProUGUI colorHexText;
        private Color currentColor = Color.white;

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
            // Setup simple color picker com sliders RGB
            if (redSlider != null)
                redSlider.onValueChanged.AddListener((value) => UpdateColorFromSliders());

            if (greenSlider != null)
                greenSlider.onValueChanged.AddListener((value) => UpdateColorFromSliders());

            if (blueSlider != null)
                blueSlider.onValueChanged.AddListener((value) => UpdateColorFromSliders());
        }

        private void UpdateColorFromSliders()
        {
            float r = redSlider != null ? redSlider.value : 1f;
            float g = greenSlider != null ? greenSlider.value : 1f;
            float b = blueSlider != null ? blueSlider.value : 1f;

            currentColor = new Color(r, g, b);
            OnColorChanged(currentColor);
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
            StartCoroutine(TweenHelper.AnimateScale(button.transform, Vector3.one * buttonHoverScale, 0.2f, TweenHelper.EaseType.OutBack));

            // Trocar material
            var renderer = button.GetComponent<Renderer>();
            if (renderer != null && buttonHoverMaterial != null)
                renderer.material = buttonHoverMaterial;

            // Efeito de luz
            if (menuAccentLight != null)
            {
                StartCoroutine(TweenHelper.AnimateLightIntensity(menuAccentLight, 1.5f, 0.2f));
            }

            // Som
            PlayMenuSound(buttonHoverSound);
        }

        private void OnButtonExit(GameObject button)
        {
            StartCoroutine(TweenHelper.AnimateScale(button.transform, Vector3.one, 0.2f, TweenHelper.EaseType.OutBack));

            var renderer = button.GetComponent<Renderer>();
            if (renderer != null && buttonDefaultMaterial != null)
                renderer.material = buttonDefaultMaterial;

            if (menuAccentLight != null)
            {
                StartCoroutine(TweenHelper.AnimateLightIntensity(menuAccentLight, 1f, 0.2f));
            }
        }

        private void OnButtonClick(GameObject button)
        {
            // Animação de click
            StartCoroutine(ButtonClickAnimation(button));

            // Material ativo temporariamente
            var renderer = button.GetComponent<Renderer>();
            if (renderer != null && buttonActiveMaterial != null)
            {
                renderer.material = buttonActiveMaterial;
                StartCoroutine(TweenHelper.DelayedCall(0.2f, () =>
                {
                    if (renderer != null)
                        renderer.material = buttonDefaultMaterial;
                }));
            }

            PlayMenuSound(buttonClickSound);
        }

        private System.Collections.IEnumerator ButtonClickAnimation(GameObject button)
        {
            yield return TweenHelper.AnimateScale(button.transform, Vector3.one * buttonClickScale, 0.1f);
            yield return TweenHelper.AnimateScale(button.transform, Vector3.one, 0.1f);
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
                StartCoroutine(AnimateMenuOut(from));
            }

            // Animar novo menu entrando
            if (to != null)
            {
                StartCoroutine(AnimateMenuIn(to));
            }

            currentActiveMenu = to;
        }

        private System.Collections.IEnumerator AnimateMenuOut(Transform menu)
        {
            StartCoroutine(TweenHelper.AnimatePosition(menu, menu.position + new Vector3(-1000, 0, 0), menuTransitionDuration, menuTransitionEase));
            yield return TweenHelper.AnimateScale(menu, Vector3.one * 0.8f, menuTransitionDuration);
        }

        private System.Collections.IEnumerator AnimateMenuIn(Transform menu)
        {
            menu.localPosition = new Vector3(1000, 0, 0);
            menu.localScale = Vector3.one * 0.8f;

            StartCoroutine(TweenHelper.AnimatePosition(menu, menu.parent.TransformPoint(Vector3.zero), menuTransitionDuration, menuTransitionEase));
            yield return TweenHelper.AnimateScale(menu, Vector3.one, menuTransitionDuration);
        }

        public void OpenMenu3D()
        {
            if (currentActiveMenu != null)
            {
                currentActiveMenu.localScale = Vector3.zero;
                StartCoroutine(TweenHelper.AnimateScale(currentActiveMenu, Vector3.one, 0.5f, TweenHelper.EaseType.OutBack));

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
                StartCoroutine(TweenHelper.AnimateScale(currentActiveMenu, Vector3.zero, 0.3f, TweenHelper.EaseType.InBack));
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
                StartCoroutine(TweenHelper.AnimateRotation(menuCamera.transform, new Vector3(0, angle, 0), 1f));
            }
        }

        public void SetMenuCameraDistance(float distance)
        {
            if (menuCamera != null)
            {
                Vector3 targetPos = new Vector3(0, 0, -distance);
                StartCoroutine(TweenHelper.AnimatePosition(menuCamera.transform, menuCamera.transform.parent.TransformPoint(targetPos), 0.8f, TweenHelper.EaseType.OutQuart));
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}

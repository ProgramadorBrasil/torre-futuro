using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SpaceRPG.Core;
using SpaceRPG.Systems;

namespace SpaceRPG.UI
{
    public class MenuManager : MonoBehaviour
    {
        private static MenuManager _instance;
        public static MenuManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MenuManager>();
                }
                return _instance;
            }
        }

        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Pause Menu")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveGameButton;
        [SerializeField] private Button pauseSettingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button pauseQuitButton;

        [Header("Settings Menu")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button backButton;

        [Header("HUD Elements")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image healthBar;
        [SerializeField] private TextMeshProUGUI questTrackerText;
        [SerializeField] private GameObject minimap;

        [Header("Notification")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 3f;

        [Header("Loading Screen")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private Image loadingBar;
        [SerializeField] private TextMeshProUGUI loadingText;

        private bool isPaused = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeMenus();
            SetupListeners();
        }

        private void Update()
        {
            // Toggle pause com ESC
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }

            UpdateHUD();
        }

        private void InitializeMenus()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (loadingScreen != null) loadingScreen.SetActive(false);
            if (notificationPanel != null) notificationPanel.SetActive(false);

            if (hudPanel != null) hudPanel.SetActive(true);
        }

        private void SetupListeners()
        {
            // Main Menu
            if (startGameButton != null)
                startGameButton.onClick.AddListener(StartNewGame);

            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(LoadGame);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);

            // Pause Menu
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);

            if (saveGameButton != null)
                saveGameButton.onClick.AddListener(SaveGame);

            if (pauseSettingsButton != null)
                pauseSettingsButton.onClick.AddListener(OpenSettings);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);

            if (pauseQuitButton != null)
                pauseQuitButton.onClick.AddListener(QuitGame);

            // Settings
            if (backButton != null)
                backButton.onClick.AddListener(CloseSettings);

            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

            if (qualityDropdown != null)
                qualityDropdown.onValueChanged.AddListener(SetQuality);

            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        public void StartNewGame()
        {
            ShowLoadingScreen();
            SceneManager.LoadScene("GameScene");
            HideLoadingScreen();
            ShowNotification("New game started!");
        }

        public void LoadGame()
        {
            if (SaveLoadSystem.Instance != null)
            {
                if (SaveLoadSystem.Instance.LoadGame())
                {
                    ShowNotification("Game loaded successfully!");
                }
                else
                {
                    ShowNotification("Failed to load game!");
                }
            }
        }

        public void SaveGame()
        {
            if (SaveLoadSystem.Instance != null)
            {
                if (SaveLoadSystem.Instance.SaveGame())
                {
                    ShowNotification("Game saved successfully!");
                }
                else
                {
                    ShowNotification("Failed to save game!");
                }
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);

            if (hudPanel != null)
                hudPanel.SetActive(false);
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            if (hudPanel != null)
                hudPanel.SetActive(true);
        }

        public void OpenSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);

            LoadSettingsValues();
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void LoadSettingsValues()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);

            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = Screen.fullScreen;
        }

        private void SetMasterVolume(float volume)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMasterVolume(volume);

            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        private void SetMusicVolume(float volume)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMusicVolume(volume);

            PlayerPrefs.SetFloat("MusicVolume", volume);
        }

        private void SetSFXVolume(float volume)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSFXVolume(volume);

            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        private void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        }

        private void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void UpdateHUD()
        {
            if (!hudPanel.activeSelf) return;

            // Atualizar créditos
            if (creditsText != null && InventorySystem.Instance != null)
            {
                creditsText.text = $"{InventorySystem.Instance.GetCurrentCredits()} ¢";
            }

            // Atualizar saúde da nave
            if (ShipSystem.Instance != null)
            {
                float health = ShipSystem.Instance.GetCurrentHealth();
                float maxHealth = ShipSystem.Instance.GetCurrentShip()?.maxHealth ?? 100f;

                if (healthText != null)
                    healthText.text = $"{health:F0}/{maxHealth:F0}";

                if (healthBar != null)
                    healthBar.fillAmount = health / maxHealth;
            }

            // Atualizar quest tracker
            if (questTrackerText != null && QuestSystem.Instance != null)
            {
                var activeQuests = QuestSystem.Instance.GetActiveQuests();
                if (activeQuests.Count > 0)
                {
                    var quest = activeQuests[0];
                    questTrackerText.text = $"{quest.questName}\n{quest.currentAmount}/{quest.targetAmount}";
                }
                else
                {
                    questTrackerText.text = "No active quests";
                }
            }
        }

        public void ShowNotification(string message)
        {
            if (notificationPanel == null || notificationText == null) return;

            notificationText.text = message;
            notificationPanel.SetActive(true);

            CancelInvoke(nameof(HideNotification));
            Invoke(nameof(HideNotification), notificationDuration);
        }

        private void HideNotification()
        {
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

        public void ShowLoadingScreen(string message = "Loading...")
        {
            if (loadingScreen == null) return;

            loadingScreen.SetActive(true);

            if (loadingText != null)
                loadingText.text = message;

            if (loadingBar != null)
                loadingBar.fillAmount = 0f;
        }

        public void UpdateLoadingProgress(float progress)
        {
            if (loadingBar != null)
                loadingBar.fillAmount = progress;
        }

        public void HideLoadingScreen()
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }

        public bool IsPaused() => isPaused;
        public void ShowHUD() { if (hudPanel != null) hudPanel.SetActive(true); }
        public void HideHUD() { if (hudPanel != null) hudPanel.SetActive(false); }
    }
}

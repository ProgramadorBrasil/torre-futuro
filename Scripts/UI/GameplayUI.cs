using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace SpaceRPG.UI
{
    /// <summary>
    /// Comprehensive Gameplay UI with real-time HUD, menus, notifications, and all visual feedback
    /// Integrates with all game systems to provide complete player interface
    /// </summary>
    public class GameplayUI : MonoBehaviour
{
    #region HUD Elements
    [Header("Health & Shields")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Slider armorBar;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private Gradient healthColorGradient;

    [Header("Energy & Fuel")]
    [SerializeField] private Slider energyBar;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Slider fuelBar;
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private GameObject lowFuelWarning;

    [Header("Weapon Info")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Slider heatBar;
    [SerializeField] private GameObject overheatedWarning;
    [SerializeField] private GameObject reloadingIndicator;

    [Header("Speed & Movement")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Image speedometer;
    [SerializeField] private GameObject boostIndicator;

    [Header("Score & Resources")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider xpBar;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Minimap")]
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RectTransform minimapPlayerIcon;
    [SerializeField] private GameObject minimapPanel;

    [Header("Crosshair")]
    [SerializeField] private Image crosshair;
    [SerializeField] private Color normalCrosshairColor = Color.white;
    [SerializeField] private Color enemyTargetColor = Color.red;
    [SerializeField] private Color friendlyTargetColor = Color.green;

    [Header("Targeting")]
    [SerializeField] private GameObject targetReticle;
    [SerializeField] private TextMeshProUGUI targetNameText;
    [SerializeField] private Slider targetHealthBar;

    [Header("Combo & Streak")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private GameObject comboPanel;
    [SerializeField] private TextMeshProUGUI streakText;
    [SerializeField] private GameObject streakPanel;

    [Header("Notifications")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationTitleText;
    [SerializeField] private TextMeshProUGUI notificationBodyText;
    [SerializeField] private Image notificationIcon;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueSpeakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image dialoguePortrait;

    [Header("Reward Popups")]
    [SerializeField] private GameObject rewardPopupPrefab;
    [SerializeField] private Transform rewardPopupParent;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Upgrade Menu")]
    [SerializeField] private GameObject upgradeMenuPanel;
    [SerializeField] private Transform upgradeListParent;
    [SerializeField] private GameObject upgradeItemPrefab;

    [Header("Inventory")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("Planting UI")]
    [SerializeField] private GameObject plantingPanel;
    [SerializeField] private TextMeshProUGUI selectedPlantText;
    [SerializeField] private TextMeshProUGUI plantCostText;
    [SerializeField] private TextMeshProUGUI activePlantsText;

    [Header("Objectives")]
    [SerializeField] private GameObject objectivesPanel;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private Slider objectiveProgressBar;

    [Header("Damage Indicator")]
    [SerializeField] private GameObject damageVignettePanel;
    [SerializeField] private Image damageVignetteImage;

    [Header("Achievement Popup")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private TextMeshProUGUI achievementNameText;
    [SerializeField] private TextMeshProUGUI achievementDescText;
    [SerializeField] private Image achievementIcon;
    #endregion

    // Private variables
    private SpaceshipController spaceshipController;
    private WeaponSystem weaponSystem;
    private UpgradeSystem upgradeSystem;
    private RewardSystem rewardSystem;
    private PlantingSystem plantingSystem;
    private bool isPaused = false;
    private Coroutine damageFlashCoroutine;

    #region Initialization

    private void Awake()
    {
        FindReferences();
        InitializeUI();
    }

    private void Start()
    {
        SetupEventListeners();
        HideAllMenus();
        UpdateAllUI();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void FindReferences()
    {
        if (spaceshipController == null)
            spaceshipController = FindObjectOfType<SpaceshipController>();

        if (weaponSystem == null)
            weaponSystem = FindObjectOfType<WeaponSystem>();

        if (upgradeSystem == null)
            upgradeSystem = FindObjectOfType<UpgradeSystem>();

        if (rewardSystem == null)
            rewardSystem = FindObjectOfType<RewardSystem>();

        if (plantingSystem == null)
            plantingSystem = FindObjectOfType<PlantingSystem>();
    }

    private void InitializeUI()
    {
        // Initialize bars
        if (healthBar != null) healthBar.value = 1f;
        if (armorBar != null) armorBar.value = 1f;
        if (energyBar != null) energyBar.value = 1f;
        if (fuelBar != null) fuelBar.value = 1f;
        if (xpBar != null) xpBar.value = 0f;

        // Hide warnings
        if (lowFuelWarning != null) lowFuelWarning.SetActive(false);
        if (overheatedWarning != null) overheatedWarning.SetActive(false);
        if (reloadingIndicator != null) reloadingIndicator.SetActive(false);
        if (boostIndicator != null) boostIndicator.SetActive(false);

        // Hide panels
        if (comboPanel != null) comboPanel.SetActive(false);
        if (streakPanel != null) streakPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (targetReticle != null) targetReticle.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(false);

        // Setup buttons
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void SetupEventListeners()
    {
        if (spaceshipController != null)
        {
            spaceshipController.OnHealthChanged += UpdateHealthBar;
            spaceshipController.OnEnergyChanged += UpdateEnergyBar;
            spaceshipController.OnFuelChanged += UpdateFuelBar;
            spaceshipController.OnArmorChanged += UpdateArmorBar;
            spaceshipController.OnSpaceshipDestroyed += OnPlayerDeath;
        }

        if (weaponSystem != null)
        {
            weaponSystem.OnAmmoChanged += UpdateAmmoDisplay;
            weaponSystem.OnWeaponSwitched += UpdateWeaponDisplay;
            weaponSystem.OnReloadStarted += ShowReloadingIndicator;
            weaponSystem.OnReloadCompleted += HideReloadingIndicator;
        }

        if (upgradeSystem != null)
        {
            upgradeSystem.OnCreditsChanged += UpdateCreditsDisplay;
            upgradeSystem.OnXPChanged += UpdateXPDisplay;
            upgradeSystem.OnLevelUp += ShowLevelUpNotification;
        }

        if (rewardSystem != null)
        {
            rewardSystem.OnStreakChanged += UpdateStreakDisplay;
            rewardSystem.OnComboChanged += UpdateComboDisplay;
            rewardSystem.OnAchievementUnlocked += ShowAchievementUnlocked;
        }

        if (plantingSystem != null)
        {
            plantingSystem.OnPlantCountChanged += UpdatePlantCount;
        }
    }

    private void UnsubscribeEvents()
    {
        if (spaceshipController != null)
        {
            spaceshipController.OnHealthChanged -= UpdateHealthBar;
            spaceshipController.OnEnergyChanged -= UpdateEnergyBar;
            spaceshipController.OnFuelChanged -= UpdateFuelBar;
            spaceshipController.OnArmorChanged -= UpdateArmorBar;
            spaceshipController.OnSpaceshipDestroyed -= OnPlayerDeath;
        }

        if (weaponSystem != null)
        {
            weaponSystem.OnAmmoChanged -= UpdateAmmoDisplay;
            weaponSystem.OnWeaponSwitched -= UpdateWeaponDisplay;
            weaponSystem.OnReloadStarted -= ShowReloadingIndicator;
            weaponSystem.OnReloadCompleted -= HideReloadingIndicator;
        }

        if (upgradeSystem != null)
        {
            upgradeSystem.OnCreditsChanged -= UpdateCreditsDisplay;
            upgradeSystem.OnXPChanged -= UpdateXPDisplay;
            upgradeSystem.OnLevelUp -= ShowLevelUpNotification;
        }

        if (rewardSystem != null)
        {
            rewardSystem.OnStreakChanged -= UpdateStreakDisplay;
            rewardSystem.OnComboChanged -= UpdateComboDisplay;
            rewardSystem.OnAchievementUnlocked -= ShowAchievementUnlocked;
        }

        if (plantingSystem != null)
        {
            plantingSystem.OnPlantCountChanged -= UpdatePlantCount;
        }
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        HandleInput();
        UpdateSpeedometer();
        UpdateMinimap();
    }

    private void HandleInput()
    {
        // Pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        // Upgrade menu (Tab key)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleUpgradeMenu();
        }

        // Inventory (I key)
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    #endregion

    #region Health & Status Updates

    private void UpdateHealthBar(float percentage)
    {
        if (healthBar != null)
        {
            healthBar.value = percentage;

            if (healthBarFill != null && healthColorGradient != null)
            {
                healthBarFill.color = healthColorGradient.Evaluate(percentage);
            }
        }

        if (healthText != null && spaceshipController != null)
        {
            float currentHealth = spaceshipController.GetHealthPercentage() * 100f;
            healthText.text = $"{currentHealth:F0}%";
        }

        // Damage vignette effect
        if (percentage < 0.3f && damageFlashCoroutine == null)
        {
            damageFlashCoroutine = StartCoroutine(PulseDamageVignette());
        }
        else if (percentage >= 0.3f && damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = null;
            if (damageVignettePanel != null)
                damageVignettePanel.SetActive(false);
        }
    }

    private void UpdateArmorBar(float percentage)
    {
        if (armorBar != null)
        {
            armorBar.value = percentage;
        }

        if (armorText != null)
        {
            armorText.text = $"{percentage * 100f:F0}%";
        }
    }

    private void UpdateEnergyBar(float percentage)
    {
        if (energyBar != null)
        {
            energyBar.value = percentage;
        }

        if (energyText != null)
        {
            energyText.text = $"{percentage * 100f:F0}%";
        }
    }

    private void UpdateFuelBar(float percentage)
    {
        if (fuelBar != null)
        {
            fuelBar.value = percentage;
        }

        if (fuelText != null)
        {
            fuelText.text = $"{percentage * 100f:F0}%";
        }

        // Low fuel warning
        if (lowFuelWarning != null)
        {
            lowFuelWarning.SetActive(percentage < 0.2f);
        }
    }

    #endregion

    #region Weapon Display

    private void UpdateAmmoDisplay(WeaponSystem.WeaponType type, int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{current} / {max}";
        }
    }

    private void UpdateWeaponDisplay(WeaponSystem.WeaponType type, int current, int max)
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = type.ToString();
        }

        UpdateAmmoDisplay(type, current, max);
    }

    private void ShowReloadingIndicator()
    {
        if (reloadingIndicator != null)
        {
            reloadingIndicator.SetActive(true);
        }
    }

    private void HideReloadingIndicator()
    {
        if (reloadingIndicator != null)
        {
            reloadingIndicator.SetActive(false);
        }
    }

    #endregion

    #region Speed & Movement

    private void UpdateSpeedometer()
    {
        if (spaceshipController == null) return;

        float speed = spaceshipController.GetCurrentSpeed();

        if (speedText != null)
        {
            speedText.text = $"{speed:F0} m/s";
        }

        if (speedometer != null)
        {
            speedometer.fillAmount = speed / 100f; // Assuming max 100 m/s for display
        }

        // Boost indicator
        if (boostIndicator != null)
        {
            bool isBoosting = Input.GetKey(KeyCode.LeftShift);
            boostIndicator.SetActive(isBoosting && spaceshipController.HasEnergy(10f));
        }
    }

    #endregion

    #region Resources Display

    private void UpdateCreditsDisplay(int credits)
    {
        if (creditsText != null)
        {
            creditsText.text = $"Credits: {credits:N0}";
        }
    }

    private void UpdateXPDisplay(int xp)
    {
        if (xpText != null && upgradeSystem != null)
        {
            int nextLevel = upgradeSystem.GetXPForNextLevel();
            xpText.text = $"XP: {xp} / {nextLevel}";

            if (xpBar != null)
            {
                xpBar.value = (float)xp / nextLevel;
            }
        }
    }

    private void ShowLevelUpNotification(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }

        ShowNotification("LEVEL UP!", $"You reached level {level}!");
    }

    #endregion

    #region Combo & Streak

    public void ShowCombo(int combo)
    {
        if (comboPanel != null)
        {
            comboPanel.SetActive(true);
        }

        if (comboText != null)
        {
            comboText.text = $"x{combo} COMBO!";
        }

        StartCoroutine(HideComboAfterDelay(2f));
    }

    private void UpdateComboDisplay(int combo, int multiplier)
    {
        if (combo > 1)
        {
            ShowCombo(combo);
        }
    }

    private IEnumerator HideComboAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
        }
    }

    private void UpdateStreakDisplay(int streak)
    {
        if (streakPanel != null)
        {
            streakPanel.SetActive(streak > 0);
        }

        if (streakText != null)
        {
            streakText.text = $"{streak} Kill Streak";
        }
    }

    public void ShowStreakNotification(int streak)
    {
        ShowNotification("KILL STREAK!", $"{streak} kills in a row!");
    }

    #endregion

    #region Dialogue System

    public void ShowDialogue(string speaker, string text)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (dialogueSpeakerText != null)
        {
            dialogueSpeakerText.text = speaker;
        }

        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    #endregion

    #region Notifications

    public void ShowNotification(string title, string body)
    {
        StartCoroutine(ShowNotificationCoroutine(title, body, 3f));
    }

    private IEnumerator ShowNotificationCoroutine(string title, string body, float duration)
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }

        if (notificationTitleText != null)
        {
            notificationTitleText.text = title;
        }

        if (notificationBodyText != null)
        {
            notificationBodyText.text = body;
        }

        yield return new WaitForSeconds(duration);

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    public void ShowRewardPopup(string text, Color color)
    {
        if (rewardPopupPrefab != null && rewardPopupParent != null)
        {
            GameObject popup = Instantiate(rewardPopupPrefab, rewardPopupParent);
            TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
            if (popupText != null)
            {
                popupText.text = text;
                popupText.color = color;
            }
            Destroy(popup, 2f);
        }
    }

    #endregion

    #region Achievements

    public void ShowAchievement(string name, string description)
    {
        StartCoroutine(ShowAchievementCoroutine(name, description));
    }

    private void ShowAchievementUnlocked(RewardSystem.Achievement achievement)
    {
        ShowAchievement(achievement.name, achievement.description);
    }

    private IEnumerator ShowAchievementCoroutine(string name, string description)
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);
        }

        if (achievementNameText != null)
        {
            achievementNameText.text = name;
        }

        if (achievementDescText != null)
        {
            achievementDescText.text = description;
        }

        yield return new WaitForSeconds(4f);

        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }
    }

    #endregion

    #region Minimap

    private void UpdateMinimap()
    {
        if (minimapCamera == null || minimapPlayerIcon == null) return;

        // Update player icon rotation
        if (spaceshipController != null)
        {
            minimapPlayerIcon.rotation = Quaternion.Euler(0, 0, -spaceshipController.transform.eulerAngles.y);
        }
    }

    #endregion

    #region Planting UI

    private void UpdatePlantCount(int count)
    {
        if (activePlantsText != null && plantingSystem != null)
        {
            activePlantsText.text = $"Plants: {count} / {plantingSystem.GetMaxPlants()}";
        }
    }

    public void UpdatePlantingUI(string plantName, int cost)
    {
        if (selectedPlantText != null)
        {
            selectedPlantText.text = plantName;
        }

        if (plantCostText != null)
        {
            plantCostText.text = $"Cost: {cost} credits";
        }

        if (plantingPanel != null && plantingSystem != null)
        {
            plantingPanel.SetActive(plantingSystem.IsPlantingMode());
        }
    }

    #endregion

    #region Menus

    private void TogglePauseMenu()
    {
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    private void ToggleUpgradeMenu()
    {
        if (upgradeMenuPanel != null)
        {
            bool isActive = upgradeMenuPanel.activeSelf;
            upgradeMenuPanel.SetActive(!isActive);

            if (!isActive)
            {
                PopulateUpgradeMenu();
            }
        }
    }

    private void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);
        }
    }

    private void HideAllMenus()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (upgradeMenuPanel != null) upgradeMenuPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    private void ResumeGame()
    {
        TogglePauseMenu();
    }

    private void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    #endregion

    #region Upgrade Menu

    private void PopulateUpgradeMenu()
    {
        if (upgradeSystem == null || upgradeListParent == null) return;

        // Clear existing items
        foreach (Transform child in upgradeListParent)
        {
            Destroy(child.gameObject);
        }

        // Create upgrade items
        var upgrades = upgradeSystem.GetAllUpgrades();
        foreach (var upgrade in upgrades)
        {
            if (upgradeItemPrefab != null)
            {
                GameObject item = Instantiate(upgradeItemPrefab, upgradeListParent);
                // Populate item with upgrade data
                // This would need custom UpgradeItemUI component
            }
        }
    }

    #endregion

    #region Visual Effects

    private IEnumerator PulseDamageVignette()
    {
        if (damageVignettePanel == null) yield break;

        damageVignettePanel.SetActive(true);

        while (true)
        {
            float alpha = Mathf.PingPong(Time.time * 2f, 0.3f);
            if (damageVignetteImage != null)
            {
                Color color = damageVignetteImage.color;
                color.a = alpha;
                damageVignetteImage.color = color;
            }

            yield return null;
        }
    }

    private void OnPlayerDeath()
    {
        ShowNotification("DESTROYED", "Press R to respawn");
    }

    #endregion

    #region Public Helpers

    public void UpdateAllUI()
    {
        if (spaceshipController != null)
        {
            UpdateHealthBar(spaceshipController.GetHealthPercentage());
            UpdateEnergyBar(spaceshipController.GetEnergyPercentage());
            UpdateFuelBar(spaceshipController.GetFuelPercentage());
            UpdateArmorBar(spaceshipController.GetArmorPercentage());
        }

        if (upgradeSystem != null)
        {
            UpdateCreditsDisplay(upgradeSystem.GetCredits());
            UpdateXPDisplay(upgradeSystem.GetXP());
        }
    }

    public void SetCrosshairColor(Color color)
    {
        if (crosshair != null)
        {
            crosshair.color = color;
        }
    }

    public void ShowTargetReticle(Transform target, string targetName, float healthPercent)
    {
        if (targetReticle != null)
        {
            targetReticle.SetActive(true);
        }

        if (targetNameText != null)
        {
            targetNameText.text = targetName;
        }

        if (targetHealthBar != null)
        {
            targetHealthBar.value = healthPercent;
        }
    }

    public void HideTargetReticle()
    {
        if (targetReticle != null)
        {
            targetReticle.SetActive(false);
        }
    }

    #endregion
}
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using SpaceRPG.Data;
using SpaceRPG.Systems;
using UnityEngine.EventSystems;

namespace SpaceRPG.UI
{
    /// <summary>
    /// UI completa do inventário com drag & drop, filtros e categorias
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Button closeButton;

        [Header("Category Tabs")]
        [SerializeField] private Button allItemsTab;
        [SerializeField] private Button weaponsTab;
        [SerializeField] private Button shipPartsTab;
        [SerializeField] private Button consumablesTab;
        [SerializeField] private Button questItemsTab;
        [SerializeField] private Button equippedTab;

        [Header("Item Grid")]
        [SerializeField] private Transform itemGridParent;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private int gridColumns = 6;

        [Header("Item Details")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailDescription;
        [SerializeField] private TextMeshProUGUI detailStats;
        [SerializeField] private Button useButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button dropButton;
        [SerializeField] private Button sellButton;

        [Header("Search & Filter")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private TMP_Dropdown rarityFilterDropdown;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI weightText;
        [SerializeField] private TextMeshProUGUI slotsText;
        [SerializeField] private Image weightBar;

        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip itemClickSound;
        [SerializeField] private AudioClip itemDragSound;
        [SerializeField] private AudioClip itemDropSound;

        // Estado interno
        private List<InventorySlot> activeSlots = new List<InventorySlot>();
        private InventoryItem selectedItem;
        private ItemType currentCategory = ItemType.Weapon; // Default
        private bool showAllCategories = true;

        private void Start()
        {
            InitializeUI();
            SetupListeners();
            CloseInventory();
        }

        private void OnEnable()
        {
            // Subscrever aos eventos do sistema de inventário
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnInventoryChanged += RefreshInventory;
                InventorySystem.Instance.OnCreditsChanged += UpdateCreditsDisplay;
            }
        }

        private void OnDisable()
        {
            // Desinscrever dos eventos
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnInventoryChanged -= RefreshInventory;
                InventorySystem.Instance.OnCreditsChanged -= UpdateCreditsDisplay;
            }
        }

        private void Update()
        {
            // Toggle inventário com TAB
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventory();
            }

            // Atalhos numéricos para categorias
            if (inventoryPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) ShowAllItems();
                if (Input.GetKeyDown(KeyCode.Alpha2)) ShowWeapons();
                if (Input.GetKeyDown(KeyCode.Alpha3)) ShowShipParts();
                if (Input.GetKeyDown(KeyCode.Alpha4)) ShowConsumables();
                if (Input.GetKeyDown(KeyCode.Alpha5)) ShowQuestItems();
                if (Input.GetKeyDown(KeyCode.Alpha6)) ShowEquipped();
            }
        }

        /// <summary>
        /// Inicializa a UI
        /// </summary>
        private void InitializeUI()
        {
            if (detailsPanel != null)
                detailsPanel.SetActive(false);

            UpdateStatsDisplay();
            PopulateSortDropdown();
            PopulateRarityFilterDropdown();
        }

        /// <summary>
        /// Configura os listeners dos botões
        /// </summary>
        private void SetupListeners()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseInventory);

            if (allItemsTab != null)
                allItemsTab.onClick.AddListener(ShowAllItems);

            if (weaponsTab != null)
                weaponsTab.onClick.AddListener(ShowWeapons);

            if (shipPartsTab != null)
                shipPartsTab.onClick.AddListener(ShowShipParts);

            if (consumablesTab != null)
                consumablesTab.onClick.AddListener(ShowConsumables);

            if (questItemsTab != null)
                questItemsTab.onClick.AddListener(ShowQuestItems);

            if (equippedTab != null)
                equippedTab.onClick.AddListener(ShowEquipped);

            if (useButton != null)
                useButton.onClick.AddListener(UseSelectedItem);

            if (equipButton != null)
                equipButton.onClick.AddListener(EquipSelectedItem);

            if (dropButton != null)
                dropButton.onClick.AddListener(DropSelectedItem);

            if (sellButton != null)
                sellButton.onClick.AddListener(SellSelectedItem);

            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);

            if (sortDropdown != null)
                sortDropdown.onValueChanged.AddListener(OnSortChanged);

            if (rarityFilterDropdown != null)
                rarityFilterDropdown.onValueChanged.AddListener(OnRarityFilterChanged);
        }

        /// <summary>
        /// Popula dropdown de ordenação
        /// </summary>
        private void PopulateSortDropdown()
        {
            if (sortDropdown == null) return;

            sortDropdown.ClearOptions();
            List<string> options = new List<string>
            {
                "Name (A-Z)",
                "Name (Z-A)",
                "Price (Low-High)",
                "Price (High-Low)",
                "Rarity",
                "Type",
                "Weight"
            };
            sortDropdown.AddOptions(options);
        }

        /// <summary>
        /// Popula dropdown de filtro de raridade
        /// </summary>
        private void PopulateRarityFilterDropdown()
        {
            if (rarityFilterDropdown == null) return;

            rarityFilterDropdown.ClearOptions();
            List<string> options = new List<string>
            {
                "All Rarities",
                "Common",
                "Uncommon",
                "Rare",
                "Epic",
                "Legendary"
            };
            rarityFilterDropdown.AddOptions(options);
        }

        /// <summary>
        /// Alterna visibilidade do inventário
        /// </summary>
        public void ToggleInventory()
        {
            if (inventoryPanel.activeSelf)
                CloseInventory();
            else
                OpenInventory();
        }

        /// <summary>
        /// Abre o inventário
        /// </summary>
        public void OpenInventory()
        {
            inventoryPanel.SetActive(true);
            RefreshInventory();
            UpdateStatsDisplay();

            if (openSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(openSound);

            Time.timeScale = 0f; // Pausar o jogo
        }

        /// <summary>
        /// Fecha o inventário
        /// </summary>
        public void CloseInventory()
        {
            inventoryPanel.SetActive(false);

            if (closeSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(closeSound);

            Time.timeScale = 1f; // Retomar o jogo
        }

        /// <summary>
        /// Atualiza a exibição do inventário
        /// </summary>
        public void RefreshInventory()
        {
            ClearGrid();

            if (InventorySystem.Instance == null) return;

            List<InventoryItem> itemsToShow = GetItemsForCurrentCategory();

            // Aplicar filtros
            itemsToShow = ApplyFilters(itemsToShow);

            // Criar slots para cada item
            foreach (var item in itemsToShow)
            {
                CreateItemSlot(item);
            }

            UpdateStatsDisplay();
        }

        /// <summary>
        /// Retorna itens da categoria atual
        /// </summary>
        private List<InventoryItem> GetItemsForCurrentCategory()
        {
            if (showAllCategories)
            {
                return InventorySystem.Instance.GetAllItems();
            }
            else if (currentCategory == ItemType.Currency) // Equipados
            {
                return InventorySystem.Instance.GetEquippedItems();
            }
            else
            {
                return InventorySystem.Instance.GetItemsByCategory(currentCategory);
            }
        }

        /// <summary>
        /// Aplica filtros de busca e raridade
        /// </summary>
        private List<InventoryItem> ApplyFilters(List<InventoryItem> items)
        {
            // Filtro de busca
            if (searchField != null && !string.IsNullOrEmpty(searchField.text))
            {
                string search = searchField.text.ToLower();
                items = items.FindAll(item =>
                    item.itemData.itemName.ToLower().Contains(search) ||
                    item.itemData.description.ToLower().Contains(search)
                );
            }

            // Filtro de raridade
            if (rarityFilterDropdown != null && rarityFilterDropdown.value > 0)
            {
                ItemRarity rarity = (ItemRarity)(rarityFilterDropdown.value - 1);
                items = items.FindAll(item => item.itemData.rarity == rarity);
            }

            return items;
        }

        /// <summary>
        /// Limpa o grid de itens
        /// </summary>
        private void ClearGrid()
        {
            foreach (var slot in activeSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            activeSlots.Clear();
        }

        /// <summary>
        /// Cria um slot de item
        /// </summary>
        private void CreateItemSlot(InventoryItem item)
        {
            if (itemSlotPrefab == null || itemGridParent == null) return;

            GameObject slotObj = Instantiate(itemSlotPrefab, itemGridParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();

            if (slot != null)
            {
                slot.Setup(item, this);
                activeSlots.Add(slot);
            }
        }

        /// <summary>
        /// Seleciona um item e mostra detalhes
        /// </summary>
        public void SelectItem(InventoryItem item)
        {
            selectedItem = item;
            ShowItemDetails(item);

            if (itemClickSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(itemClickSound);
        }

        /// <summary>
        /// Mostra detalhes de um item
        /// </summary>
        private void ShowItemDetails(InventoryItem item)
        {
            if (detailsPanel == null) return;

            detailsPanel.SetActive(true);

            if (detailIcon != null)
                detailIcon.sprite = item.itemData.icon;

            if (detailName != null)
            {
                detailName.text = item.itemData.itemName;
                detailName.color = item.itemData.GetRarityColor();
            }

            if (detailDescription != null)
                detailDescription.text = item.itemData.GetDetailedDescription();

            if (detailStats != null)
                detailStats.text = GetItemStatsText(item);

            // Atualizar botões
            UpdateDetailButtons(item);
        }

        /// <summary>
        /// Atualiza os botões de ação baseado no item
        /// </summary>
        private void UpdateDetailButtons(InventoryItem item)
        {
            if (useButton != null)
                useButton.gameObject.SetActive(item.itemData.isConsumable);

            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(item.itemData.isEquippable);
                if (equipButton.gameObject.activeSelf)
                {
                    var buttonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = item.isEquipped ? "Unequip" : "Equip";
                }
            }

            if (dropButton != null)
                dropButton.gameObject.SetActive(true);

            if (sellButton != null)
                sellButton.gameObject.SetActive(item.itemData.canBeSold);
        }

        /// <summary>
        /// Retorna texto de stats do item
        /// </summary>
        private string GetItemStatsText(InventoryItem item)
        {
            string stats = $"Quantity: {item.quantity}\n";
            stats += $"Weight: {item.GetTotalWeight():F1} kg\n";
            stats += $"Value: {item.itemData.GetTotalValue(item.quantity)} credits\n";

            if (!item.itemData.isConsumable)
            {
                stats += $"Durability: {item.durability:F0}%\n";
            }

            if (item.isEquipped)
            {
                stats += "\n<color=green>[EQUIPPED]</color>";
            }

            return stats;
        }

        /// <summary>
        /// Usa o item selecionado
        /// </summary>
        private void UseSelectedItem()
        {
            if (selectedItem == null) return;

            if (InventorySystem.Instance.UseItem(selectedItem.itemData.itemID))
            {
                RefreshInventory();
                if (selectedItem.quantity <= 0)
                {
                    detailsPanel.SetActive(false);
                    selectedItem = null;
                }
                else
                {
                    ShowItemDetails(selectedItem);
                }
            }
        }

        /// <summary>
        /// Equipa/desequipa o item selecionado
        /// </summary>
        private void EquipSelectedItem()
        {
            if (selectedItem == null) return;

            if (selectedItem.isEquipped)
            {
                InventorySystem.Instance.UnequipItem(selectedItem.itemData.itemID);
            }
            else
            {
                InventorySystem.Instance.EquipItem(selectedItem.itemData.itemID);
            }

            RefreshInventory();
            ShowItemDetails(selectedItem);
        }

        /// <summary>
        /// Descarta o item selecionado
        /// </summary>
        private void DropSelectedItem()
        {
            if (selectedItem == null) return;

            // TODO: Abrir diálogo de confirmação
            InventorySystem.Instance.RemoveItem(selectedItem.itemData.itemID, 1);
            RefreshInventory();
            detailsPanel.SetActive(false);
            selectedItem = null;
        }

        /// <summary>
        /// Vende o item selecionado
        /// </summary>
        private void SellSelectedItem()
        {
            if (selectedItem == null) return;

            int sellValue = selectedItem.itemData.sellPrice;
            InventorySystem.Instance.RemoveItem(selectedItem.itemData.itemID, 1);
            InventorySystem.Instance.AddCredits(sellValue);

            RefreshInventory();
            if (selectedItem.quantity <= 0)
            {
                detailsPanel.SetActive(false);
                selectedItem = null;
            }
        }

        /// <summary>
        /// Atualiza exibição de estatísticas
        /// </summary>
        private void UpdateStatsDisplay()
        {
            if (InventorySystem.Instance == null) return;

            if (creditsText != null)
                creditsText.text = $"{InventorySystem.Instance.GetCurrentCredits()} Credits";

            if (slotsText != null)
            {
                int used = InventorySystem.Instance.GetAllItems().Count;
                int max = InventorySystem.Instance.GetMaxSlots();
                slotsText.text = $"Slots: {used}/{max}";
            }

            if (weightText != null)
            {
                float current = InventorySystem.Instance.GetCurrentWeight();
                float max = InventorySystem.Instance.GetMaxWeight();
                weightText.text = $"Weight: {current:F1}/{max:F0} kg";

                if (weightBar != null)
                    weightBar.fillAmount = current / max;
            }
        }

        private void UpdateCreditsDisplay(int newAmount)
        {
            UpdateStatsDisplay();
        }

        // Métodos para trocar de categoria
        private void ShowAllItems()
        {
            showAllCategories = true;
            RefreshInventory();
        }

        private void ShowWeapons()
        {
            showAllCategories = false;
            currentCategory = ItemType.Weapon;
            RefreshInventory();
        }

        private void ShowShipParts()
        {
            showAllCategories = false;
            currentCategory = ItemType.ShipPart;
            RefreshInventory();
        }

        private void ShowConsumables()
        {
            showAllCategories = false;
            currentCategory = ItemType.Consumable;
            RefreshInventory();
        }

        private void ShowQuestItems()
        {
            showAllCategories = false;
            currentCategory = ItemType.QuestItem;
            RefreshInventory();
        }

        private void ShowEquipped()
        {
            showAllCategories = false;
            currentCategory = ItemType.Currency; // Hack para equipados
            RefreshInventory();
        }

        // Callbacks de filtros
        private void OnSearchChanged(string searchText)
        {
            RefreshInventory();
        }

        private void OnSortChanged(int sortIndex)
        {
            // Implementar ordenação
            RefreshInventory();
        }

        private void OnRarityFilterChanged(int filterIndex)
        {
            RefreshInventory();
        }
    }

    /// <summary>
    /// Componente de slot individual de item
    /// </summary>
    public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private GameObject equippedIndicator;

        private InventoryItem item;
        private InventoryUI inventoryUI;
        private Canvas canvas;
        private CanvasGroup canvasGroup;

        public void Setup(InventoryItem inventoryItem, InventoryUI ui)
        {
            item = inventoryItem;
            inventoryUI = ui;

            if (iconImage != null)
                iconImage.sprite = item.itemData.icon;

            if (quantityText != null)
            {
                quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";
            }

            if (rarityBorder != null)
                rarityBorder.color = item.itemData.GetRarityColor();

            if (equippedIndicator != null)
                equippedIndicator.SetActive(item.isEquipped);

            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (inventoryUI != null)
                inventoryUI.SelectItem(item);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            // Implementar lógica de drop
        }
    }
}

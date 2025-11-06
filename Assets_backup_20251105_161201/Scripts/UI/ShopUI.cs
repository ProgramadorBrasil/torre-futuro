using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using SpaceRPG.Data;
using SpaceRPG.Systems;

namespace SpaceRPG.UI
{
    /// <summary>
    /// UI completa da loja com categorias, busca e wishlist
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI shopNameText;

        [Header("Tabs")]
        [SerializeField] private Button allItemsTab;
        [SerializeField] private Button weaponsTab;
        [SerializeField] private Button shipPartsTab;
        [SerializeField] private Button consumablesTab;
        [SerializeField] private Button specialOffersTab;
        [SerializeField] private Button wishlistTab;

        [Header("Item Grid")]
        [SerializeField] private Transform shopGridParent;
        [SerializeField] private GameObject shopItemSlotPrefab;

        [Header("Item Details")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailDescription;
        [SerializeField] private TextMeshProUGUI detailStats;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI stockText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button wishlistButton;
        [SerializeField] private TMP_InputField quantityInput;

        [Header("Search & Filter")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private Slider minPriceSlider;
        [SerializeField] private Slider maxPriceSlider;
        [SerializeField] private TextMeshProUGUI priceRangeText;

        [Header("Player Stats")]
        [SerializeField] private TextMeshProUGUI playerCreditsText;
        [SerializeField] private TextMeshProUGUI loyaltyDiscountText;
        [SerializeField] private TextMeshProUGUI totalPurchasesText;

        [Header("Transaction Feedback")]
        [SerializeField] private GameObject transactionPopup;
        [SerializeField] private TextMeshProUGUI transactionMessageText;
        [SerializeField] private float popupDuration = 2f;

        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip buySound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip wishlistSound;

        // Estado interno
        private List<ShopItemSlot> activeSlots = new List<ShopItemSlot>();
        private ShopItem selectedItem;
        private ItemType currentCategory;
        private bool showAllCategories = true;
        private bool showSpecialOffers = false;
        private bool showWishlist = false;

        private void Start()
        {
            InitializeUI();
            SetupListeners();
            CloseShop();
        }

        private void OnEnable()
        {
            if (ShopSystem.Instance != null)
            {
                ShopSystem.Instance.OnItemPurchased += OnItemPurchased;
                ShopSystem.Instance.OnShopRestocked += RefreshShop;
                ShopSystem.Instance.OnTransactionFailed += ShowTransactionError;
            }

            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnCreditsChanged += UpdatePlayerCredits;
            }
        }

        private void OnDisable()
        {
            if (ShopSystem.Instance != null)
            {
                ShopSystem.Instance.OnItemPurchased -= OnItemPurchased;
                ShopSystem.Instance.OnShopRestocked -= RefreshShop;
                ShopSystem.Instance.OnTransactionFailed -= ShowTransactionError;
            }

            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnCreditsChanged -= UpdatePlayerCredits;
            }
        }

        private void Update()
        {
            // Toggle loja com tecla S
            if (Input.GetKeyDown(KeyCode.S) && !Input.GetKey(KeyCode.LeftControl))
            {
                ToggleShop();
            }
        }

        /// <summary>
        /// Inicializa a UI
        /// </summary>
        private void InitializeUI()
        {
            if (detailsPanel != null)
                detailsPanel.SetActive(false);

            if (transactionPopup != null)
                transactionPopup.SetActive(false);

            if (shopNameText != null && ShopSystem.Instance != null)
                shopNameText.text = ShopSystem.Instance.GetShopName();

            PopulateSortDropdown();
            UpdatePlayerStats();
        }

        /// <summary>
        /// Configura listeners
        /// </summary>
        private void SetupListeners()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseShop);

            if (allItemsTab != null)
                allItemsTab.onClick.AddListener(ShowAllItems);

            if (weaponsTab != null)
                weaponsTab.onClick.AddListener(ShowWeapons);

            if (shipPartsTab != null)
                shipPartsTab.onClick.AddListener(ShowShipParts);

            if (consumablesTab != null)
                consumablesTab.onClick.AddListener(ShowConsumables);

            if (specialOffersTab != null)
                specialOffersTab.onClick.AddListener(ShowSpecialOffers);

            if (wishlistTab != null)
                wishlistTab.onClick.AddListener(ShowWishlist);

            if (buyButton != null)
                buyButton.onClick.AddListener(BuySelectedItem);

            if (wishlistButton != null)
                wishlistButton.onClick.AddListener(ToggleWishlist);

            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);

            if (sortDropdown != null)
                sortDropdown.onValueChanged.AddListener(OnSortChanged);

            if (minPriceSlider != null)
                minPriceSlider.onValueChanged.AddListener(OnPriceFilterChanged);

            if (maxPriceSlider != null)
                maxPriceSlider.onValueChanged.AddListener(OnPriceFilterChanged);
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
                "Price (Low-High)",
                "Price (High-Low)",
                "Rarity",
                "Type"
            };
            sortDropdown.AddOptions(options);
        }

        /// <summary>
        /// Alterna visibilidade da loja
        /// </summary>
        public void ToggleShop()
        {
            if (shopPanel.activeSelf)
                CloseShop();
            else
                OpenShop();
        }

        /// <summary>
        /// Abre a loja
        /// </summary>
        public void OpenShop()
        {
            shopPanel.SetActive(true);
            RefreshShop();
            UpdatePlayerStats();

            if (openSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(openSound);

            Time.timeScale = 0f;
        }

        /// <summary>
        /// Fecha a loja
        /// </summary>
        public void CloseShop()
        {
            shopPanel.SetActive(false);

            if (closeSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(closeSound);

            Time.timeScale = 1f;
        }

        /// <summary>
        /// Atualiza a exibição da loja
        /// </summary>
        public void RefreshShop()
        {
            ClearGrid();

            if (ShopSystem.Instance == null) return;

            List<ShopItem> itemsToShow = GetItemsForCurrentView();
            itemsToShow = ApplyFilters(itemsToShow);

            foreach (var item in itemsToShow)
            {
                CreateShopItemSlot(item);
            }
        }

        /// <summary>
        /// Retorna itens para a visão atual
        /// </summary>
        private List<ShopItem> GetItemsForCurrentView()
        {
            if (showWishlist)
            {
                return ShopSystem.Instance.GetWishlistItems();
            }
            else if (showSpecialOffers)
            {
                return ShopSystem.Instance.GetSpecialOffers();
            }
            else if (showAllCategories)
            {
                return ShopSystem.Instance.GetAllShopItems();
            }
            else
            {
                return ShopSystem.Instance.GetItemsByCategory(currentCategory);
            }
        }

        /// <summary>
        /// Aplica filtros de busca e preço
        /// </summary>
        private List<ShopItem> ApplyFilters(List<ShopItem> items)
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

            // Filtro de preço
            if (minPriceSlider != null && maxPriceSlider != null)
            {
                int minPrice = Mathf.RoundToInt(minPriceSlider.value);
                int maxPrice = Mathf.RoundToInt(maxPriceSlider.value);
                items = items.FindAll(item =>
                    item.itemData.buyPrice >= minPrice &&
                    item.itemData.buyPrice <= maxPrice
                );
            }

            return items;
        }

        /// <summary>
        /// Limpa o grid
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
        /// Cria um slot de item da loja
        /// </summary>
        private void CreateShopItemSlot(ShopItem item)
        {
            if (shopItemSlotPrefab == null || shopGridParent == null) return;

            GameObject slotObj = Instantiate(shopItemSlotPrefab, shopGridParent);
            ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();

            if (slot != null)
            {
                slot.Setup(item, this);
                activeSlots.Add(slot);
            }
        }

        /// <summary>
        /// Seleciona um item e mostra detalhes
        /// </summary>
        public void SelectItem(ShopItem item)
        {
            selectedItem = item;
            ShowItemDetails(item);
        }

        /// <summary>
        /// Mostra detalhes de um item
        /// </summary>
        private void ShowItemDetails(ShopItem item)
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
                detailStats.text = GetShopItemStatsText(item);

            if (priceText != null)
            {
                int price = item.GetDiscountedPrice();
                priceText.text = $"{price} Credits";

                if (item.discountPercent > 0)
                {
                    priceText.text += $" <color=green>(-{item.discountPercent:F0}%)</color>";
                }
            }

            if (stockText != null)
            {
                stockText.text = item.stock < 0 ? "In Stock: Unlimited" : $"In Stock: {item.stock}";
            }

            // Atualizar botão wishlist
            if (wishlistButton != null)
            {
                var buttonText = wishlistButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    bool isInWishlist = ShopSystem.Instance.IsInWishlist(item.itemData.itemID);
                    buttonText.text = isInWishlist ? "Remove from Wishlist" : "Add to Wishlist";
                }
            }
        }

        /// <summary>
        /// Retorna texto de stats do item da loja
        /// </summary>
        private string GetShopItemStatsText(ShopItem item)
        {
            string stats = item.itemData.GetDetailedDescription();

            if (item.isNew)
                stats += "\n<color=yellow>[NEW]</color>";

            if (item.isSpecialOffer)
                stats += "\n<color=orange>[SPECIAL OFFER]</color>";

            if (item.isLocked)
                stats += "\n<color=red>[LOCKED]</color>";

            return stats;
        }

        /// <summary>
        /// Compra o item selecionado
        /// </summary>
        private void BuySelectedItem()
        {
            if (selectedItem == null) return;

            int quantity = 1;
            if (quantityInput != null && !string.IsNullOrEmpty(quantityInput.text))
            {
                int.TryParse(quantityInput.text, out quantity);
                quantity = Mathf.Max(1, quantity);
            }

            if (ShopSystem.Instance.BuyItem(selectedItem.itemData.itemID, quantity))
            {
                ShowTransactionSuccess($"Purchased {quantity}x {selectedItem.itemData.itemName}!");
                RefreshShop();
                ShowItemDetails(selectedItem);
            }
        }

        /// <summary>
        /// Toggle wishlist do item selecionado
        /// </summary>
        private void ToggleWishlist()
        {
            if (selectedItem == null) return;

            bool isInWishlist = ShopSystem.Instance.IsInWishlist(selectedItem.itemData.itemID);

            if (isInWishlist)
            {
                ShopSystem.Instance.RemoveFromWishlist(selectedItem.itemData.itemID);
                ShowTransactionSuccess($"Removed from wishlist");
            }
            else
            {
                ShopSystem.Instance.AddToWishlist(selectedItem.itemData.itemID);
                ShowTransactionSuccess($"Added to wishlist");

                if (wishlistSound != null && AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(wishlistSound);
            }

            ShowItemDetails(selectedItem);
        }

        /// <summary>
        /// Atualiza estatísticas do jogador
        /// </summary>
        private void UpdatePlayerStats()
        {
            if (playerCreditsText != null && InventorySystem.Instance != null)
            {
                playerCreditsText.text = $"Credits: {InventorySystem.Instance.GetCurrentCredits()}";
            }

            if (loyaltyDiscountText != null && ShopSystem.Instance != null)
            {
                float discount = ShopSystem.Instance.GetLoyaltyDiscount();
                loyaltyDiscountText.text = $"Loyalty Discount: {discount:F1}%";
            }

            if (totalPurchasesText != null && ShopSystem.Instance != null)
            {
                totalPurchasesText.text = $"Total Purchases: {ShopSystem.Instance.GetTotalPurchases()}";
            }
        }

        private void UpdatePlayerCredits(int newAmount)
        {
            UpdatePlayerStats();
        }

        /// <summary>
        /// Mostra mensagem de sucesso
        /// </summary>
        private void ShowTransactionSuccess(string message)
        {
            ShowTransactionMessage(message, Color.green);

            if (buySound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(buySound);
        }

        /// <summary>
        /// Mostra mensagem de erro
        /// </summary>
        private void ShowTransactionError(string message)
        {
            ShowTransactionMessage(message, Color.red);

            if (errorSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(errorSound);
        }

        /// <summary>
        /// Mostra mensagem de transação
        /// </summary>
        private void ShowTransactionMessage(string message, Color color)
        {
            if (transactionPopup == null || transactionMessageText == null) return;

            transactionMessageText.text = message;
            transactionMessageText.color = color;
            transactionPopup.SetActive(true);

            CancelInvoke(nameof(HideTransactionPopup));
            Invoke(nameof(HideTransactionPopup), popupDuration);
        }

        private void HideTransactionPopup()
        {
            if (transactionPopup != null)
                transactionPopup.SetActive(false);
        }

        private void OnItemPurchased(ShopItem item)
        {
            UpdatePlayerStats();
        }

        // Métodos de navegação
        private void ShowAllItems()
        {
            showAllCategories = true;
            showSpecialOffers = false;
            showWishlist = false;
            RefreshShop();
        }

        private void ShowWeapons()
        {
            showAllCategories = false;
            showSpecialOffers = false;
            showWishlist = false;
            currentCategory = ItemType.Weapon;
            RefreshShop();
        }

        private void ShowShipParts()
        {
            showAllCategories = false;
            showSpecialOffers = false;
            showWishlist = false;
            currentCategory = ItemType.ShipPart;
            RefreshShop();
        }

        private void ShowConsumables()
        {
            showAllCategories = false;
            showSpecialOffers = false;
            showWishlist = false;
            currentCategory = ItemType.Consumable;
            RefreshShop();
        }

        private void ShowSpecialOffers()
        {
            showAllCategories = false;
            showSpecialOffers = true;
            showWishlist = false;
            RefreshShop();
        }

        private void ShowWishlist()
        {
            showAllCategories = false;
            showSpecialOffers = false;
            showWishlist = true;
            RefreshShop();
        }

        // Callbacks de filtros
        private void OnSearchChanged(string search)
        {
            RefreshShop();
        }

        private void OnSortChanged(int sortIndex)
        {
            RefreshShop();
        }

        private void OnPriceFilterChanged(float value)
        {
            if (priceRangeText != null && minPriceSlider != null && maxPriceSlider != null)
            {
                priceRangeText.text = $"Price: {minPriceSlider.value:F0} - {maxPriceSlider.value:F0}";
            }
            RefreshShop();
        }
    }

    /// <summary>
    /// Slot individual de item da loja
    /// </summary>
    public class ShopItemSlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI stockText;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private GameObject discountTag;
        [SerializeField] private TextMeshProUGUI discountText;
        [SerializeField] private GameObject newTag;
        [SerializeField] private GameObject wishlistIndicator;
        [SerializeField] private Button selectButton;

        private ShopItem item;
        private ShopUI shopUI;

        public void Setup(ShopItem shopItem, ShopUI ui)
        {
            item = shopItem;
            shopUI = ui;

            if (iconImage != null)
                iconImage.sprite = item.itemData.icon;

            if (nameText != null)
                nameText.text = item.itemData.itemName;

            if (priceText != null)
            {
                int price = item.GetDiscountedPrice();
                priceText.text = $"{price}¢";
            }

            if (stockText != null)
            {
                stockText.text = item.stock < 0 ? "" : item.stock.ToString();
            }

            if (rarityBorder != null)
                rarityBorder.color = item.itemData.GetRarityColor();

            if (discountTag != null)
            {
                discountTag.SetActive(item.discountPercent > 0);
                if (discountText != null && item.discountPercent > 0)
                    discountText.text = $"-{item.discountPercent:F0}%";
            }

            if (newTag != null)
                newTag.SetActive(item.isNew);

            if (wishlistIndicator != null)
                wishlistIndicator.SetActive(ShopSystem.Instance.IsInWishlist(item.itemData.itemID));

            if (selectButton != null)
                selectButton.onClick.AddListener(() => shopUI.SelectItem(item));
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SpaceRPG.Data;
using System;

namespace SpaceRPG.Systems
{
    /// <summary>
    /// Sistema completo de loja com compra/venda, descontos e wishlist
    /// </summary>
    public class ShopSystem : MonoBehaviour
    {
        private static ShopSystem _instance;
        public static ShopSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ShopSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ShopSystem");
                        _instance = go.AddComponent<ShopSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Shop Configuration")]
        [SerializeField] private string shopName = "Galactic Trade Station";
        [SerializeField] private float sellPriceMultiplier = 0.5f; // Vende por 50% do valor
        [SerializeField] private float buyPriceMultiplier = 1.0f; // Compra por 100% do valor

        [Header("Inventory")]
        [SerializeField] private List<ShopItem> shopInventory = new List<ShopItem>();
        [SerializeField] private List<ShopItem> specialOffers = new List<ShopItem>();

        [Header("Discounts")]
        [SerializeField] private float dailyDiscountPercent = 10f;
        [SerializeField] private float loyaltyDiscountPercent = 0f; // Aumenta com compras
        [SerializeField] private int purchaseCountForLoyalty = 10; // Compras necessárias para desconto

        [Header("Player Data")]
        [SerializeField] private int totalPurchases = 0;
        [SerializeField] private int totalSales = 0;
        [SerializeField] private int totalCreditsSpent = 0;
        [SerializeField] private List<string> purchaseHistory = new List<string>();
        [SerializeField] private List<string> wishlist = new List<string>();

        [Header("Stock Management")]
        [SerializeField] private bool hasLimitedStock = true;
        [SerializeField] private bool restockDaily = true;
        [SerializeField] private float restockTime = 86400f; // 24 horas em segundos
        private float lastRestockTime;

        // Events
        public event Action<ShopItem> OnItemPurchased;
        public event Action<ShopItem> OnItemSold;
        public event Action<ShopItem> OnItemAddedToWishlist;
        public event Action<ShopItem> OnItemRemovedFromWishlist;
        public event Action OnShopRestocked;
        public event Action<string> OnTransactionFailed;

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
            InitializeShop();
        }

        private void Update()
        {
            // Verificar se é hora de restock
            if (restockDaily && Time.time - lastRestockTime > restockTime)
            {
                RestockShop();
            }
        }

        /// <summary>
        /// Inicializa a loja com itens
        /// </summary>
        private void InitializeShop()
        {
            if (ItemDatabase.Instance == null)
            {
                Debug.LogError("ItemDatabase not found!");
                return;
            }

            // Adicionar itens compráveis ao inventário da loja
            List<ItemData> buyableItems = ItemDatabase.Instance.GetBuyableItems();
            foreach (var itemData in buyableItems)
            {
                if (!shopInventory.Exists(si => si.itemData.itemID == itemData.itemID))
                {
                    ShopItem shopItem = new ShopItem(itemData, UnityEngine.Random.Range(1, 10));
                    shopInventory.Add(shopItem);
                }
            }

            // Criar ofertas especiais aleatórias
            CreateSpecialOffers();

            lastRestockTime = Time.time;
            Debug.Log($"Shop initialized with {shopInventory.Count} items");
        }

        /// <summary>
        /// Cria ofertas especiais com descontos
        /// </summary>
        private void CreateSpecialOffers()
        {
            specialOffers.Clear();

            // Selecionar 3-5 itens aleatórios para ofertas
            int offerCount = UnityEngine.Random.Range(3, 6);
            var randomItems = shopInventory.OrderBy(x => UnityEngine.Random.value).Take(offerCount);

            foreach (var item in randomItems)
            {
                ShopItem offer = item.Clone();
                offer.discountPercent = UnityEngine.Random.Range(10f, 40f);
                specialOffers.Add(offer);
            }

            Debug.Log($"Created {specialOffers.Count} special offers");
        }

        /// <summary>
        /// Compra um item da loja
        /// </summary>
        public bool BuyItem(string itemID, int quantity = 1)
        {
            ShopItem shopItem = FindShopItem(itemID);
            if (shopItem == null)
            {
                OnTransactionFailed?.Invoke("Item not found in shop");
                return false;
            }

            // Verificar estoque
            if (hasLimitedStock && shopItem.stock < quantity)
            {
                OnTransactionFailed?.Invoke("Insufficient stock");
                return false;
            }

            // Calcular preço com descontos
            int totalPrice = CalculatePurchasePrice(shopItem, quantity);

            // Verificar se o jogador tem créditos suficientes
            if (InventorySystem.Instance.GetCurrentCredits() < totalPrice)
            {
                OnTransactionFailed?.Invoke("Not enough credits");
                return false;
            }

            // Verificar espaço no inventário
            if (!InventorySystem.Instance.HasSpace() && !shopItem.itemData.isStackable)
            {
                OnTransactionFailed?.Invoke("Inventory is full");
                return false;
            }

            // Executar compra
            InventorySystem.Instance.RemoveCredits(totalPrice);
            InventorySystem.Instance.AddItem(shopItem.itemData, quantity);

            // Atualizar estoque
            if (hasLimitedStock)
            {
                shopItem.stock -= quantity;
            }

            // Atualizar estatísticas
            totalPurchases++;
            totalCreditsSpent += totalPrice;
            purchaseHistory.Add($"{DateTime.Now:yyyy-MM-dd HH:mm} - Bought {quantity}x {shopItem.itemData.itemName} for {totalPrice} credits");

            // Atualizar desconto de lealdade
            UpdateLoyaltyDiscount();

            OnItemPurchased?.Invoke(shopItem);
            Debug.Log($"Purchased {quantity}x {shopItem.itemData.itemName} for {totalPrice} credits");

            return true;
        }

        /// <summary>
        /// Vende um item para a loja
        /// </summary>
        public bool SellItem(string itemID, int quantity = 1)
        {
            if (!InventorySystem.Instance.HasItem(itemID, quantity))
            {
                OnTransactionFailed?.Invoke("Item not found in inventory");
                return false;
            }

            InventoryItem inventoryItem = InventorySystem.Instance.FindItemByID(itemID);
            if (inventoryItem == null || !inventoryItem.itemData.canBeSold)
            {
                OnTransactionFailed?.Invoke("Item cannot be sold");
                return false;
            }

            // Calcular preço de venda
            int sellPrice = CalculateSellPrice(inventoryItem.itemData, quantity);

            // Executar venda
            InventorySystem.Instance.RemoveItem(itemID, quantity);
            InventorySystem.Instance.AddCredits(sellPrice);

            // Adicionar ao estoque da loja
            ShopItem shopItem = FindShopItem(itemID);
            if (shopItem != null && hasLimitedStock)
            {
                shopItem.stock += quantity;
            }

            // Atualizar estatísticas
            totalSales++;

            OnItemSold?.Invoke(shopItem);
            Debug.Log($"Sold {quantity}x {inventoryItem.itemData.itemName} for {sellPrice} credits");

            return true;
        }

        /// <summary>
        /// Calcula o preço de compra com descontos
        /// </summary>
        public int CalculatePurchasePrice(ShopItem shopItem, int quantity)
        {
            float basePrice = shopItem.itemData.buyPrice * buyPriceMultiplier;
            float discount = shopItem.discountPercent + loyaltyDiscountPercent;
            float finalPrice = basePrice * (1f - discount / 100f);

            return Mathf.CeilToInt(finalPrice * quantity);
        }

        /// <summary>
        /// Calcula o preço de venda
        /// </summary>
        public int CalculateSellPrice(ItemData itemData, int quantity)
        {
            float sellPrice = itemData.sellPrice * sellPriceMultiplier;
            return Mathf.FloorToInt(sellPrice * quantity);
        }

        /// <summary>
        /// Busca um item no inventário da loja
        /// </summary>
        private ShopItem FindShopItem(string itemID)
        {
            ShopItem item = shopInventory.FirstOrDefault(si => si.itemData.itemID == itemID);
            if (item == null)
            {
                item = specialOffers.FirstOrDefault(si => si.itemData.itemID == itemID);
            }
            return item;
        }

        /// <summary>
        /// Adiciona item à wishlist
        /// </summary>
        public void AddToWishlist(string itemID)
        {
            if (wishlist.Contains(itemID))
            {
                Debug.LogWarning("Item already in wishlist");
                return;
            }

            wishlist.Add(itemID);

            ShopItem shopItem = FindShopItem(itemID);
            OnItemAddedToWishlist?.Invoke(shopItem);
            Debug.Log($"Added to wishlist: {shopItem?.itemData.itemName}");
        }

        /// <summary>
        /// Remove item da wishlist
        /// </summary>
        public void RemoveFromWishlist(string itemID)
        {
            if (!wishlist.Remove(itemID))
            {
                Debug.LogWarning("Item not in wishlist");
                return;
            }

            ShopItem shopItem = FindShopItem(itemID);
            OnItemRemovedFromWishlist?.Invoke(shopItem);
            Debug.Log($"Removed from wishlist: {shopItem?.itemData.itemName}");
        }

        /// <summary>
        /// Verifica se item está na wishlist
        /// </summary>
        public bool IsInWishlist(string itemID)
        {
            return wishlist.Contains(itemID);
        }

        /// <summary>
        /// Retorna itens da wishlist
        /// </summary>
        public List<ShopItem> GetWishlistItems()
        {
            List<ShopItem> items = new List<ShopItem>();
            foreach (string itemID in wishlist)
            {
                ShopItem item = FindShopItem(itemID);
                if (item != null)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        /// <summary>
        /// Atualiza desconto de lealdade baseado em compras
        /// </summary>
        private void UpdateLoyaltyDiscount()
        {
            loyaltyDiscountPercent = Mathf.Min(20f, (totalPurchases / purchaseCountForLoyalty) * 5f);
            Debug.Log($"Loyalty discount updated to {loyaltyDiscountPercent}%");
        }

        /// <summary>
        /// Reabastece a loja
        /// </summary>
        public void RestockShop()
        {
            foreach (var item in shopInventory)
            {
                item.stock = UnityEngine.Random.Range(5, 15);
            }

            CreateSpecialOffers();
            lastRestockTime = Time.time;

            OnShopRestocked?.Invoke();
            Debug.Log("Shop restocked!");
        }

        /// <summary>
        /// Retorna todos os itens da loja
        /// </summary>
        public List<ShopItem> GetAllShopItems()
        {
            return new List<ShopItem>(shopInventory);
        }

        /// <summary>
        /// Retorna itens por categoria
        /// </summary>
        public List<ShopItem> GetItemsByCategory(ItemType category)
        {
            return shopInventory.Where(item => item.itemData.itemType == category).ToList();
        }

        /// <summary>
        /// Retorna ofertas especiais
        /// </summary>
        public List<ShopItem> GetSpecialOffers()
        {
            return new List<ShopItem>(specialOffers);
        }

        /// <summary>
        /// Busca itens na loja
        /// </summary>
        public List<ShopItem> SearchShop(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return GetAllShopItems();

            searchTerm = searchTerm.ToLower();
            return shopInventory.Where(item =>
                item.itemData.itemName.ToLower().Contains(searchTerm) ||
                item.itemData.description.ToLower().Contains(searchTerm)
            ).ToList();
        }

        /// <summary>
        /// Filtra itens por preço
        /// </summary>
        public List<ShopItem> FilterByPrice(int minPrice, int maxPrice)
        {
            return shopInventory.Where(item =>
                item.itemData.buyPrice >= minPrice &&
                item.itemData.buyPrice <= maxPrice
            ).ToList();
        }

        /// <summary>
        /// Ordena itens da loja
        /// </summary>
        public List<ShopItem> SortShopItems(List<ShopItem> items, ItemDatabase.SortType sortType)
        {
            List<ShopItem> sorted = new List<ShopItem>(items);

            switch (sortType)
            {
                case ItemDatabase.SortType.Name:
                    sorted = sorted.OrderBy(i => i.itemData.itemName).ToList();
                    break;
                case ItemDatabase.SortType.Price:
                    sorted = sorted.OrderBy(i => i.itemData.buyPrice).ToList();
                    break;
                case ItemDatabase.SortType.Rarity:
                    sorted = sorted.OrderBy(i => i.itemData.rarity).ToList();
                    break;
                case ItemDatabase.SortType.Type:
                    sorted = sorted.OrderBy(i => i.itemData.itemType).ToList();
                    break;
            }

            return sorted;
        }

        /// <summary>
        /// Retorna histórico de compras
        /// </summary>
        public List<string> GetPurchaseHistory()
        {
            return new List<string>(purchaseHistory);
        }

        /// <summary>
        /// Retorna estatísticas da loja
        /// </summary>
        public string GetShopStats()
        {
            return $"Shop Name: {shopName}\n" +
                   $"Total Purchases: {totalPurchases}\n" +
                   $"Total Sales: {totalSales}\n" +
                   $"Credits Spent: {totalCreditsSpent}\n" +
                   $"Loyalty Discount: {loyaltyDiscountPercent:F1}%\n" +
                   $"Items in Stock: {shopInventory.Count}\n" +
                   $"Special Offers: {specialOffers.Count}\n" +
                   $"Wishlist Items: {wishlist.Count}";
        }

        // Getters públicos
        public string GetShopName() => shopName;
        public float GetLoyaltyDiscount() => loyaltyDiscountPercent;
        public int GetTotalPurchases() => totalPurchases;
        public int GetTotalSales() => totalSales;
        public int GetTotalCreditsSpent() => totalCreditsSpent;
    }

    /// <summary>
    /// Representa um item na loja
    /// </summary>
    [System.Serializable]
    public class ShopItem
    {
        public ItemData itemData;
        public int stock;
        public float discountPercent;
        public bool isSpecialOffer;
        public bool isNew;
        public bool isLocked; // Requer missão/level para desbloquear

        public ShopItem(ItemData data, int stockAmount = -1)
        {
            itemData = data;
            stock = stockAmount;
            discountPercent = 0f;
            isSpecialOffer = false;
            isNew = false;
            isLocked = false;
        }

        /// <summary>
        /// Retorna o preço com desconto
        /// </summary>
        public int GetDiscountedPrice()
        {
            float finalPrice = itemData.buyPrice * (1f - discountPercent / 100f);
            return Mathf.CeilToInt(finalPrice);
        }

        /// <summary>
        /// Verifica se está em estoque
        /// </summary>
        public bool IsInStock()
        {
            return stock < 0 || stock > 0; // -1 = estoque infinito
        }

        /// <summary>
        /// Clona o item da loja
        /// </summary>
        public ShopItem Clone()
        {
            return new ShopItem(itemData, stock)
            {
                discountPercent = this.discountPercent,
                isSpecialOffer = this.isSpecialOffer,
                isNew = this.isNew,
                isLocked = this.isLocked
            };
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SpaceRPG.Data
{
    /// <summary>
    /// Database centralizado de todos os itens do jogo
    /// Singleton pattern para acesso global
    /// </summary>
    public class ItemDatabase : MonoBehaviour
    {
        private static ItemDatabase _instance;
        public static ItemDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ItemDatabase>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ItemDatabase");
                        _instance = go.AddComponent<ItemDatabase>();
                    }
                }
                return _instance;
            }
        }

        [Header("Database Configuration")]
        [SerializeField] private List<ItemData> allItems = new List<ItemData>();

        [Header("Categories")]
        [SerializeField] private List<ItemData> weapons = new List<ItemData>();
        [SerializeField] private List<ItemData> shipParts = new List<ItemData>();
        [SerializeField] private List<ItemData> consumables = new List<ItemData>();
        [SerializeField] private List<ItemData> questItems = new List<ItemData>();
        [SerializeField] private List<ItemData> materials = new List<ItemData>();
        [SerializeField] private List<ItemData> seeds = new List<ItemData>();
        [SerializeField] private List<ItemData> plantCareItems = new List<ItemData>();
        [SerializeField] private List<ItemData> tools = new List<ItemData>();

        [Header("Default Items")]
        [SerializeField] private ItemData defaultCurrency;
        [SerializeField] private ItemData defaultWeapon;
        [SerializeField] private ItemData repairKit;
        [SerializeField] private ItemData waterItem;
        [SerializeField] private ItemData fertilizerItem;

        // Cache para busca rápida
        private Dictionary<string, ItemData> itemCache = new Dictionary<string, ItemData>();
        private bool isInitialized = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        /// <summary>
        /// Inicializa o database e cache
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            // Combinar todas as listas em allItems
            allItems.Clear();
            allItems.AddRange(weapons);
            allItems.AddRange(shipParts);
            allItems.AddRange(consumables);
            allItems.AddRange(questItems);
            allItems.AddRange(materials);
            allItems.AddRange(seeds);
            allItems.AddRange(plantCareItems);
            allItems.AddRange(tools);

            // Criar cache
            itemCache.Clear();
            foreach (var item in allItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemID))
                {
                    if (!itemCache.ContainsKey(item.itemID))
                    {
                        itemCache[item.itemID] = item;
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate item ID found: {item.itemID}");
                    }
                }
            }

            isInitialized = true;
            Debug.Log($"ItemDatabase initialized with {allItems.Count} items");
        }

        /// <summary>
        /// Busca um item pelo ID
        /// </summary>
        public ItemData GetItemByID(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
                return null;

            if (itemCache.TryGetValue(itemID, out ItemData item))
            {
                return item;
            }

            Debug.LogWarning($"Item not found: {itemID}");
            return null;
        }

        /// <summary>
        /// Busca um item pelo nome
        /// </summary>
        public ItemData GetItemByName(string itemName)
        {
            return allItems.FirstOrDefault(item =>
                item != null && item.itemName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retorna todos os itens de um tipo específico
        /// </summary>
        public List<ItemData> GetItemsByType(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon:
                    return new List<ItemData>(weapons);
                case ItemType.ShipPart:
                    return new List<ItemData>(shipParts);
                case ItemType.Consumable:
                    return new List<ItemData>(consumables);
                case ItemType.QuestItem:
                    return new List<ItemData>(questItems);
                case ItemType.Material:
                    return new List<ItemData>(materials);
                case ItemType.Seed:
                    return new List<ItemData>(seeds);
                case ItemType.PlantCare:
                    return new List<ItemData>(plantCareItems);
                case ItemType.Tool:
                    return new List<ItemData>(tools);
                default:
                    return new List<ItemData>();
            }
        }

        /// <summary>
        /// Retorna todos os itens de uma raridade específica
        /// </summary>
        public List<ItemData> GetItemsByRarity(ItemRarity rarity)
        {
            return allItems.Where(item => item != null && item.rarity == rarity).ToList();
        }

        /// <summary>
        /// Busca itens por nome (pesquisa parcial)
        /// </summary>
        public List<ItemData> SearchItems(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<ItemData>(allItems);

            searchTerm = searchTerm.ToLower();
            return allItems.Where(item =>
                item != null &&
                (item.itemName.ToLower().Contains(searchTerm) ||
                 item.description.ToLower().Contains(searchTerm))
            ).ToList();
        }

        /// <summary>
        /// Retorna itens que podem ser comprados
        /// </summary>
        public List<ItemData> GetBuyableItems()
        {
            return allItems.Where(item => item != null && item.canBeBought).ToList();
        }

        /// <summary>
        /// Retorna itens que podem ser vendidos
        /// </summary>
        public List<ItemData> GetSellableItems()
        {
            return allItems.Where(item => item != null && item.canBeSold).ToList();
        }

        /// <summary>
        /// Filtra itens por múltiplos critérios
        /// </summary>
        public List<ItemData> FilterItems(
            ItemType? type = null,
            ItemRarity? rarity = null,
            int? minPrice = null,
            int? maxPrice = null,
            bool? stackableOnly = null)
        {
            var filtered = new List<ItemData>(allItems);

            if (type.HasValue)
                filtered = filtered.Where(i => i.itemType == type.Value).ToList();

            if (rarity.HasValue)
                filtered = filtered.Where(i => i.rarity == rarity.Value).ToList();

            if (minPrice.HasValue)
                filtered = filtered.Where(i => i.buyPrice >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                filtered = filtered.Where(i => i.buyPrice <= maxPrice.Value).ToList();

            if (stackableOnly.HasValue)
                filtered = filtered.Where(i => i.isStackable == stackableOnly.Value).ToList();

            return filtered;
        }

        /// <summary>
        /// Ordena itens
        /// </summary>
        public enum SortType
        {
            Name,
            Price,
            Rarity,
            Type,
            Weight
        }

        public List<ItemData> SortItems(List<ItemData> items, SortType sortType, bool ascending = true)
        {
            List<ItemData> sorted = new List<ItemData>(items);

            switch (sortType)
            {
                case SortType.Name:
                    sorted = ascending ?
                        sorted.OrderBy(i => i.itemName).ToList() :
                        sorted.OrderByDescending(i => i.itemName).ToList();
                    break;
                case SortType.Price:
                    sorted = ascending ?
                        sorted.OrderBy(i => i.buyPrice).ToList() :
                        sorted.OrderByDescending(i => i.buyPrice).ToList();
                    break;
                case SortType.Rarity:
                    sorted = ascending ?
                        sorted.OrderBy(i => i.rarity).ToList() :
                        sorted.OrderByDescending(i => i.rarity).ToList();
                    break;
                case SortType.Type:
                    sorted = ascending ?
                        sorted.OrderBy(i => i.itemType).ToList() :
                        sorted.OrderByDescending(i => i.itemType).ToList();
                    break;
                case SortType.Weight:
                    sorted = ascending ?
                        sorted.OrderBy(i => i.weight).ToList() :
                        sorted.OrderByDescending(i => i.weight).ToList();
                    break;
            }

            return sorted;
        }

        /// <summary>
        /// Cria um novo item no inventário
        /// </summary>
        public InventoryItem CreateInventoryItem(string itemID, int quantity = 1)
        {
            ItemData data = GetItemByID(itemID);
            if (data == null)
            {
                Debug.LogError($"Cannot create inventory item: Item ID '{itemID}' not found");
                return null;
            }

            return new InventoryItem(data, quantity);
        }

        /// <summary>
        /// Retorna item de moeda padrão
        /// </summary>
        public ItemData GetCurrencyItem()
        {
            return defaultCurrency;
        }

        /// <summary>
        /// Retorna arma padrão
        /// </summary>
        public ItemData GetDefaultWeapon()
        {
            return defaultWeapon;
        }

        /// <summary>
        /// Adiciona um novo item ao database (runtime)
        /// </summary>
        public void RegisterItem(ItemData item)
        {
            if (item == null || string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogError("Cannot register null or invalid item");
                return;
            }

            if (itemCache.ContainsKey(item.itemID))
            {
                Debug.LogWarning($"Item already registered: {item.itemID}");
                return;
            }

            allItems.Add(item);
            itemCache[item.itemID] = item;

            // Adicionar à categoria apropriada
            switch (item.itemType)
            {
                case ItemType.Weapon:
                    weapons.Add(item);
                    break;
                case ItemType.ShipPart:
                    shipParts.Add(item);
                    break;
                case ItemType.Consumable:
                    consumables.Add(item);
                    break;
                case ItemType.QuestItem:
                    questItems.Add(item);
                    break;
                case ItemType.Material:
                    materials.Add(item);
                    break;
                case ItemType.Seed:
                    seeds.Add(item);
                    break;
                case ItemType.PlantCare:
                    plantCareItems.Add(item);
                    break;
                case ItemType.Tool:
                    tools.Add(item);
                    break;
            }

            Debug.Log($"Item registered: {item.itemName}");
        }

        /// <summary>
        /// Retorna estatísticas do database
        /// </summary>
        public string GetDatabaseStats()
        {
            return $"Total Items: {allItems.Count}\n" +
                   $"Weapons: {weapons.Count}\n" +
                   $"Ship Parts: {shipParts.Count}\n" +
                   $"Consumables: {consumables.Count}\n" +
                   $"Quest Items: {questItems.Count}\n" +
                   $"Materials: {materials.Count}\n" +
                   $"Seeds: {seeds.Count}\n" +
                   $"Plant Care: {plantCareItems.Count}\n" +
                   $"Tools: {tools.Count}";
        }

        // Getters para itens especiais
        public ItemData GetRepairKit() => repairKit;
        public ItemData GetWaterItem() => waterItem;
        public ItemData GetFertilizerItem() => fertilizerItem;

        /// <summary>
        /// Valida a integridade do database
        /// </summary>
        public bool ValidateDatabase()
        {
            bool isValid = true;

            foreach (var item in allItems)
            {
                if (item == null)
                {
                    Debug.LogError("Null item found in database");
                    isValid = false;
                    continue;
                }

                if (string.IsNullOrEmpty(item.itemID))
                {
                    Debug.LogError($"Item '{item.itemName}' has no ID");
                    isValid = false;
                }

                if (item.icon == null)
                {
                    Debug.LogWarning($"Item '{item.itemName}' has no icon");
                }

                if (item.buyPrice < 0 || item.sellPrice < 0)
                {
                    Debug.LogError($"Item '{item.itemName}' has invalid prices");
                    isValid = false;
                }
            }

            return isValid;
        }
    }
}

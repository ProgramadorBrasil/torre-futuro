using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SpaceRPG.Data;
using System;

namespace SpaceRPG.Systems
{
    /// <summary>
    /// Sistema completo de inventário com categorias, peso, filtros e persistência
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        private static InventorySystem _instance;
        public static InventorySystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InventorySystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("InventorySystem");
                        _instance = go.AddComponent<InventorySystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 50;
        [SerializeField] private float maxWeight = 500f;
        [SerializeField] private bool unlimitedWeight = false;

        [Header("Categories")]
        [SerializeField] private List<InventoryItem> allItems = new List<InventoryItem>();
        [SerializeField] private List<InventoryItem> weapons = new List<InventoryItem>();
        [SerializeField] private List<InventoryItem> shipParts = new List<InventoryItem>();
        [SerializeField] private List<InventoryItem> consumables = new List<InventoryItem>();
        [SerializeField] private List<InventoryItem> questItems = new List<InventoryItem>();
        [SerializeField] private List<InventoryItem> equippedItems = new List<InventoryItem>();

        [Header("Current State")]
        [SerializeField] private int currentCredits = 1000;
        [SerializeField] private InventoryItem equippedWeapon;
        [SerializeField] private InventoryItem equippedShield;
        [SerializeField] private InventoryItem equippedEngine;

        // Events
        public event Action<InventoryItem> OnItemAdded;
        public event Action<InventoryItem> OnItemRemoved;
        public event Action<InventoryItem> OnItemUsed;
        public event Action<InventoryItem> OnItemEquipped;
        public event Action<InventoryItem> OnItemUnequipped;
        public event Action<int> OnCreditsChanged;
        public event Action OnInventoryChanged;
        public event Action OnInventoryFull;
        public event Action OnWeightExceeded;

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
            InitializeInventory();
        }

        /// <summary>
        /// Inicializa o inventário com itens padrão
        /// </summary>
        private void InitializeInventory()
        {
            // Adicionar itens iniciais se necessário
            if (ItemDatabase.Instance != null)
            {
                ItemData defaultWeapon = ItemDatabase.Instance.GetDefaultWeapon();
                if (defaultWeapon != null)
                {
                    AddItem(defaultWeapon, 1);
                }
            }

            Debug.Log("Inventory initialized");
        }

        /// <summary>
        /// Adiciona um item ao inventário
        /// </summary>
        public bool AddItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                Debug.LogError("Cannot add null item");
                return false;
            }

            // Verificar se há espaço
            if (!HasSpace() && !itemData.isStackable)
            {
                Debug.LogWarning("Inventory is full");
                OnInventoryFull?.Invoke();
                return false;
            }

            // Verificar peso
            float itemWeight = itemData.weight * quantity;
            if (!unlimitedWeight && GetCurrentWeight() + itemWeight > maxWeight)
            {
                Debug.LogWarning("Weight limit exceeded");
                OnWeightExceeded?.Invoke();
                return false;
            }

            // Se o item é empilhável, tentar adicionar a uma pilha existente
            if (itemData.isStackable)
            {
                InventoryItem existingItem = FindItemByID(itemData.itemID);
                if (existingItem != null)
                {
                    int remainingSpace = itemData.maxStackSize - existingItem.quantity;
                    if (remainingSpace >= quantity)
                    {
                        existingItem.AddQuantity(quantity);
                        NotifyItemAdded(existingItem);
                        return true;
                    }
                    else if (remainingSpace > 0)
                    {
                        existingItem.AddQuantity(remainingSpace);
                        quantity -= remainingSpace;
                        NotifyItemAdded(existingItem);
                    }
                }
            }

            // Criar nova instância do item
            InventoryItem newItem = new InventoryItem(itemData, quantity);
            allItems.Add(newItem);

            // Adicionar à categoria apropriada
            CategorizeItem(newItem);

            NotifyItemAdded(newItem);
            return true;
        }

        /// <summary>
        /// Remove um item do inventário
        /// </summary>
        public bool RemoveItem(string itemID, int quantity = 1)
        {
            InventoryItem item = FindItemByID(itemID);
            if (item == null)
            {
                Debug.LogWarning($"Item not found: {itemID}");
                return false;
            }

            if (item.quantity < quantity)
            {
                Debug.LogWarning($"Not enough quantity. Have: {item.quantity}, Need: {quantity}");
                return false;
            }

            item.RemoveQuantity(quantity);

            if (item.quantity <= 0)
            {
                RemoveItemCompletely(item);
            }

            NotifyItemRemoved(item);
            return true;
        }

        /// <summary>
        /// Remove completamente um item do inventário
        /// </summary>
        private void RemoveItemCompletely(InventoryItem item)
        {
            allItems.Remove(item);
            weapons.Remove(item);
            shipParts.Remove(item);
            consumables.Remove(item);
            questItems.Remove(item);
            equippedItems.Remove(item);

            if (equippedWeapon == item) equippedWeapon = null;
            if (equippedShield == item) equippedShield = null;
            if (equippedEngine == item) equippedEngine = null;
        }

        /// <summary>
        /// Usa um item
        /// </summary>
        public bool UseItem(string itemID)
        {
            InventoryItem item = FindItemByID(itemID);
            if (item == null || !item.itemData.CanUse())
            {
                Debug.LogWarning($"Cannot use item: {itemID}");
                return false;
            }

            if (item.itemData.isConsumable)
            {
                // Aplicar efeitos do consumível
                ApplyConsumableEffects(item);

                // Reduzir quantidade
                item.Use();
                if (item.quantity <= 0)
                {
                    RemoveItemCompletely(item);
                }
            }
            else if (item.itemData.isEquippable)
            {
                // Equipar o item
                EquipItem(itemID);
            }

            OnItemUsed?.Invoke(item);
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Aplica efeitos de consumíveis
        /// </summary>
        private void ApplyConsumableEffects(InventoryItem item)
        {
            // Aqui você pode integrar com outros sistemas (saúde, energia, etc)
            Debug.Log($"Used consumable: {item.itemData.itemName}");

            // Tocar som de uso
            if (item.itemData.useSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(item.itemData.useSound);
            }

            // Spawnar efeito visual
            if (item.itemData.useEffect != null)
            {
                Instantiate(item.itemData.useEffect, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Equipa um item
        /// </summary>
        public bool EquipItem(string itemID)
        {
            InventoryItem item = FindItemByID(itemID);
            if (item == null || !item.itemData.isEquippable)
            {
                Debug.LogWarning($"Cannot equip item: {itemID}");
                return false;
            }

            // Desequipar item anterior do mesmo tipo
            if (item.itemData.itemType == ItemType.Weapon && equippedWeapon != null)
            {
                UnequipItem(equippedWeapon.itemData.itemID);
            }

            // Equipar novo item
            item.isEquipped = true;
            if (!equippedItems.Contains(item))
            {
                equippedItems.Add(item);
            }

            // Atribuir ao slot apropriado
            switch (item.itemData.itemType)
            {
                case ItemType.Weapon:
                    equippedWeapon = item;
                    break;
                case ItemType.ShipPart:
                    if (item.itemData.itemName.Contains("Shield"))
                        equippedShield = item;
                    else if (item.itemData.itemName.Contains("Engine"))
                        equippedEngine = item;
                    break;
            }

            OnItemEquipped?.Invoke(item);
            OnInventoryChanged?.Invoke();

            // Tocar som de equipar
            if (item.itemData.equipSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(item.itemData.equipSound);
            }

            Debug.Log($"Equipped: {item.itemData.itemName}");
            return true;
        }

        /// <summary>
        /// Desequipa um item
        /// </summary>
        public bool UnequipItem(string itemID)
        {
            InventoryItem item = FindItemByID(itemID);
            if (item == null || !item.isEquipped)
            {
                Debug.LogWarning($"Item not equipped: {itemID}");
                return false;
            }

            item.isEquipped = false;
            equippedItems.Remove(item);

            if (equippedWeapon == item) equippedWeapon = null;
            if (equippedShield == item) equippedShield = null;
            if (equippedEngine == item) equippedEngine = null;

            OnItemUnequipped?.Invoke(item);
            OnInventoryChanged?.Invoke();

            Debug.Log($"Unequipped: {item.itemData.itemName}");
            return true;
        }

        /// <summary>
        /// Busca um item pelo ID
        /// </summary>
        public InventoryItem FindItemByID(string itemID)
        {
            return allItems.FirstOrDefault(item => item.itemData.itemID == itemID);
        }

        /// <summary>
        /// Busca um item pelo nome
        /// </summary>
        public InventoryItem FindItemByName(string itemName)
        {
            return allItems.FirstOrDefault(item =>
                item.itemData.itemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retorna todos os itens de uma categoria
        /// </summary>
        public List<InventoryItem> GetItemsByCategory(ItemType category)
        {
            switch (category)
            {
                case ItemType.Weapon:
                    return new List<InventoryItem>(weapons);
                case ItemType.ShipPart:
                    return new List<InventoryItem>(shipParts);
                case ItemType.Consumable:
                    return new List<InventoryItem>(consumables);
                case ItemType.QuestItem:
                    return new List<InventoryItem>(questItems);
                default:
                    return new List<InventoryItem>(allItems);
            }
        }

        /// <summary>
        /// Categoriza um item nas listas apropriadas
        /// </summary>
        private void CategorizeItem(InventoryItem item)
        {
            switch (item.itemData.itemType)
            {
                case ItemType.Weapon:
                    if (!weapons.Contains(item)) weapons.Add(item);
                    break;
                case ItemType.ShipPart:
                    if (!shipParts.Contains(item)) shipParts.Add(item);
                    break;
                case ItemType.Consumable:
                case ItemType.PlantCare:
                    if (!consumables.Contains(item)) consumables.Add(item);
                    break;
                case ItemType.QuestItem:
                    if (!questItems.Contains(item)) questItems.Add(item);
                    break;
            }
        }

        /// <summary>
        /// Verifica se o inventário tem espaço
        /// </summary>
        public bool HasSpace()
        {
            return allItems.Count < maxSlots;
        }

        /// <summary>
        /// Retorna o peso atual do inventário
        /// </summary>
        public float GetCurrentWeight()
        {
            float totalWeight = 0f;
            foreach (var item in allItems)
            {
                totalWeight += item.GetTotalWeight();
            }
            return totalWeight;
        }

        /// <summary>
        /// Verifica se tem um item específico
        /// </summary>
        public bool HasItem(string itemID, int quantity = 1)
        {
            InventoryItem item = FindItemByID(itemID);
            return item != null && item.quantity >= quantity;
        }

        /// <summary>
        /// Retorna a quantidade de um item
        /// </summary>
        public int GetItemQuantity(string itemID)
        {
            InventoryItem item = FindItemByID(itemID);
            return item?.quantity ?? 0;
        }

        /// <summary>
        /// Adiciona créditos
        /// </summary>
        public void AddCredits(int amount)
        {
            currentCredits += amount;
            OnCreditsChanged?.Invoke(currentCredits);
            OnInventoryChanged?.Invoke();
            Debug.Log($"Added {amount} credits. Total: {currentCredits}");
        }

        /// <summary>
        /// Remove créditos
        /// </summary>
        public bool RemoveCredits(int amount)
        {
            if (currentCredits < amount)
            {
                Debug.LogWarning($"Not enough credits. Have: {currentCredits}, Need: {amount}");
                return false;
            }

            currentCredits -= amount;
            OnCreditsChanged?.Invoke(currentCredits);
            OnInventoryChanged?.Invoke();
            Debug.Log($"Removed {amount} credits. Total: {currentCredits}");
            return true;
        }

        /// <summary>
        /// Ordena o inventário
        /// </summary>
        public void SortInventory(ItemDatabase.SortType sortType)
        {
            List<ItemData> itemDataList = allItems.Select(i => i.itemData).ToList();
            List<ItemData> sorted = ItemDatabase.Instance.SortItems(itemDataList, sortType);

            allItems = allItems.OrderBy(item => sorted.IndexOf(item.itemData)).ToList();
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Filtra o inventário
        /// </summary>
        public List<InventoryItem> FilterInventory(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<InventoryItem>(allItems);

            searchTerm = searchTerm.ToLower();
            return allItems.Where(item =>
                item.itemData.itemName.ToLower().Contains(searchTerm) ||
                item.itemData.description.ToLower().Contains(searchTerm)
            ).ToList();
        }

        /// <summary>
        /// Limpa o inventário
        /// </summary>
        public void ClearInventory()
        {
            allItems.Clear();
            weapons.Clear();
            shipParts.Clear();
            consumables.Clear();
            questItems.Clear();
            equippedItems.Clear();
            equippedWeapon = null;
            equippedShield = null;
            equippedEngine = null;
            OnInventoryChanged?.Invoke();
        }

        // Notificações
        private void NotifyItemAdded(InventoryItem item)
        {
            OnItemAdded?.Invoke(item);
            OnInventoryChanged?.Invoke();
            Debug.Log($"Added to inventory: {item.itemData.itemName} x{item.quantity}");
        }

        private void NotifyItemRemoved(InventoryItem item)
        {
            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
            Debug.Log($"Removed from inventory: {item.itemData.itemName}");
        }

        // Getters públicos
        public int GetMaxSlots() => maxSlots;
        public float GetMaxWeight() => maxWeight;
        public int GetCurrentCredits() => currentCredits;
        public List<InventoryItem> GetAllItems() => new List<InventoryItem>(allItems);
        public List<InventoryItem> GetEquippedItems() => new List<InventoryItem>(equippedItems);
        public InventoryItem GetEquippedWeapon() => equippedWeapon;
        public InventoryItem GetEquippedShield() => equippedShield;
        public InventoryItem GetEquippedEngine() => equippedEngine;

        /// <summary>
        /// Retorna estatísticas do inventário
        /// </summary>
        public string GetInventoryStats()
        {
            return $"Items: {allItems.Count}/{maxSlots}\n" +
                   $"Weight: {GetCurrentWeight():F1}/{maxWeight}\n" +
                   $"Credits: {currentCredits}\n" +
                   $"Weapons: {weapons.Count}\n" +
                   $"Ship Parts: {shipParts.Count}\n" +
                   $"Consumables: {consumables.Count}\n" +
                   $"Quest Items: {questItems.Count}\n" +
                   $"Equipped: {equippedItems.Count}";
        }
    }
}

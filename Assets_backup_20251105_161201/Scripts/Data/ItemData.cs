using UnityEngine;
using System;

namespace SpaceRPG.Data
{
    /// <summary>
    /// Tipos de itens disponíveis no sistema
    /// </summary>
    public enum ItemType
    {
        Weapon,
        ShipPart,
        Consumable,
        QuestItem,
        Material,
        Seed,
        PlantCare,
        Tool,
        Currency
    }

    /// <summary>
    /// Raridade dos itens
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Categoria de armas
    /// </summary>
    public enum WeaponCategory
    {
        Laser,
        Plasma,
        Missile,
        Beam,
        Special
    }

    /// <summary>
    /// Dados base de um item no sistema
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Space RPG/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Information")]
        public string itemID;
        public string itemName;
        [TextArea(3, 6)]
        public string description;
        public Sprite icon;
        public GameObject prefab;

        [Header("Item Properties")]
        public ItemType itemType;
        public ItemRarity rarity;
        public bool isStackable = true;
        public int maxStackSize = 99;
        public float weight = 1f;

        [Header("Economic")]
        public int buyPrice;
        public int sellPrice;
        public bool canBeSold = true;
        public bool canBeBought = true;

        [Header("Usage")]
        public bool isConsumable;
        public bool isEquippable;
        public int usageCount = 1; // -1 para infinito

        [Header("Stats (for equipment)")]
        public float damageBonus;
        public float defenseBonus;
        public float speedBonus;
        public float energyBonus;

        [Header("Weapon Specific")]
        public WeaponCategory weaponCategory;
        public float fireRate;
        public float range;
        public int ammoCapacity;

        [Header("Plant Care Specific")]
        public float healthRestoreAmount;
        public float growthBoost;
        public bool removesPests;

        [Header("Audio")]
        public AudioClip useSound;
        public AudioClip equipSound;
        public AudioClip dropSound;

        [Header("Visual Effects")]
        public GameObject useEffect;
        public Color rarityColor = Color.white;

        /// <summary>
        /// Obtém a cor baseada na raridade do item
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return new Color(0.8f, 0.8f, 0.8f); // Cinza
                case ItemRarity.Uncommon:
                    return new Color(0.2f, 1f, 0.2f); // Verde
                case ItemRarity.Rare:
                    return new Color(0.2f, 0.5f, 1f); // Azul
                case ItemRarity.Epic:
                    return new Color(0.8f, 0.2f, 1f); // Roxo
                case ItemRarity.Legendary:
                    return new Color(1f, 0.6f, 0f); // Laranja
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Retorna uma descrição detalhada do item
        /// </summary>
        public string GetDetailedDescription()
        {
            string details = $"<b>{itemName}</b>\n";
            details += $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}>{rarity}</color>\n\n";
            details += $"{description}\n\n";

            if (itemType == ItemType.Weapon)
            {
                details += "<b>Weapon Stats:</b>\n";
                details += $"Damage: +{damageBonus}\n";
                details += $"Fire Rate: {fireRate}/s\n";
                details += $"Range: {range}m\n";
                details += $"Ammo: {ammoCapacity}\n\n";
            }

            if (isEquippable)
            {
                details += "<b>Equipment Bonuses:</b>\n";
                if (damageBonus > 0) details += $"Damage: +{damageBonus}\n";
                if (defenseBonus > 0) details += $"Defense: +{defenseBonus}\n";
                if (speedBonus > 0) details += $"Speed: +{speedBonus}\n";
                if (energyBonus > 0) details += $"Energy: +{energyBonus}\n\n";
            }

            details += $"<b>Weight:</b> {weight} kg\n";
            details += $"<b>Value:</b> {buyPrice} credits";

            return details;
        }

        /// <summary>
        /// Valida se o item pode ser usado
        /// </summary>
        public bool CanUse()
        {
            return isConsumable || isEquippable;
        }

        /// <summary>
        /// Calcula o valor total baseado na quantidade
        /// </summary>
        public int GetTotalValue(int quantity)
        {
            return sellPrice * quantity;
        }
    }

    /// <summary>
    /// Representa uma instância de item no inventário
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        public ItemData itemData;
        public int quantity;
        public string instanceID; // ID único desta instância
        public bool isEquipped;
        public float durability = 100f; // Para itens que degradam

        public InventoryItem(ItemData data, int qty = 1)
        {
            itemData = data;
            quantity = qty;
            instanceID = System.Guid.NewGuid().ToString();
            isEquipped = false;
            durability = 100f;
        }

        /// <summary>
        /// Adiciona quantidade ao item
        /// </summary>
        public bool AddQuantity(int amount)
        {
            if (!itemData.isStackable && quantity > 0)
                return false;

            int newQuantity = quantity + amount;
            if (newQuantity > itemData.maxStackSize)
                return false;

            quantity = newQuantity;
            return true;
        }

        /// <summary>
        /// Remove quantidade do item
        /// </summary>
        public bool RemoveQuantity(int amount)
        {
            if (quantity < amount)
                return false;

            quantity -= amount;
            return true;
        }

        /// <summary>
        /// Verifica se o item está quebrado
        /// </summary>
        public bool IsBroken()
        {
            return durability <= 0f;
        }

        /// <summary>
        /// Usa o item (reduz durabilidade se aplicável)
        /// </summary>
        public void Use()
        {
            if (itemData.isConsumable)
            {
                quantity--;
            }
            else
            {
                durability -= 1f; // Degradação leve
            }
        }

        /// <summary>
        /// Repara o item
        /// </summary>
        public void Repair(float amount)
        {
            durability = Mathf.Min(100f, durability + amount);
        }

        /// <summary>
        /// Obtém o peso total do item
        /// </summary>
        public float GetTotalWeight()
        {
            return itemData.weight * quantity;
        }

        /// <summary>
        /// Clona o item
        /// </summary>
        public InventoryItem Clone()
        {
            return new InventoryItem(itemData, quantity)
            {
                instanceID = System.Guid.NewGuid().ToString(),
                isEquipped = false,
                durability = this.durability
            };
        }
    }
}

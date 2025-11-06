using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SpaceRPG.Systems
{
    [System.Serializable]
    public class ShipData
    {
        public string shipID;
        public string shipName;
        public GameObject shipPrefab;
        public Sprite icon;

        [Header("Stats")]
        public float maxSpeed = 10f;
        public float acceleration = 5f;
        public float handling = 3f;
        public float maxHealth = 100f;
        public float armor = 10f;
        public float shieldCapacity = 50f;
        public float energyCapacity = 100f;

        [Header("Weapons")]
        public int weaponSlots = 2;
        public int maxFirepower = 50;

        [Header("Economic")]
        public int buyPrice = 1000;
        public int sellPrice = 500;
        public bool isUnlocked = false;

        public ShipData(string id, string name)
        {
            shipID = id;
            shipName = name;
        }

        public string GetStatsText()
        {
            return $"Speed: {maxSpeed}\n" +
                   $"Armor: {armor}\n" +
                   $"Health: {maxHealth}\n" +
                   $"Shield: {shieldCapacity}\n" +
                   $"Energy: {energyCapacity}\n" +
                   $"Weapon Slots: {weaponSlots}";
        }
    }

    public class ShipSystem : MonoBehaviour
    {
        private static ShipSystem _instance;
        public static ShipSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ShipSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ShipSystem");
                        _instance = go.AddComponent<ShipSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Available Ships")]
        [SerializeField] private List<ShipData> allShips = new List<ShipData>();
        [SerializeField] private List<ShipData> ownedShips = new List<ShipData>();

        [Header("Current Ship")]
        [SerializeField] private ShipData currentShip;
        [SerializeField] private GameObject currentShipInstance;

        [Header("Customization")]
        [SerializeField] private Color primaryColor = Color.white;
        [SerializeField] private Color secondaryColor = Color.gray;
        [SerializeField] private List<string> appliedDecals = new List<string>();

        [Header("Ship Health")]
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float currentShield = 50f;
        [SerializeField] private float damageLevel = 0f; // 0-100, quanto maior mais danificada

        public event Action<ShipData> OnShipChanged;
        public event Action<ShipData> OnShipPurchased;
        public event Action<float> OnShipDamaged;
        public event Action OnShipRepaired;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeShips();
        }

        private void InitializeShips()
        {
            // Criar naves de exemplo
            ShipData shuttle = new ShipData("ship_shuttle", "Space Shuttle")
            {
                maxSpeed = 8f,
                maxHealth = 150f,
                armor = 20f,
                weaponSlots = 1,
                buyPrice = 0,
                isUnlocked = true
            };
            allShips.Add(shuttle);
            ownedShips.Add(shuttle);

            ShipData fighter = new ShipData("ship_fighter", "Omega Fighter")
            {
                maxSpeed = 15f,
                maxHealth = 80f,
                armor = 5f,
                weaponSlots = 3,
                buyPrice = 2000,
                isUnlocked = false
            };
            allShips.Add(fighter);

            ShipData cruiser = new ShipData("ship_cruiser", "Star Cruiser")
            {
                maxSpeed = 6f,
                maxHealth = 200f,
                armor = 30f,
                weaponSlots = 2,
                buyPrice = 5000,
                isUnlocked = false
            };
            allShips.Add(cruiser);

            // Definir nave inicial
            if (currentShip == null && ownedShips.Count > 0)
            {
                ChangeShip(ownedShips[0].shipID);
            }

            Debug.Log($"Ship System initialized with {allShips.Count} ships");
        }

        public bool ChangeShip(string shipID)
        {
            ShipData newShip = ownedShips.FirstOrDefault(s => s.shipID == shipID);
            if (newShip == null)
            {
                Debug.LogWarning($"Ship not owned: {shipID}");
                return false;
            }

            // Destruir instância anterior
            if (currentShipInstance != null)
            {
                Destroy(currentShipInstance);
            }

            // Definir nova nave
            currentShip = newShip;

            // Instanciar prefab se disponível
            if (newShip.shipPrefab != null)
            {
                currentShipInstance = Instantiate(newShip.shipPrefab);
                currentShipInstance.name = newShip.shipName;
            }

            // Resetar saúde
            currentHealth = newShip.maxHealth;
            currentShield = newShip.shieldCapacity;
            damageLevel = 0f;

            OnShipChanged?.Invoke(newShip);
            Debug.Log($"Changed to ship: {newShip.shipName}");
            return true;
        }

        public bool PurchaseShip(string shipID)
        {
            ShipData ship = allShips.FirstOrDefault(s => s.shipID == shipID);
            if (ship == null)
            {
                Debug.LogWarning($"Ship not found: {shipID}");
                return false;
            }

            if (ownedShips.Contains(ship))
            {
                Debug.LogWarning("Ship already owned");
                return false;
            }

            if (InventorySystem.Instance == null || InventorySystem.Instance.GetCurrentCredits() < ship.buyPrice)
            {
                Debug.LogWarning("Not enough credits");
                return false;
            }

            // Comprar nave
            InventorySystem.Instance.RemoveCredits(ship.buyPrice);
            ship.isUnlocked = true;
            ownedShips.Add(ship);

            OnShipPurchased?.Invoke(ship);
            Debug.Log($"Purchased ship: {ship.shipName}");
            return true;
        }

        public bool SellShip(string shipID)
        {
            ShipData ship = ownedShips.FirstOrDefault(s => s.shipID == shipID);
            if (ship == null)
            {
                Debug.LogWarning("Ship not owned");
                return false;
            }

            if (ship == currentShip && ownedShips.Count == 1)
            {
                Debug.LogWarning("Cannot sell last ship");
                return false;
            }

            // Se for a nave atual, trocar para outra
            if (ship == currentShip)
            {
                var otherShip = ownedShips.FirstOrDefault(s => s != currentShip);
                if (otherShip != null)
                {
                    ChangeShip(otherShip.shipID);
                }
            }

            // Vender nave
            InventorySystem.Instance.AddCredits(ship.sellPrice);
            ownedShips.Remove(ship);
            ship.isUnlocked = false;

            Debug.Log($"Sold ship: {ship.shipName}");
            return true;
        }

        public void TakeDamage(float amount)
        {
            // Primeiro reduz shield
            if (currentShield > 0)
            {
                float shieldDamage = Mathf.Min(amount, currentShield);
                currentShield -= shieldDamage;
                amount -= shieldDamage;
            }

            // Depois reduz health
            if (amount > 0)
            {
                currentHealth -= amount;
                currentHealth = Mathf.Max(0, currentHealth);

                // Atualizar nível de dano
                damageLevel = 100f * (1f - currentHealth / currentShip.maxHealth);
            }

            OnShipDamaged?.Invoke(amount);

            if (currentHealth <= 0)
            {
                Debug.Log("Ship destroyed!");
            }
        }

        public bool RepairShip(float repairAmount)
        {
            if (currentHealth >= currentShip.maxHealth)
            {
                Debug.Log("Ship already at full health");
                return false;
            }

            currentHealth = Mathf.Min(currentHealth + repairAmount, currentShip.maxHealth);
            damageLevel = 100f * (1f - currentHealth / currentShip.maxHealth);

            OnShipRepaired?.Invoke();
            Debug.Log($"Ship repaired: +{repairAmount} HP");
            return true;
        }

        public void SetShipColor(Color primary, Color secondary)
        {
            primaryColor = primary;
            secondaryColor = secondary;

            // Aplicar cores ao modelo
            if (currentShipInstance != null)
            {
                Renderer[] renderers = currentShipInstance.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    // Implementar lógica de colorização
                }
            }
        }

        public List<ShipData> GetOwnedShips() => new List<ShipData>(ownedShips);
        public List<ShipData> GetAllShips() => new List<ShipData>(allShips);
        public ShipData GetCurrentShip() => currentShip;
        public float GetCurrentHealth() => currentHealth;
        public float GetCurrentShield() => currentShield;
        public float GetDamageLevel() => damageLevel;
        public GameObject GetCurrentShipInstance() => currentShipInstance;

        public string GetShipStats()
        {
            if (currentShip == null) return "No ship equipped";

            return $"Current Ship: {currentShip.shipName}\n" +
                   $"Health: {currentHealth:F0}/{currentShip.maxHealth}\n" +
                   $"Shield: {currentShield:F0}/{currentShip.shieldCapacity}\n" +
                   $"Damage Level: {damageLevel:F0}%\n" +
                   $"Owned Ships: {ownedShips.Count}\n" +
                   $"\n{currentShip.GetStatsText()}";
        }
    }
}

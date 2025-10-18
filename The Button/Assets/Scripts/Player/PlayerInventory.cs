using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TheButton.Items;
using TheButton.Network;

namespace TheButton.Player
{
    /// <summary>
    /// Manages player inventory (4-5 slots)
    /// Handles item storage, usage, and synchronization across network
    /// Uses ItemData ScriptableObject references
    /// </summary>
    public class PlayerInventory : NetworkBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 5;
        
        [Header("References")]
        [Tooltip("Reference to PlayerNetwork for stat modifications")]
        [SerializeField] private PlayerNetwork playerNetwork;

        // Network sync: Store asset names of items
        private NetworkList<NetworkString> inventoryItemNames;
        
        // Local cache: Actual ItemData references
        private List<ItemData> inventoryItems = new List<ItemData>();
        
        // Selected slot (local only, no need to sync)
        private int selectedSlotIndex = 0;

        public event System.Action OnInventoryChanged;
        public event System.Action<int> OnSelectedSlotChanged;
        
        // Event fired when a key is used (for door interaction)
        public event System.Action OnKeyUsed;

        private void Awake()
        {
            inventoryItemNames = new NetworkList<NetworkString>();
            
            // Auto-find PlayerNetwork if not assigned
            if (playerNetwork == null)
            {
                playerNetwork = GetComponent<PlayerNetwork>();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsOwner)
            {
                inventoryItemNames.OnListChanged += OnInventoryListChanged;
            }
            
            // Initial load of inventory from network list
            RebuildLocalInventory();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsOwner)
            {
                inventoryItemNames.OnListChanged -= OnInventoryListChanged;
            }
        }

        /// <summary>
        /// Attempt to add an item to inventory
        /// </summary>
        /// <param name="itemDataAssetName">The asset name of the ItemData to add</param>
        [ServerRpc(RequireOwnership = false)]
        public void AddItemServerRpc(string itemDataAssetName)
        {
            if (inventoryItemNames.Count >= maxSlots)
            {
                Debug.LogWarning($"[Inventory] Inventory is full! Cannot add item {itemDataAssetName}");
                return;
            }

            inventoryItemNames.Add(new NetworkString(itemDataAssetName));
            Debug.Log($"[Inventory] Added item {itemDataAssetName} to inventory");
        }

        /// <summary>
        /// Remove an item from inventory by slot index
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RemoveItemAtSlotServerRpc(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventoryItemNames.Count)
            {
                Debug.LogWarning($"[Inventory] Invalid slot index: {slotIndex}");
                return;
            }

            string itemName = inventoryItemNames[slotIndex].ToString();
            inventoryItemNames.RemoveAt(slotIndex);
            Debug.Log($"[Inventory] Removed item {itemName} from slot {slotIndex}");
        }

        /// <summary>
        /// Use/consume an item from inventory
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void UseItemServerRpc(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventoryItemNames.Count)
            {
                Debug.LogWarning($"[Inventory] Invalid slot index: {slotIndex}");
                return;
            }

            string itemDataAssetName = inventoryItemNames[slotIndex].ToString();
            
            // Load item data from Resources
            ItemData itemData = Resources.Load<ItemData>($"Items/{itemDataAssetName}");
            if (itemData == null)
            {
                Debug.LogWarning($"[Inventory] Item '{itemDataAssetName}' not found in Resources/Items/!");
                inventoryItemNames.RemoveAt(slotIndex);
                return;
            }
            
            Debug.Log($"[Inventory] Using item {itemData.itemName} from slot {slotIndex}");
            
            // Apply item effects based on category
            bool consumeItem = false;
            
            switch (itemData.category)
            {
                case ItemCategory.Consumable:
                    UseConsumable(itemData);
                    consumeItem = true;  // Consumables are removed after use
                    break;
                    
                case ItemCategory.Key:
                    UseKey(itemData);
                    consumeItem = true;  // Keys are consumed when used on doors
                    break;
                    
                case ItemCategory.Usable:
                    UseUsable(itemData);
                    consumeItem = false;  // Usables stay in inventory
                    break;
                    
                case ItemCategory.Collectible:
                    // Collectibles need to be placed in world, not "used" from inventory
                    Debug.Log($"[Inventory] {itemData.itemName} is a collectible. Use place mode to place it.");
                    consumeItem = false;
                    break;
                    
                default:
                    Debug.LogWarning($"[Inventory] Unknown item category: {itemData.category}");
                    break;
            }
            
            // Remove the item from inventory if it was consumed
            if (consumeItem)
            {
                inventoryItemNames.RemoveAt(slotIndex);
            }
        }
        
        /// <summary>
        /// Use a consumable item (food, water, medkit, etc.)
        /// </summary>
        private void UseConsumable(ItemData itemData)
        {
            if (playerNetwork == null) return;
            
            // Apply health effects
            if (itemData.healthRestore > 0)
            {
                playerNetwork.ModifyHealthServerRpc(itemData.healthRestore);
                Debug.Log($"[Inventory] Restored {itemData.healthRestore} health");
            }
            
            // Apply hunger effects
            if (itemData.hungerRestore > 0)
            {
                playerNetwork.ModifyHungerServerRpc(itemData.hungerRestore);
                Debug.Log($"[Inventory] Restored {itemData.hungerRestore} hunger");
            }
            
            // Apply thirst effects
            if (itemData.thirstRestore > 0)
            {
                playerNetwork.ModifyThirstServerRpc(itemData.thirstRestore);
                Debug.Log($"[Inventory] Restored {itemData.thirstRestore} thirst");
            }
            
            // Apply stamina effects
            if (itemData.staminaRestore > 0)
            {
                playerNetwork.ModifyStaminaServerRpc(itemData.staminaRestore);
                Debug.Log($"[Inventory] Restored {itemData.staminaRestore} stamina");
            }
            
            // Apply damage (for poison/hazard consumables)
            if (itemData.damageAmount > 0)
            {
                playerNetwork.ModifyHealthServerRpc(-itemData.damageAmount);
                Debug.Log($"[Inventory] Took {itemData.damageAmount} damage!");
            }
        }
        
        /// <summary>
        /// Use a key item
        /// </summary>
        private void UseKey(ItemData itemData)
        {
            Debug.Log($"[Inventory] Using key: {itemData.itemName}");
            NotifyKeyUsedClientRpc();
        }
        
        /// <summary>
        /// Use a usable item (tools, etc.)
        /// </summary>
        private void UseUsable(ItemData itemData)
        {
            Debug.Log($"[Inventory] Using tool: {itemData.itemName}");
            // TODO: Implement tool usage logic
            // For now, just log
            // Future: Enable hand model, allow interactions, etc.
        }
        
        [ClientRpc]
        private void NotifyKeyUsedClientRpc()
        {
            OnKeyUsed?.Invoke();
        }

        /// <summary>
        /// Get the number of items in inventory
        /// </summary>
        public int GetItemCount()
        {
            return inventoryItemNames.Count;
        }

        /// <summary>
        /// Check if inventory is full
        /// </summary>
        public bool IsFull()
        {
            return inventoryItemNames.Count >= maxSlots;
        }

        /// <summary>
        /// Get ItemData at specific slot (from local cache)
        /// </summary>
        public ItemData GetItemAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventoryItems.Count)
                return null;
            
            return inventoryItems[slotIndex];
        }

        /// <summary>
        /// Get all items in inventory (from local cache)
        /// </summary>
        public List<ItemData> GetAllItems()
        {
            return new List<ItemData>(inventoryItems);
        }

        /// <summary>
        /// Clear all items from inventory
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ClearInventoryServerRpc()
        {
            inventoryItemNames.Clear();
            Debug.Log("[Inventory] Cleared inventory");
        }

        private void OnInventoryListChanged(NetworkListEvent<NetworkString> changeEvent)
        {
            RebuildLocalInventory();
            OnInventoryChanged?.Invoke();
        }
        
        /// <summary>
        /// Rebuild local inventory cache from network list
        /// </summary>
        private void RebuildLocalInventory()
        {
            inventoryItems.Clear();
            
            foreach (var itemName in inventoryItemNames)
            {
                string assetName = itemName.ToString();
                ItemData itemData = Resources.Load<ItemData>($"Items/{assetName}");
                
                if (itemData != null)
                {
                    inventoryItems.Add(itemData);
                }
                else
                {
                    Debug.LogWarning($"[Inventory] Failed to load ItemData from Resources/Items/{assetName}");
                    inventoryItems.Add(null); // Keep slot count consistent
                }
            }
        }
        
        /// <summary>
        /// Check if player has a specific item type
        /// </summary>
        public bool HasItemOfType(ItemType itemType)
        {
            foreach (var itemData in inventoryItems)
            {
                if (itemData != null && itemData.itemType == itemType)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Check if player has a specific item category
        /// </summary>
        public bool HasItemOfCategory(ItemCategory category)
        {
            foreach (var itemData in inventoryItems)
            {
                if (itemData != null && itemData.category == category)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Get the first item of a specific type (returns slot index)
        /// </summary>
        public int GetFirstItemOfType(ItemType itemType)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i] != null && inventoryItems[i].itemType == itemType)
                {
                    return i; // Return slot index
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Get the first item of a specific category (returns slot index)
        /// </summary>
        public int GetFirstItemOfCategory(ItemCategory category)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i] != null && inventoryItems[i].category == category)
                {
                    return i; // Return slot index
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Check if player has a specific item by asset name
        /// </summary>
        public bool HasItem(string itemAssetName)
        {
            foreach (var itemData in inventoryItems)
            {
                if (itemData != null && itemData.name == itemAssetName)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Get the first slot containing a specific item by asset name (returns slot index)
        /// </summary>
        public int GetFirstItemSlot(string itemAssetName)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i] != null && inventoryItems[i].name == itemAssetName)
                {
                    return i; // Return slot index
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Set the selected slot (0-4)
        /// </summary>
        public void SetSelectedSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots)
            {
                Debug.LogWarning($"[Inventory] Invalid slot index: {slotIndex}");
                return;
            }
            
            selectedSlotIndex = slotIndex;
            OnSelectedSlotChanged?.Invoke(selectedSlotIndex);
            Debug.Log($"[Inventory] Selected slot: {selectedSlotIndex}");
        }
        
        /// <summary>
        /// Get the currently selected slot index
        /// </summary>
        public int GetSelectedSlot()
        {
            return selectedSlotIndex;
        }
        
        /// <summary>
        /// Get the item in the currently selected slot
        /// </summary>
        public ItemData GetSelectedItem()
        {
            return GetItemAtSlot(selectedSlotIndex);
        }
        
        /// <summary>
        /// Drop the item in the currently selected slot
        /// </summary>
        public void DropSelectedItem()
        {
            if (selectedSlotIndex >= inventoryItems.Count) return;
            if (inventoryItems[selectedSlotIndex] == null) return;
            
            DropItemServerRpc(selectedSlotIndex);
        }
        
        /// <summary>
        /// Drop an item from inventory into the world
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void DropItemServerRpc(int slotIndex, ServerRpcParams rpcParams = default)
        {
            if (slotIndex < 0 || slotIndex >= inventoryItemNames.Count)
            {
                Debug.LogWarning($"[Inventory] Invalid slot index: {slotIndex}");
                return;
            }
            
            string itemDataAssetName = inventoryItemNames[slotIndex].ToString();
            ItemData itemData = Resources.Load<ItemData>($"Items/{itemDataAssetName}");
            
            if (itemData == null)
            {
                Debug.LogError($"[Inventory] Failed to load ItemData: {itemDataAssetName}");
                return;
            }
            
            // Get player object
            ulong clientId = rpcParams.Receive.SenderClientId;
            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                Debug.LogError($"[Inventory] Client {clientId} not found");
                return;
            }
            
            var playerObject = client.PlayerObject;
            if (playerObject == null)
            {
                Debug.LogError($"[Inventory] Player object not found for client {clientId}");
                return;
            }
            
            // Calculate drop position (in front of player)
            Vector3 dropPosition = playerObject.transform.position + playerObject.transform.forward * 2f + Vector3.up * 1f;
            Quaternion dropRotation = Quaternion.identity;
            
            // Spawn the item in the world
            if (ItemSpawner.Instance != null)
            {
                ItemSpawner.Instance.SpawnItem(itemData, dropPosition, dropRotation);
                Debug.Log($"[Inventory] Dropped item {itemData.itemName} at {dropPosition}");
            }
            else
            {
                Debug.LogError("[Inventory] ItemSpawner instance not found!");
            }
            
            // Remove from inventory
            inventoryItemNames.RemoveAt(slotIndex);
        }
    }
}

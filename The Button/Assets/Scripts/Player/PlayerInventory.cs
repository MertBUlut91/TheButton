using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TheButton.Items;

namespace TheButton.Player
{
    /// <summary>
    /// Manages player inventory (4-5 slots)
    /// Handles item storage, usage, and synchronization across network
    /// </summary>
    public class PlayerInventory : NetworkBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 5;
        
        [Header("References")]
        [Tooltip("Reference to PlayerNetwork for stat modifications")]
        [SerializeField] private PlayerNetwork playerNetwork;

        // Network list for synchronized inventory
        private NetworkList<int> inventoryItems;

        public event System.Action OnInventoryChanged;
        
        // Event fired when a key is used (for door interaction)
        public event System.Action OnKeyUsed;

        private void Awake()
        {
            inventoryItems = new NetworkList<int>();
            
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
                inventoryItems.OnListChanged += OnInventoryListChanged;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsOwner)
            {
                inventoryItems.OnListChanged -= OnInventoryListChanged;
            }
        }

        /// <summary>
        /// Attempt to add an item to inventory
        /// </summary>
        /// <param name="itemId">The ID of the item to add</param>
        /// <returns>True if item was added successfully</returns>
        [ServerRpc(RequireOwnership = false)]
        public void AddItemServerRpc(int itemId)
        {
            if (inventoryItems.Count >= maxSlots)
            {
                Debug.LogWarning($"[Inventory] Inventory is full! Cannot add item {itemId}");
                return;
            }

            inventoryItems.Add(itemId);
            Debug.Log($"[Inventory] Added item {itemId} to inventory");
        }

        /// <summary>
        /// Remove an item from inventory by slot index
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RemoveItemAtSlotServerRpc(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventoryItems.Count)
            {
                Debug.LogWarning($"[Inventory] Invalid slot index: {slotIndex}");
                return;
            }

            int itemId = inventoryItems[slotIndex];
            inventoryItems.RemoveAt(slotIndex);
            Debug.Log($"[Inventory] Removed item {itemId} from slot {slotIndex}");
        }

        /// <summary>
        /// Use/consume an item from inventory
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void UseItemServerRpc(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventoryItems.Count)
            {
                Debug.LogWarning($"[Inventory] Invalid slot index: {slotIndex}");
                return;
            }

            int itemId = inventoryItems[slotIndex];
            
            // Get item data from database
            ItemData itemData = ItemDatabase.Instance?.GetItem(itemId);
            if (itemData == null)
            {
                Debug.LogWarning($"[Inventory] Item {itemId} not found in database!");
                inventoryItems.RemoveAt(slotIndex);
                return;
            }
            
            Debug.Log($"[Inventory] Using item {itemData.itemName} (ID: {itemId}) from slot {slotIndex}");
            
            // Apply item effects based on type
            bool consumeItem = true;
            
            switch (itemData.itemType)
            {
                case ItemType.Medkit:
                    if (playerNetwork != null)
                    {
                        playerNetwork.ModifyHealthServerRpc(itemData.healthRestore);
                        Debug.Log($"[Inventory] Restored {itemData.healthRestore} health");
                    }
                    break;
                    
                case ItemType.Food:
                    if (playerNetwork != null)
                    {
                        playerNetwork.ModifyHungerServerRpc(itemData.hungerRestore);
                        Debug.Log($"[Inventory] Restored {itemData.hungerRestore} hunger");
                    }
                    break;
                    
                case ItemType.Water:
                    if (playerNetwork != null)
                    {
                        playerNetwork.ModifyThirstServerRpc(itemData.thirstRestore);
                        Debug.Log($"[Inventory] Restored {itemData.thirstRestore} thirst");
                    }
                    break;
                    
                case ItemType.Key:
                    // Notify that key was used (door will listen to this)
                    NotifyKeyUsedClientRpc();
                    Debug.Log($"[Inventory] Key used");
                    break;
                    
                case ItemType.Hazard:
                    if (playerNetwork != null)
                    {
                        playerNetwork.ModifyHealthServerRpc(-itemData.damageAmount);
                        Debug.Log($"[Inventory] Took {itemData.damageAmount} damage from hazard item!");
                    }
                    break;
                    
                default:
                    Debug.LogWarning($"[Inventory] Unknown item type: {itemData.itemType}");
                    break;
            }
            
            // Remove the item from inventory if it was consumed
            if (consumeItem)
            {
                inventoryItems.RemoveAt(slotIndex);
            }
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
            return inventoryItems.Count;
        }

        /// <summary>
        /// Check if inventory is full
        /// </summary>
        public bool IsFull()
        {
            return inventoryItems.Count >= maxSlots;
        }

        /// <summary>
        /// Get item ID at specific slot
        /// </summary>
        public int GetItemAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventoryItems.Count)
                return -1;
            
            return inventoryItems[slotIndex];
        }

        /// <summary>
        /// Get all items in inventory
        /// </summary>
        public List<int> GetAllItems()
        {
            List<int> items = new List<int>();
            foreach (var item in inventoryItems)
            {
                items.Add(item);
            }
            return items;
        }

        /// <summary>
        /// Clear all items from inventory
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ClearInventoryServerRpc()
        {
            inventoryItems.Clear();
            Debug.Log("[Inventory] Cleared inventory");
        }

        private void OnInventoryListChanged(NetworkListEvent<int> changeEvent)
        {
            OnInventoryChanged?.Invoke();
        }
        
        /// <summary>
        /// Check if player has a specific item type
        /// </summary>
        public bool HasItemOfType(ItemType itemType)
        {
            foreach (int itemId in inventoryItems)
            {
                ItemData itemData = ItemDatabase.Instance?.GetItem(itemId);
                if (itemData != null && itemData.itemType == itemType)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Get the first item of a specific type
        /// </summary>
        public int GetFirstItemOfType(ItemType itemType)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                ItemData itemData = ItemDatabase.Instance?.GetItem(inventoryItems[i]);
                if (itemData != null && itemData.itemType == itemType)
                {
                    return i; // Return slot index
                }
            }
            return -1;
        }
    }
}


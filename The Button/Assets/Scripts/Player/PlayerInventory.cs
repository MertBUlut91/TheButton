using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TheButton.Player
{
    /// <summary>
    /// Manages player inventory (4-5 slots)
    /// This is a basic structure for future item system implementation
    /// </summary>
    public class PlayerInventory : NetworkBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 5;

        // Network list for synchronized inventory
        private NetworkList<int> inventoryItems;

        // Local cache for item references (itemId -> itemData)
        private Dictionary<int, object> itemCache = new Dictionary<int, object>();

        public event System.Action OnInventoryChanged;

        private void Awake()
        {
            inventoryItems = new NetworkList<int>();
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
            Debug.Log($"[Inventory] Using item {itemId} from slot {slotIndex}");

            // TODO: Implement item usage logic here
            // This will be expanded when item system is implemented
            // Example:
            // - If medkit: restore health
            // - If food: restore hunger
            // - If water: restore thirst
            // - If key: unlock door

            // For now, just remove the item after use
            inventoryItems.RemoveAt(slotIndex);
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
    }
}


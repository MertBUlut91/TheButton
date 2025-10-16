using System.Collections.Generic;
using UnityEngine;

namespace TheButton.Items
{
    /// <summary>
    /// Singleton manager that holds all item data
    /// Allows lookup of ItemData by itemId
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "TheButton/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        private static ItemDatabase instance;
        
        [Header("Item Database")]
        [Tooltip("List of all items in the game")]
        public List<ItemData> items = new List<ItemData>();
        
        private Dictionary<int, ItemData> itemDictionary;
        
        /// <summary>
        /// Get the singleton instance (loads from Resources if needed)
        /// </summary>
        public static ItemDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<ItemDatabase>("ItemDatabase");
                    if (instance == null)
                    {
                        Debug.LogError("[ItemDatabase] ItemDatabase not found in Resources folder! " +
                            "Please create one at Assets/Resources/ItemDatabase.asset");
                    }
                    else
                    {
                        instance.Initialize();
                    }
                }
                return instance;
            }
        }
        
        /// <summary>
        /// Initialize the item dictionary for fast lookup
        /// </summary>
        private void Initialize()
        {
            itemDictionary = new Dictionary<int, ItemData>();
            foreach (var item in items)
            {
                if (item != null)
                {
                    if (itemDictionary.ContainsKey(item.itemId))
                    {
                        Debug.LogWarning($"[ItemDatabase] Duplicate item ID {item.itemId} found! " +
                            $"Items: {itemDictionary[item.itemId].itemName} and {item.itemName}");
                    }
                    else
                    {
                        itemDictionary[item.itemId] = item;
                    }
                }
            }
            Debug.Log($"[ItemDatabase] Initialized with {itemDictionary.Count} items");
        }
        
        /// <summary>
        /// Get item data by ID
        /// </summary>
        public ItemData GetItem(int itemId)
        {
            if (itemDictionary == null || itemDictionary.Count == 0)
                Initialize();
            
            if (itemDictionary.TryGetValue(itemId, out ItemData itemData))
            {
                return itemData;
            }
            
            Debug.LogWarning($"[ItemDatabase] Item with ID {itemId} not found!");
            return null;
        }
        
        /// <summary>
        /// Check if an item exists in the database
        /// </summary>
        public bool HasItem(int itemId)
        {
            if (itemDictionary == null || itemDictionary.Count == 0)
                Initialize();
            
            return itemDictionary.ContainsKey(itemId);
        }
        
        /// <summary>
        /// Get all items of a specific type
        /// </summary>
        public List<ItemData> GetItemsByType(ItemType type)
        {
            List<ItemData> result = new List<ItemData>();
            foreach (var item in items)
            {
                if (item != null && item.itemType == type)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Validate item IDs in the editor
        /// </summary>
        private void OnValidate()
        {
            // Check for duplicate IDs
            HashSet<int> seenIds = new HashSet<int>();
            foreach (var item in items)
            {
                if (item != null)
                {
                    if (seenIds.Contains(item.itemId))
                    {
                        Debug.LogError($"[ItemDatabase] Duplicate item ID {item.itemId} detected!");
                    }
                    seenIds.Add(item.itemId);
                }
            }
        }
#endif
    }
}


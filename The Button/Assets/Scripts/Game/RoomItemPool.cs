using System.Collections.Generic;
using UnityEngine;
using TheButton.Items;

namespace TheButton.Game
{
    /// <summary>
    /// Pool of items that can be spawned in the procedural room
    /// Contains required items (must spawn) and random items (optional)
    /// </summary>
    [CreateAssetMenu(fileName = "RoomItemPool", menuName = "The Button/Room Item Pool")]
    public class RoomItemPool : ScriptableObject
    {
        [Header("Required Items")]
        [Tooltip("Items that MUST spawn in every room (e.g., key, door opener)")]
        public List<ItemData> requiredItems = new List<ItemData>();
        
        [Header("Random Item Pool")]
        [Tooltip("Items that can randomly spawn on buttons")]
        public List<ItemData> randomItemPool = new List<ItemData>();
        
        /// <summary>
        /// Get a random item from the pool
        /// </summary>
        public ItemData GetRandomItem()
        {
            if (randomItemPool == null || randomItemPool.Count == 0)
            {
                Debug.LogWarning("[RoomItemPool] Random item pool is empty!");
                return null;
            }
            
            int randomIndex = Random.Range(0, randomItemPool.Count);
            return randomItemPool[randomIndex];
        }
        
        /// <summary>
        /// Validate that all required items are assigned
        /// </summary>
        public bool Validate()
        {
            if (requiredItems == null || requiredItems.Count == 0)
            {
                Debug.LogWarning("[RoomItemPool] No required items defined!");
                return false;
            }
            
            if (randomItemPool == null || randomItemPool.Count == 0)
            {
                Debug.LogWarning("[RoomItemPool] No random items defined!");
                return false;
            }
            
            // Check for null items
            foreach (var item in requiredItems)
            {
                if (item == null)
                {
                    Debug.LogError("[RoomItemPool] Required items list contains null entry!");
                    return false;
                }
            }
            
            foreach (var item in randomItemPool)
            {
                if (item == null)
                {
                    Debug.LogError("[RoomItemPool] Random item pool contains null entry!");
                    return false;
                }
            }
            
            return true;
        }
    }
}


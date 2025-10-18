using System.Collections.Generic;
using UnityEngine;
using TheButton.Items;

namespace TheButton.Game
{
    /// <summary>
    /// ScriptableObject that defines a multi-block event's properties
    /// Events can occupy multiple grid blocks and require specific items to interact
    /// </summary>
    [CreateAssetMenu(fileName = "New Event", menuName = "TheButton/Event Data")]
    public class EventData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Display name of the event")]
        public string eventName;
        
        [Tooltip("Description of the event")]
        [TextArea(2, 4)]
        public string description;
        
        [Header("Size & Placement")]
        [Tooltip("Size in grid blocks (X=width, Y=height, Z=depth)")]
        public Vector3Int size = Vector3Int.one;
        
        [Tooltip("Where this event can be placed")]
        public PlacementType placementType = PlacementType.Wall;
        
        [Header("Event Prefab")]
        [Tooltip("The prefab to spawn (should have InteractableEvent or similar component)")]
        public GameObject eventPrefab;
        
        [Header("Required Items")]
        [Tooltip("Items required to interact with this event (e.g., key for door, wrench for valve)")]
        public List<ItemData> requiredItems = new List<ItemData>();
        
        [Header("Spawn Settings")]
        [Tooltip("Should this event spawn in every room?")]
        public bool isRequired = false;
        
        [Tooltip("Spawn weight for random selection (higher = more likely)")]
        [Range(0f, 100f)]
        public float spawnWeight = 50f;
        
        /// <summary>
        /// Check if this event requires any items to interact
        /// </summary>
        public bool HasRequiredItems => requiredItems != null && requiredItems.Count > 0;
        
        /// <summary>
        /// Get total number of blocks this event occupies
        /// </summary>
        public int TotalBlocks => size.x * size.y * size.z;
        
        /// <summary>
        /// Validate the event data
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogError($"[EventData] Event has no name!");
                return false;
            }
            
            if (eventPrefab == null)
            {
                Debug.LogError($"[EventData] {eventName} has no event prefab assigned!");
                return false;
            }
            
            if (size.x <= 0 || size.y <= 0 || size.z <= 0)
            {
                Debug.LogError($"[EventData] {eventName} has invalid size: {size}");
                return false;
            }
            
            // Check for null items in required items list
            if (requiredItems != null)
            {
                foreach (var item in requiredItems)
                {
                    if (item == null)
                    {
                        Debug.LogWarning($"[EventData] {eventName} has null item in required items list!");
                    }
                }
            }
            
            return true;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure size is at least 1 in all dimensions
            size.x = Mathf.Max(1, size.x);
            size.y = Mathf.Max(1, size.y);
            size.z = Mathf.Max(1, size.z);
        }
#endif
    }
}


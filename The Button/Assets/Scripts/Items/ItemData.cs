using UnityEngine;

namespace TheButton.Items
{
    /// <summary>
    /// ScriptableObject that defines an item's properties
    /// Create instances via: Assets > Create > TheButton > Item Data
    /// Each ItemData contains a reference to its world prefab
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "TheButton/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Display name of the item")]
        public string itemName;
        
        [Tooltip("Description shown in UI")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("Icon displayed in inventory")]
        public Sprite icon;
        
        [Header("Item Category")]
        [Tooltip("Main category - determines behavior")]
        public ItemCategory category;
        
        [Tooltip("Specific type (optional) - for future features like crafting/quests")]
        public ItemType itemType = ItemType.Generic;
        
        [Header("Physical Properties")]
        [Tooltip("Weight of the item (affects physics)")]
        [Range(0.1f, 100f)]
        public float weight = 1f;
        
        [Tooltip("Can this item be stacked in inventory?")]
        public bool isStackable = false;
        
        [Tooltip("Maximum stack size (if stackable)")]
        public int maxStackSize = 1;
        
        [Header("Consumable Properties")]
        [Tooltip("Amount of health restored")]
        public float healthRestore = 0f;
        
        [Tooltip("Amount of hunger restored")]
        public float hungerRestore = 0f;
        
        [Tooltip("Amount of thirst restored")]
        public float thirstRestore = 0f;
        
        [Tooltip("Amount of stamina restored")]
        public float staminaRestore = 0f;
        
        [Tooltip("Damage dealt (negative = heal)")]
        public float damageAmount = 0f;
        
        [Header("Collectible Properties")]
        [Tooltip("Can this item be placed in the world?")]
        public bool canBePlaced = false;
        
        [Tooltip("Prefab to spawn when placed (for collectibles)")]
        public GameObject placedPrefab;
        
        [Header("Usable Properties")]
        [Tooltip("Can be held in hand?")]
        public bool canBeHeld = false;
        
        [Tooltip("Hand model when held")]
        public GameObject handModel;
        
        [Tooltip("Interaction range when held")]
        public float interactionRange = 2f;
        
        [Header("World Prefab")]
        [Tooltip("The world item prefab to spawn (must have WorldItem component)")]
        public GameObject itemPrefab;
        
        /// <summary>
        /// Check if this item is a consumable
        /// </summary>
        public bool IsConsumable => category == ItemCategory.Consumable;
        
        /// <summary>
        /// Check if this item is a collectible
        /// </summary>
        public bool IsCollectible => category == ItemCategory.Collectible;
        
        /// <summary>
        /// Check if this item is usable
        /// </summary>
        public bool IsUsable => category == ItemCategory.Usable || category == ItemCategory.Key;
        
        /// <summary>
        /// Check if this item is a key
        /// </summary>
        public bool IsKey => category == ItemCategory.Key;
    }
}


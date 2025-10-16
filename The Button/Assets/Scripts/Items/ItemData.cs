using UnityEngine;

namespace TheButton.Items
{
    /// <summary>
    /// ScriptableObject that defines an item's properties
    /// Create instances via: Assets > Create > TheButton > Item Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "TheButton/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique identifier for this item")]
        public int itemId;
        
        [Tooltip("Display name of the item")]
        public string itemName;
        
        [Tooltip("Description shown in UI")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("Icon displayed in inventory")]
        public Sprite icon;
        
        [Header("Item Properties")]
        [Tooltip("Type of item determines its usage behavior")]
        public ItemType itemType;
        
        [Header("Usage Values")]
        [Tooltip("Amount of health restored (for Medkit)")]
        public float healthRestore = 0f;
        
        [Tooltip("Amount of hunger restored (for Food)")]
        public float hungerRestore = 0f;
        
        [Tooltip("Amount of thirst restored (for Water)")]
        public float thirstRestore = 0f;
        
        [Tooltip("Amount of damage dealt (for Hazard items, negative values heal)")]
        public float damageAmount = 0f;
        
        [Header("Visual")]
        [Tooltip("Material to use for the 3D world item")]
        public Material worldMaterial;
        
        [Tooltip("Color tint for the world item")]
        public Color itemColor = Color.white;
    }
}


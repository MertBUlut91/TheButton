namespace TheButton.Items
{
    /// <summary>
    /// Defines the main category of an item
    /// Determines how the item behaves and can be used
    /// </summary>
    public enum ItemCategory
    {
        Consumable,   // Can be consumed (food, water, medkit) - disappears after use
        Collectible,  // Can be collected and placed (furniture, decoration) - stays in world when placed
        Usable,       // Can be held and used (key, screwdriver, pen) - stays in inventory
        Key           // Special usable item for doors
    }
}


namespace TheButton.Items
{
    /// <summary>
    /// Defines specific types of items
    /// Used for specific gameplay effects
    /// </summary>
    public enum ItemType
    {
        // Consumables
        Food,           // Restores hunger
        Water,          // Restores thirst
        Medkit,         // Restores health
        Bandage,        // Small health restore
        EnergyDrink,    // Restores stamina
        Poison,         // Damages player
        
        // Collectibles (Furniture & Decoration)
        Chair,
        Table,
        Lamp,
        Picture,
        Plant,
        Box,
        Barrel,
        
        // Usable Tools
        Key,            // Opens doors
        Screwdriver,    // Repair/interact
        Pen,            // Write/interact
        Flashlight,     // Light source
        Wrench,         // Repair
        Hammer,         // Break/build
        
        // Generic
        Generic         // Default item type
    }
}


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
        
        // Weapons
        Pistol,         // Ranged weapon - low damage, fast fire rate
        Rifle,          // Ranged weapon - medium damage, medium fire rate
        Shotgun,        // Ranged weapon - high damage, slow fire rate
        Knife,          // Melee weapon - low damage, very fast
        Bat,            // Melee weapon - medium damage, medium speed
        Axe,            // Melee weapon - high damage, slow speed
        
        // Generic
        Generic         // Default item type
    }
}


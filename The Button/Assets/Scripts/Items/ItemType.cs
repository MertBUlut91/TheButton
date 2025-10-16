namespace TheButton.Items
{
    /// <summary>
    /// Defines the types of items that can exist in the game
    /// </summary>
    public enum ItemType
    {
        Key,      // Opens the exit door
        Medkit,   // Restores health
        Food,     // Restores hunger
        Water,    // Restores thirst
        Hazard    // Damages player or has negative effect
    }
}


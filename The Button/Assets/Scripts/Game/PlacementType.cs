namespace TheButton.Game
{
    /// <summary>
    /// Defines where an event can be placed in the room
    /// </summary>
    public enum PlacementType
    {
        /// <summary>
        /// Can be placed on any wall (North, South, East, West)
        /// </summary>
        Wall,
        
        /// <summary>
        /// Can be placed on the floor
        /// </summary>
        Floor,
        
        /// <summary>
        /// Can be placed on the ceiling
        /// </summary>
        Ceiling,
        
        /// <summary>
        /// Can be placed anywhere (wall, floor, or ceiling)
        /// </summary>
        Any
    }
}


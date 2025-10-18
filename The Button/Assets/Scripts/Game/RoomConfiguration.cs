using UnityEngine;

namespace TheButton.Game
{
    /// <summary>
    /// Configuration for procedural room generation
    /// Defines room dimensions, materials, and generation parameters
    /// </summary>
    [CreateAssetMenu(fileName = "RoomConfiguration", menuName = "The Button/Room Configuration")]
    public class RoomConfiguration : ScriptableObject
    {
        [Header("Room Dimensions")]
        [Tooltip("Width of the room in cubes")]
        [Range(5, 30)]
        public int roomWidth = 15;
        
        [Tooltip("Height of the room in cubes")]
        [Range(3, 15)]
        public int roomHeight = 10;
        
        [Tooltip("Depth of the room in cubes")]
        [Range(5, 30)]
        public int roomDepth = 15;
        
        [Tooltip("Size of each cube unit")]
        public float cubeSize = 1f;
        
        [Header("Structure Prefabs")]
        [Tooltip("Prefab for floor tiles")]
        public GameObject floorPrefab;
        
        [Tooltip("Prefab for ceiling tiles")]
        public GameObject ceilingPrefab;
        
        [Tooltip("Wall cube with button prefab (already includes button on top)")]
        public GameObject wallCubeWithButtonPrefab;
        
        [Tooltip("Plain wall cube prefab (no button, just a wall)")]
        public GameObject plainWallCubePrefab;
        
        [Tooltip("Plain corner cube prefab (no button, for corners)")]
        public GameObject cornerCubePrefab;
        
        [Header("Button Density")]
        [Tooltip("Minimum percentage of wall positions that will have buttons (0-100)")]
        [Range(0f, 100f)]
        public float minButtonDensityPercent = 20f;
        
        [Tooltip("Maximum percentage of wall positions that will have buttons (0-100)")]
        [Range(0f, 100f)]
        public float maxButtonDensityPercent = 50f;
        
        [Header("Materials")]
        [Tooltip("Material for floor")]
        public Material floorMaterial;
        
        [Tooltip("Material for ceiling")]
        public Material ceilingMaterial;
        
        [Tooltip("Material for wall cubes")]
        public Material wallMaterial;
        
        [Header("Special Positions")]
        [Tooltip("Create a spawn point cube at ceiling center")]
        public bool createCeilingSpawnPoint = true;
        
        [Tooltip("Prefab for ceiling spawn point cube")]
        public GameObject spawnPointCubePrefab;
        
        [Header("Spawn Settings")]
        [Tooltip("Offset from center for player spawn")]
        public Vector3 playerSpawnOffset = Vector3.zero;
        
        [Tooltip("Height offset for item spawn points")]
        public float itemSpawnOffset = 0.5f;
        
        [Header("Events")]
        [Tooltip("Pool of events that can spawn in the room (doors, puzzles, etc.)")]
        public RoomEventPool eventPool;
    }
}


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
        [Header("Room Prefab System")]
        [Tooltip("Hazır oda prefab'ı (RoomPrefabManager component'i ile)")]
        public GameObject roomPrefab;
        
        [Header("Room Dimensions (DEPRECATED - Use Room Prefab)")]
        [Tooltip("DEPRECATED: Width of the room in cubes - artık roomPrefab kullanılıyor")]
        [Range(5, 30)]
        public int roomWidth = 15;
        
        [Tooltip("DEPRECATED: Height of the room in cubes - artık roomPrefab kullanılıyor")]
        [Range(3, 15)]
        public int roomHeight = 10;
        
        [Tooltip("DEPRECATED: Depth of the room in cubes - artık roomPrefab kullanılıyor")]
        [Range(5, 30)]
        public int roomDepth = 15;
        
        [Tooltip("Size of each cube unit")]
        public float cubeSize = 1f;
        
        [Header("Structure Prefabs (DEPRECATED - Use Room Prefab)")]
        [Tooltip("DEPRECATED: Prefab for floor tiles - artık roomPrefab'da hazır")]
        public GameObject floorPrefab;
        
        [Tooltip("DEPRECATED: Prefab for ceiling tiles - artık roomPrefab'da hazır")]
        public GameObject ceilingPrefab;
        
        [Tooltip("Wall cube with button prefab (marker yerine gelecek)")]
        public GameObject wallCubeWithButtonPrefab;
        
        [Tooltip("DEPRECATED: Plain wall cube prefab - marker'lar zaten duvar küpü")]
        public GameObject plainWallCubePrefab;
        
        [Tooltip("DEPRECATED: Plain corner cube prefab - artık roomPrefab'da hazır")]
        public GameObject cornerCubePrefab;
        
        [Header("Button Density")]
        [Tooltip("Minimum percentage of wall positions that will have item spawn buttons (0-100)")]
        [Range(0f, 100f)]
        public float minButtonDensityPercent = 20f;
        
        [Tooltip("Maximum percentage of wall positions that will have item spawn buttons (0-100)")]
        [Range(0f, 100f)]
        public float maxButtonDensityPercent = 50f;
        
        [Header("Enemy Spawn Button")]
        [Tooltip("Wall cube with enemy spawn button prefab (same visual as item button)")]
        public GameObject wallCubeWithEnemyButtonPrefab;
        
        [Tooltip("Minimum percentage of wall positions that will have enemy spawn buttons (0-100)")]
        [Range(0f, 100f)]
        public float minEnemyButtonDensityPercent = 5f;
        
        [Tooltip("Maximum percentage of wall positions that will have enemy spawn buttons (0-100)")]
        [Range(0f, 100f)]
        public float maxEnemyButtonDensityPercent = 15f;
        
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
        
        [Header("Enemies")]
        [Tooltip("Pool of enemies that can spawn in the room")]
        public TheButton.Enemy.EnemyPool enemyPool;
    }
}


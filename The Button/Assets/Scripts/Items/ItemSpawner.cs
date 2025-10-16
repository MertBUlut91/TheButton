using Unity.Netcode;
using UnityEngine;

namespace TheButton.Items
{
    /// <summary>
    /// Server-authoritative item spawner
    /// Handles spawning of networked items at designated spawn points
    /// </summary>
    public class ItemSpawner : NetworkBehaviour
    {
        public static ItemSpawner Instance { get; private set; }
        
        [Header("Item Prefab")]
        [Tooltip("The networked item prefab to spawn")]
        [SerializeField] private GameObject itemPrefab;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// Spawn an item at a specific position (Server only)
        /// </summary>
        public void SpawnItem(int itemId, Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[ItemSpawner] SpawnItem can only be called on server!");
                return;
            }
            
            if (itemPrefab == null)
            {
                Debug.LogError("[ItemSpawner] Item prefab is not assigned!");
                return;
            }
            
            // Instantiate the item
            GameObject itemObject = Instantiate(itemPrefab, position, rotation);
            
            // Get the WorldItem component and set its ID
            WorldItem worldItem = itemObject.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                worldItem.SetItemId(itemId);
            }
            
            // Get NetworkObject and spawn it
            NetworkObject networkObject = itemObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(true);
                Debug.Log($"[ItemSpawner] Spawned item {itemId} at {position}");
            }
            else
            {
                Debug.LogError("[ItemSpawner] Item prefab does not have NetworkObject component!");
                Destroy(itemObject);
            }
        }
        
        /// <summary>
        /// Spawn an item at a specific transform (Server only)
        /// </summary>
        public void SpawnItemAtTransform(int itemId, Transform spawnPoint)
        {
            SpawnItem(itemId, spawnPoint.position, spawnPoint.rotation);
        }
    }
}


using Unity.Netcode;
using UnityEngine;

namespace TheButton.Items
{
    /// <summary>
    /// Server-authoritative item spawner
    /// Handles spawning of networked items at designated spawn points
    /// Uses ItemData ScriptableObject to spawn the correct prefab
    /// </summary>
    public class ItemSpawner : NetworkBehaviour
    {
        public static ItemSpawner Instance { get; private set; }
        
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
        public void SpawnItem(ItemData itemData, Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[ItemSpawner] SpawnItem can only be called on server!");
                return;
            }
            
            if (itemData == null)
            {
                Debug.LogError("[ItemSpawner] ItemData is null!");
                return;
            }
            
            if (itemData.itemPrefab == null)
            {
                Debug.LogError($"[ItemSpawner] ItemData '{itemData.itemName}' does not have an itemPrefab assigned!");
                return;
            }
            
            // Instantiate the item prefab from ItemData
            GameObject itemObject = Instantiate(itemData.itemPrefab, position, rotation);
            
            // Get the WorldItem component and set its ItemData
            WorldItem worldItem = itemObject.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                worldItem.SetItemData(itemData);
            }
            else
            {
                Debug.LogWarning($"[ItemSpawner] Spawned prefab '{itemData.itemPrefab.name}' does not have WorldItem component!");
            }
            
            // Get NetworkObject and spawn it
            NetworkObject networkObject = itemObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(true);
                Debug.Log($"[ItemSpawner] Spawned item '{itemData.itemName}' at {position}");
            }
            else
            {
                Debug.LogError($"[ItemSpawner] Item prefab '{itemData.itemPrefab.name}' does not have NetworkObject component!");
                Destroy(itemObject);
            }
        }
        
        /// <summary>
        /// Spawn an item at a specific transform (Server only)
        /// </summary>
        public void SpawnItemAtTransform(ItemData itemData, Transform spawnPoint)
        {
            if (spawnPoint == null)
            {
                Debug.LogError("[ItemSpawner] Spawn point is null!");
                return;
            }
            
            SpawnItem(itemData, spawnPoint.position, spawnPoint.rotation);
        }
    }
}

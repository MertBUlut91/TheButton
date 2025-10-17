using Unity.Netcode;
using UnityEngine;
using TheButton.Network;
using TheButton.Interactables;

namespace TheButton.Items
{
    /// <summary>
    /// Represents an item in the 3D world that can be picked up by players
    /// Must have NetworkObject, Collider, and Rigidbody
    /// Uses ItemData ScriptableObject for all properties
    /// Pickup with "E" key interaction
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class WorldItem : NetworkBehaviour, IInteractable
    {
        [Header("Item Configuration")]
        [Tooltip("The ItemData ScriptableObject this item represents")]
        [SerializeField] private ItemData itemData;
        
        [Header("Physics Settings")]
        [Tooltip("Rigidbody component (auto-assigned)")]
        [SerializeField] private Rigidbody rb;
        
        // Network sync: Store asset name for clients to load
        private NetworkVariable<NetworkString> itemDataAssetName = new NetworkVariable<NetworkString>(
            new NetworkString(""), 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server
        );
        
        private bool isBeingPickedUp = false;
        
        public ItemData GetItemData() => itemData;
        
        private void Awake()
        {
            // Auto-find components
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
            
            // Ensure collider is NOT trigger (for physics)
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
            }
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Subscribe to network variable changes
            itemDataAssetName.OnValueChanged += OnItemDataAssetNameChanged;
            
            // Apply visual properties
            if (itemData != null)
            {
                ApplyItemData();
            }
            else if (!IsServer && !string.IsNullOrEmpty(itemDataAssetName.Value))
            {
                // Client: Load ItemData from Resources
                LoadItemDataFromAssetName(itemDataAssetName.Value);
            }
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            itemDataAssetName.OnValueChanged -= OnItemDataAssetNameChanged;
        }
        
        /// <summary>
        /// Set the item data (Server only)
        /// </summary>
        public void SetItemData(ItemData data)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[WorldItem] SetItemData can only be called on server!");
                return;
            }
            
            if (data == null)
            {
                Debug.LogError("[WorldItem] Attempted to set null ItemData!");
                return;
            }
            
            itemData = data;
            itemDataAssetName.Value = new NetworkString(data.name);
            ApplyItemData();
            
            Debug.Log($"[WorldItem] Set ItemData to {data.itemName} (asset: {data.name})");
        }
        
        private void OnItemDataAssetNameChanged(NetworkString oldValue, NetworkString newValue)
        {
            // Client: Load ItemData when network variable changes
            if (!IsServer && !string.IsNullOrEmpty(newValue))
            {
                LoadItemDataFromAssetName(newValue);
            }
        }
        
        private void LoadItemDataFromAssetName(string assetName)
        {
            // Load from Resources/Items/ folder
            itemData = Resources.Load<ItemData>($"Items/{assetName}");
            
            if (itemData != null)
            {
                ApplyItemData();
                Debug.Log($"[WorldItem] Loaded ItemData: {itemData.itemName} from Resources");
            }
            else
            {
                Debug.LogError($"[WorldItem] Failed to load ItemData from Resources/Items/{assetName}");
            }
        }
        
        private void ApplyItemData()
        {
            if (itemData == null) return;
            
            // Apply physics properties
            if (rb != null)
            {
                rb.mass = itemData.weight;
                rb.useGravity = true;
                rb.isKinematic = false;
            }
        }
        
        #region IInteractable Implementation
        
        public void Interact(GameObject playerGameObject)
        {
            Debug.Log($"[WorldItem] Interact called by player: {playerGameObject.name}, IsServer: {IsServer}");
            
            // Client needs to request interaction from server
            if (!IsServer)
            {
                // Request pickup on server
                RequestPickupServerRpc(playerGameObject.GetComponent<NetworkObject>().OwnerClientId);
                return;
            }
            
            if (isBeingPickedUp)
            {
                Debug.Log("[WorldItem] Item is already being picked up");
                return;
            }
            
            if (itemData == null)
            {
                Debug.LogError("[WorldItem] ItemData is null!");
                return;
            }
            
            // Get player inventory
            var playerInventory = playerGameObject.GetComponent<Player.PlayerInventory>();
            if (playerInventory == null)
            {
                Debug.LogWarning($"[WorldItem] Player {playerGameObject.name} has no inventory component!");
                return;
            }
            
            // Try to add item to player's inventory
            if (!playerInventory.IsFull())
            {
                Debug.Log($"[WorldItem] Player {playerInventory.OwnerClientId} picked up {itemData.itemName}");
                
                // Add item to inventory (pass ItemData asset name)
                playerInventory.AddItemServerRpc(itemData.name);
                
                // Mark as being picked up to prevent double pickup
                isBeingPickedUp = true;
                
                // Despawn the item
                GetComponent<NetworkObject>().Despawn(true);
            }
            else
            {
                Debug.Log($"[WorldItem] Player's inventory is full!");
                // Could show UI message here
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RequestPickupServerRpc(ulong clientId)
        {
            Debug.Log($"[WorldItem] RequestPickupServerRpc called by client {clientId}");
            
            // Find the player object for this client
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    Interact(playerObject.gameObject);
                }
                else
                {
                    Debug.LogError($"[WorldItem] Player object not found for client {clientId}");
                }
            }
            else
            {
                Debug.LogError($"[WorldItem] Client {clientId} not found in connected clients");
            }
        }
        
        public string GetInteractionPrompt()
        {
            if (itemData == null)
                return "Press E to pick up";
            
            return $"Press E to pick up {itemData.itemName}";
        }
        
        public bool CanInteract()
        {
            return itemData != null && !isBeingPickedUp;
        }
        
        #endregion
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-find components if not set
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }
#endif
    }
}

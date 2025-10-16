using Unity.Netcode;
using UnityEngine;

namespace TheButton.Items
{
    /// <summary>
    /// Represents an item in the 3D world that can be picked up by players
    /// Must have NetworkObject component and a Collider (isTrigger = true)
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    public class WorldItem : NetworkBehaviour
    {
        [Header("Item Configuration")]
        [Tooltip("The ID of the item this represents")]
        [SerializeField] private int itemId;
        
        [Header("Visual Settings")]
        [Tooltip("The visual mesh renderer")]
        [SerializeField] private MeshRenderer meshRenderer;
        
        [Tooltip("Rotation speed for visual effect")]
        [SerializeField] private float rotationSpeed = 50f;
        
        [Tooltip("Bob animation speed")]
        [SerializeField] private float bobSpeed = 2f;
        
        [Tooltip("Bob animation height")]
        [SerializeField] private float bobHeight = 0.2f;
        
        private NetworkVariable<int> networkItemId = new NetworkVariable<int>(
            0, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server
        );
        
        private Vector3 startPosition;
        private bool isBeingPickedUp = false;
        
        public int ItemId => networkItemId.Value;
        
        private void Awake()
        {
            // Ensure collider is trigger
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                networkItemId.Value = itemId;
            }
            
            startPosition = transform.position;
            
            // Apply visual properties from item database
            ApplyItemVisuals();
            
            networkItemId.OnValueChanged += OnItemIdChanged;
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            networkItemId.OnValueChanged -= OnItemIdChanged;
        }
        
        private void Update()
        {
            if (!isBeingPickedUp)
            {
                // Rotate the item
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                
                // Bob up and down
                float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }
        
        /// <summary>
        /// Set the item ID (Server only)
        /// </summary>
        public void SetItemId(int id)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[WorldItem] SetItemId can only be called on server!");
                return;
            }
            
            itemId = id;
            networkItemId.Value = id;
            ApplyItemVisuals();
        }
        
        private void OnItemIdChanged(int oldValue, int newValue)
        {
            ApplyItemVisuals();
        }
        
        private void ApplyItemVisuals()
        {
            ItemData itemData = ItemDatabase.Instance?.GetItem(networkItemId.Value);
            if (itemData != null && meshRenderer != null)
            {
                // Apply material if specified
                if (itemData.worldMaterial != null)
                {
                    meshRenderer.material = itemData.worldMaterial;
                }
                
                // Apply color tint
                meshRenderer.material.color = itemData.itemColor;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Only server handles pickup logic
            if (!IsServer) return;
            if (isBeingPickedUp) return;
            
            // Check if the collider is a player
            var playerInventory = other.GetComponent<Player.PlayerInventory>();
            if (playerInventory != null && playerInventory.IsOwner)
            {
                // Try to add item to player's inventory
                if (!playerInventory.IsFull())
                {
                    Debug.Log($"[WorldItem] Player picked up item {networkItemId.Value}");
                    
                    // Add item to inventory
                    playerInventory.AddItemServerRpc(networkItemId.Value);
                    
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
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-find mesh renderer if not set
            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            }
        }
#endif
    }
}


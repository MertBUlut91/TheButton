using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using TheButton.Items;

namespace TheButton.Player
{
    /// <summary>
    /// Handles item usage, slot selection, dropping, and placement
    /// Works alongside PlayerInteraction (E for world items) and PlayerInventory
    /// </summary>
    public class PlayerItemUsage : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private PlayerInteraction playerInteraction;
        
        [Header("Placement Settings")]
        [Tooltip("Distance in front of player to place items")]
        [SerializeField] private float placementDistance = 3f;
        
        [Tooltip("Layer mask for placement ground check")]
        [SerializeField] private LayerMask placementLayerMask = ~0;
        
        [Tooltip("Layer mask for collision detection (objects to avoid overlapping with)")]
        [SerializeField] private LayerMask collisionCheckLayerMask = ~0;
        
        [Tooltip("Use mesh-based collision detection (more accurate but slightly more expensive)")]
        [SerializeField] private bool useMeshCollisionCheck = true;
        
        [Tooltip("Minimum distance from other objects (in meters)")]
        [SerializeField] private float minimumClearance = 0.1f;
        
        [Tooltip("Color of placement preview when valid")]
        [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f);
        
        [Tooltip("Color of placement preview when invalid")]
        [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f);
        
        // Placement mode state
        private bool isInPlacementMode = false;
        private GameObject placementPreview;
        private Vector3 placementPosition;
        private Quaternion placementRotation = Quaternion.identity;
        private float rotationAngle = 0f;
        private bool canPlaceAtCurrentPosition = false;
        private Collider[] previewColliders; // Store colliders for overlap check
        
        private void Awake()
        {
            // Auto-find components
            if (inventory == null)
                inventory = GetComponent<PlayerInventory>();
            
            if (playerInteraction == null)
                playerInteraction = GetComponent<PlayerInteraction>();
        }
        
        private void Start()
        {
            // Find camera
            if (cameraTransform == null)
            {
                cameraTransform = transform.Find("PlayerCamera");
                if (cameraTransform == null)
                {
                    var cameras = GetComponentsInChildren<Camera>(true);
                    if (cameras.Length > 0)
                        cameraTransform = cameras[0].transform;
                }
            }
        }
        
        private void Update()
        {
            // Only local player can use items
            if (!IsOwner) return;
            
            // Handle placement mode
            if (isInPlacementMode)
            {
                HandlePlacementMode();
                return; // Don't process other inputs while in placement mode
            }
            
            // Handle slot selection (1-4 keys)
            HandleSlotSelection();
            
            // Handle item usage/drop
            HandleItemActions();
        }
        
        private void HandleSlotSelection()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.SetSelectedSlot(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.SetSelectedSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.SetSelectedSlot(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.SetSelectedSlot(3);
        }
        
        private void HandleItemActions()
        {
            // Q to drop item
            if (Input.GetKeyDown(KeyCode.Q))
            {
                DropSelectedItem();
            }
            
            // E to use item (only if not looking at interactable)
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Check if player is looking at an interactable world object
                if (playerInteraction != null && playerInteraction.IsLookingAtInteractable())
                {
                    // Let PlayerInteraction handle it
                    return;
                }
                
                // No interactable in sight, try to use inventory item
                UseSelectedItem();
            }
        }
        
        private void DropSelectedItem()
        {
            ItemData item = inventory.GetSelectedItem();
            if (item == null)
            {
                Debug.Log("[PlayerItemUsage] No item in selected slot to drop");
                return;
            }
            
            Debug.Log($"[PlayerItemUsage] Dropping item: {item.itemName}");
            inventory.DropSelectedItem();
        }
        
        private void UseSelectedItem()
        {
            ItemData item = inventory.GetSelectedItem();
            if (item == null)
            {
                Debug.Log("[PlayerItemUsage] No item in selected slot");
                return;
            }
            
            Debug.Log($"[PlayerItemUsage] Using item: {item.itemName} (Category: {item.category})");
            
            // Check if it's a collectible that can be placed
            if (item.category == ItemCategory.Collectible && item.canBePlaced)
            {
                // Enter placement mode
                EnterPlacementMode(item);
            }
            else
            {
                // Use the item normally (consumable, usable, key)
                inventory.UseItemServerRpc(inventory.GetSelectedSlot());
            }
        }
        
        #region Placement Mode
        
        private void EnterPlacementMode(ItemData item)
        {
            if (item.placedPrefab == null)
            {
                Debug.LogError($"[PlayerItemUsage] Item {item.itemName} has no placed prefab!");
                return;
            }
            
            Debug.Log($"[PlayerItemUsage] Entering placement mode for {item.itemName}");
            isInPlacementMode = true;
            rotationAngle = 0f;
            
            // Create preview object
            placementPreview = Instantiate(item.placedPrefab);
            
            // Check if prefab has PlaceableItem component
            var placeableItem = placementPreview.GetComponent<PlaceableItem>();
            if (placeableItem != null)
            {
                // Use the PlaceableItem component for proper setup
                placeableItem.SetVisualMode(VisualMode.Placement);
                
                if (!placeableItem.ValidateSetup())
                {
                    Debug.LogWarning($"[PlayerItemUsage] PlaceableItem setup is incomplete on {item.itemName}");
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerItemUsage] No PlaceableItem component found on {item.itemName}. Add PlaceableItem component to prefab for proper placement preview.");
            }
            
            // Disable any physics/network components on preview
            var rb = placementPreview.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            
            // Store colliders for collision checking but configure them as triggers
            previewColliders = placementPreview.GetComponentsInChildren<Collider>(true);
            foreach (var col in previewColliders)
            {
                if (useMeshCollisionCheck)
                {
                    // Enable colliders as triggers for overlap detection
                    col.enabled = true;
                    col.isTrigger = true;
                }
                else
                {
                    // Disable colliders completely for simpler raycast-only mode
                    col.enabled = false;
                }
            }
            
            // Disable any scripts that shouldn't run in preview mode
            // NOTE: We disable these instead of destroying to avoid dependency issues
            
            var worldItem = placementPreview.GetComponent<WorldItem>();
            if (worldItem != null) worldItem.enabled = false;
            
            var networkTransform = placementPreview.GetComponent<NetworkTransform>();
            if (networkTransform != null) networkTransform.enabled = false;
            
            // Disable NetworkObject - don't destroy it because other components depend on it
            var netObj = placementPreview.GetComponent<NetworkObject>();
            if (netObj != null) netObj.enabled = false;
            
            // Initialize placement position immediately
            UpdatePlacementPosition();
            if (placementPreview != null)
            {
                placementPreview.transform.position = placementPosition;
                placementPreview.transform.rotation = placementRotation;
            }
        }
        
        private void HandlePlacementMode()
        {
            if (placementPreview == null)
            {
                ExitPlacementMode();
                return;
            }
            
            // Calculate placement position (this also updates preview position)
            UpdatePlacementPosition();
            
            // Update preview color based on validity
            UpdatePreviewColor();
            
            // Handle rotation (R key)
            if (Input.GetKeyDown(KeyCode.R))
            {
                rotationAngle += 45f;
                if (rotationAngle >= 360f) rotationAngle = 0f;
                placementRotation = Quaternion.Euler(0, rotationAngle, 0);
                Debug.Log($"[PlayerItemUsage] Rotated to {rotationAngle}°");
            }
            
            // Handle placement (E key)
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (canPlaceAtCurrentPosition)
                {
                    PlaceItem();
                }
                else
                {
                    Debug.Log("[PlayerItemUsage] Cannot place item at current position");
                }
            }
            
            // Cancel placement (Q key or ESC)
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[PlayerItemUsage] Cancelled placement");
                ExitPlacementMode();
            }
        }
        
        private void UpdatePlacementPosition()
        {
            if (cameraTransform == null) return;
            
            // Raycast from camera to find placement position
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;
            
            // Ignore triggers in raycast (our preview is a trigger)
            if (Physics.Raycast(ray, out hit, placementDistance, placementLayerMask, QueryTriggerInteraction.Ignore))
            {
                // Set position based on raycast hit
                placementPosition = hit.point;
                
                // Update preview position first (needed for collision check)
                if (placementPreview != null)
                {
                    placementPreview.transform.position = placementPosition;
                    placementPreview.transform.rotation = placementRotation;
                }
                
                // Now check collision at this position
                if (useMeshCollisionCheck)
                {
                    // Wait one physics frame for colliders to update
                    canPlaceAtCurrentPosition = CheckMeshCollision();
                }
                else
                {
                    // Simple mode: if we hit something, we can place
                    canPlaceAtCurrentPosition = true;
                }
            }
            else
            {
                // Place at max distance if no hit
                placementPosition = cameraTransform.position + cameraTransform.forward * placementDistance;
                canPlaceAtCurrentPosition = false;
            }
        }
        
        /// <summary>
        /// Check if the preview object collides with any existing objects using mesh collision
        /// Returns true if placement is valid (no collision), false otherwise
        /// </summary>
        private bool CheckMeshCollision()
        {
            if (placementPreview == null || previewColliders == null || previewColliders.Length == 0)
                return true;
            
            // Check each collider in the preview object
            foreach (var col in previewColliders)
            {
                if (col == null || !col.enabled) continue;
                
                // Use Physics.OverlapBox, OverlapSphere, or OverlapCapsule based on collider type
                Collider[] overlaps = null;
                
                if (col is BoxCollider boxCol)
                {
                    // Calculate world space bounds
                    Vector3 center = col.bounds.center;
                    Vector3 halfExtents = boxCol.size * 0.5f;
                    halfExtents.x *= col.transform.lossyScale.x;
                    halfExtents.y *= col.transform.lossyScale.y;
                    halfExtents.z *= col.transform.lossyScale.z;
                    
                    // Shrink slightly by clearance amount
                    halfExtents -= Vector3.one * minimumClearance;
                    if (halfExtents.x < 0.01f) halfExtents.x = 0.01f;
                    if (halfExtents.y < 0.01f) halfExtents.y = 0.01f;
                    if (halfExtents.z < 0.01f) halfExtents.z = 0.01f;
                    
                    // Ignore triggers (our preview colliders are triggers)
                    overlaps = Physics.OverlapBox(center, halfExtents, col.transform.rotation, collisionCheckLayerMask, QueryTriggerInteraction.Ignore);
                }
                else if (col is SphereCollider sphereCol)
                {
                    Vector3 center = col.bounds.center;
                    float radius = sphereCol.radius * Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.y, col.transform.lossyScale.z);
                    radius -= minimumClearance;
                    if (radius < 0.01f) radius = 0.01f;
                    
                    overlaps = Physics.OverlapSphere(center, radius, collisionCheckLayerMask, QueryTriggerInteraction.Ignore);
                }
                else if (col is CapsuleCollider capsuleCol)
                {
                    Vector3 center = col.bounds.center;
                    float radius = capsuleCol.radius * Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.z);
                    float height = capsuleCol.height * col.transform.lossyScale.y;
                    
                    radius -= minimumClearance;
                    if (radius < 0.01f) radius = 0.01f;
                    
                    // Calculate capsule points
                    float halfHeight = (height * 0.5f) - radius;
                    Vector3 point1 = center + Vector3.up * halfHeight;
                    Vector3 point2 = center - Vector3.up * halfHeight;
                    
                    overlaps = Physics.OverlapCapsule(point1, point2, radius, collisionCheckLayerMask, QueryTriggerInteraction.Ignore);
                }
                else if (col is MeshCollider)
                {
                    // For mesh colliders, use bounds-based box check as approximation
                    Vector3 center = col.bounds.center;
                    Vector3 halfExtents = col.bounds.extents - Vector3.one * minimumClearance;
                    if (halfExtents.x < 0.01f) halfExtents.x = 0.01f;
                    if (halfExtents.y < 0.01f) halfExtents.y = 0.01f;
                    if (halfExtents.z < 0.01f) halfExtents.z = 0.01f;
                    
                    overlaps = Physics.OverlapBox(center, halfExtents, col.transform.rotation, collisionCheckLayerMask, QueryTriggerInteraction.Ignore);
                }
                
                // Check if we found any overlapping colliders
                if (overlaps != null && overlaps.Length > 0)
                {
                    // Filter out our own preview colliders
                    foreach (var overlap in overlaps)
                    {
                        // Check if this overlapping collider is part of our preview
                        bool isOwnCollider = false;
                        foreach (var previewCol in previewColliders)
                        {
                            if (overlap == previewCol)
                            {
                                isOwnCollider = true;
                                break;
                            }
                        }
                        
                        // If it's not our own collider, we have a collision
                        if (!isOwnCollider)
                        {
                            Debug.Log($"[PlayerItemUsage] Collision detected with {overlap.gameObject.name}");
                            return false;
                        }
                    }
                }
            }
            
            // No collisions found, placement is valid
            return true;
        }
        
        private void UpdatePreviewColor()
        {
            if (placementPreview == null) return;
            
            // Use PlaceableItem component if available
            var placeableItem = placementPreview.GetComponent<PlaceableItem>();
            if (placeableItem != null)
            {
                placeableItem.SetPlacementValid(canPlaceAtCurrentPosition);
            }
            else
            {
                // Fallback: Manually update colors (deprecated, but kept for backwards compatibility)
                Color targetColor = canPlaceAtCurrentPosition ? validPlacementColor : invalidPlacementColor;
                
                var renderers = placementPreview.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    foreach (var mat in renderer.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            mat.color = targetColor;
                        }
                    }
                }
            }
        }
        
        private void PlaceItem()
        {
            ItemData item = inventory.GetSelectedItem();
            if (item == null) return;
            
            Debug.Log($"[PlayerItemUsage] Placing item {item.itemName} at {placementPosition}");
            
            // Request placement on server
            PlaceItemServerRpc(inventory.GetSelectedSlot(), placementPosition, placementRotation);
            
            // Exit placement mode
            ExitPlacementMode();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void PlaceItemServerRpc(int slotIndex, Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
        {
            // Get player inventory
            ulong clientId = rpcParams.Receive.SenderClientId;
            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                Debug.LogError($"[PlayerItemUsage] Client {clientId} not found");
                return;
            }
            
            var playerObject = client.PlayerObject;
            if (playerObject == null)
            {
                Debug.LogError($"[PlayerItemUsage] Player object not found");
                return;
            }
            
            var playerInventory = playerObject.GetComponent<PlayerInventory>();
            if (playerInventory == null)
            {
                Debug.LogError("[PlayerItemUsage] PlayerInventory not found");
                return;
            }
            
            // Get item data
            ItemData item = playerInventory.GetItemAtSlot(slotIndex);
            if (item == null || !item.canBePlaced || item.placedPrefab == null)
            {
                Debug.LogError($"[PlayerItemUsage] Item cannot be placed");
                return;
            }
            
            // Spawn the placed item
            if (ItemSpawner.Instance != null)
            {
                ItemSpawner.Instance.SpawnItem(item, position, rotation);
                Debug.Log($"[PlayerItemUsage] Server placed item {item.itemName}");
            }
            
            // Remove from inventory
            playerInventory.RemoveItemAtSlotServerRpc(slotIndex);
        }
        
        private void ExitPlacementMode()
        {
            isInPlacementMode = false;
            rotationAngle = 0f;
            
            if (placementPreview != null)
            {
                Destroy(placementPreview);
                placementPreview = null;
            }
            
            previewColliders = null;
            
            Debug.Log("[PlayerItemUsage] Exited placement mode");
        }
        
        #endregion
        
        /// <summary>
        /// Check if currently in placement mode (public for PlayerInteraction)
        /// </summary>
        public bool IsInPlacementMode()
        {
            return isInPlacementMode;
        }
        
        private void OnDestroy()
        {
            // Clean up preview if exists
            if (placementPreview != null)
            {
                Destroy(placementPreview);
            }
        }
    }
}


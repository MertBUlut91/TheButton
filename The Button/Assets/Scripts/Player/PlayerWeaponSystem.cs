using Unity.Netcode;
using UnityEngine;
using TheButton.Items;
using TheButton.Network;
using TheButton.Enemy;

namespace TheButton.Player
{
    /// <summary>
    /// Handles weapon equipping, attacking, and damage dealing
    /// Works with PlayerInventory to manage weapon items
    /// Left-click to attack with equipped weapon
    /// </summary>
    public class PlayerWeaponSystem : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private PlayerNetwork playerNetwork;
        
        [Header("Weapon Display")]
        [Tooltip("Parent transform for weapon models (in hand)")]
        [SerializeField] private Transform weaponHolder;
        
        [Tooltip("Hand bone to follow (drag your character's hand bone here)")]
        [SerializeField] private Transform handBone;
        
        [Tooltip("Use constraint system instead of parenting (fixes scale issues)")]
        [SerializeField] private bool useConstraintSystem = true;
        
        [Header("Position Offset (Optional)")]
        [Tooltip("Position offset from hand bone")]
        [SerializeField] private Vector3 positionOffset = Vector3.zero;
        
        [Tooltip("Rotation offset from hand bone (in degrees)")]
        [SerializeField] private Vector3 rotationOffset = Vector3.zero;
        
        // Runtime offset (can be changed via code)
        private Quaternion runtimeRotationOffset = Quaternion.identity;
        
        [Header("Attack Settings")]
        [Tooltip("Layer mask for attackable targets")]
        [SerializeField] private LayerMask attackLayerMask = ~0;
        
        [Tooltip("Visual effect for melee attacks")]
        [SerializeField] private GameObject meleeAttackEffect;
        
        [Tooltip("Visual effect for ranged attacks")]
        [SerializeField] private GameObject rangedAttackEffect;
        
        // Current weapon state
        private ItemData currentWeapon;
        private GameObject currentWeaponModel;
        private float lastAttackTime;
        private bool isAttacking = false;
        
        // Events
        public event System.Action<ItemData> OnWeaponEquipped;
        public event System.Action OnWeaponUnequipped;
        public event System.Action<float> OnAttack; // Passes damage dealt
        
        private void Awake()
        {
            // Auto-find components
            if (inventory == null)
                inventory = GetComponent<PlayerInventory>();
            
            if (playerNetwork == null)
                playerNetwork = GetComponent<PlayerNetwork>();
            
            // Try to find weapon holder if not assigned
            if (weaponHolder == null)
            {
                // Try to find existing WeaponHolder in children
                weaponHolder = FindTransformRecursive(transform, "WeaponHolder");
                
                // If still not found, create one
                if (weaponHolder == null)
                {
                    Debug.LogWarning("[PlayerWeaponSystem] WeaponHolder not found! Creating default holder.");
                    weaponHolder = new GameObject("WeaponHolder").transform;
                    weaponHolder.SetParent(transform);
                    weaponHolder.localPosition = Vector3.zero;
                    weaponHolder.localRotation = Quaternion.identity;
                }
                else
                {
                    Debug.Log($"[PlayerWeaponSystem] Found WeaponHolder at: {weaponHolder.name}");
                }
            }
            
            // Setup constraint system
            if (useConstraintSystem && handBone != null && weaponHolder != null)
            {
                SetupConstraintSystem();
            }
            else if (handBone == null)
            {
                Debug.LogWarning("[PlayerWeaponSystem] Hand bone not assigned! Please drag your character's hand bone to the 'Hand Bone' field in the inspector.");
            }
            
            // Calculate runtime rotation offset
            runtimeRotationOffset = Quaternion.Euler(rotationOffset);
        }
        
        /// <summary>
        /// Setup constraint system
        /// </summary>
        private void SetupConstraintSystem()
        {
            if (useConstraintSystem)
            {
                // Keep WeaponHolder as child of player (not bone) to avoid scale issues
                weaponHolder.SetParent(transform);
                weaponHolder.localScale = Vector3.one;
                
                Debug.Log($"[PlayerWeaponSystem] Constraint system enabled. WeaponHolder will follow: {handBone.name}");
            }
            else
            {
                // Direct parenting (old system)
                weaponHolder.SetParent(handBone);
                weaponHolder.localPosition = positionOffset;
                weaponHolder.localRotation = Quaternion.Euler(rotationOffset);
                weaponHolder.localScale = Vector3.one;
                
                Debug.Log($"[PlayerWeaponSystem] Direct parenting enabled. WeaponHolder attached to: {handBone.name}");
            }
        }
        
        /// <summary>
        /// Recursively find a transform by name (helper function)
        /// </summary>
        private Transform FindTransformRecursive(Transform parent, string targetName)
        {
            // Check if this is the target
            if (parent.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase) || 
                parent.name.Contains(targetName))
            {
                return parent;
            }
            
            // Search in children
            foreach (Transform child in parent)
            {
                Transform result = FindTransformRecursive(child, targetName);
                if (result != null)
                    return result;
            }
            
            return null;
        }
        
        private void Start()
        {
            // Only initialize for local player
            if (!IsOwner) return;
            
            // Find camera if not assigned
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
            
            // Subscribe to inventory changes
            if (inventory != null)
            {
                inventory.OnSelectedSlotChanged += OnSelectedSlotChanged;
            }
        }
        
        private void Update()
        {
            // Only local player can attack
            if (!IsOwner) return;
            
            // Update constraint system
            if (useConstraintSystem && handBone != null && weaponHolder != null)
            {
                UpdateWeaponHolderConstraint();
            }
            
            // Handle attack input (left mouse button)
            if (Input.GetMouseButtonDown(0))
            {
                TryAttack();
            }
        }
        
        /// <summary>
        /// Update weapon holder position to follow hand bone (constraint system)
        /// </summary>
        private void UpdateWeaponHolderConstraint()
        {
            // Copy position and rotation from hand bone, but keep scale independent
            weaponHolder.position = handBone.position + handBone.TransformDirection(positionOffset);
            weaponHolder.rotation = handBone.rotation * runtimeRotationOffset;
            
            // Force scale to stay at 1 (ignore bone scale)
            weaponHolder.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Called when player changes selected inventory slot
        /// </summary>
        private void OnSelectedSlotChanged(int slotIndex)
        {
            ItemData item = inventory.GetItemAtSlot(slotIndex);
            
            // Check if the selected item is a weapon
            if (item != null && item.IsWeapon)
            {
                EquipWeapon(item);
            }
            else
            {
                UnequipWeapon();
            }
        }
        
        /// <summary>
        /// Equip a weapon from inventory
        /// </summary>
        private void EquipWeapon(ItemData weaponData)
        {
            if (weaponData == null || !weaponData.IsWeapon)
            {
                Debug.LogWarning("[PlayerWeaponSystem] Attempted to equip non-weapon item");
                return;
            }
            
            // Unequip current weapon first
            if (currentWeapon != null)
            {
                UnequipWeapon();
            }
            
            currentWeapon = weaponData;
            Debug.Log($"[PlayerWeaponSystem] Equipped weapon: {weaponData.itemName} (Damage: {weaponData.weaponDamage})");
            
            // Spawn weapon model if available
            if (weaponData.handModel != null && weaponHolder != null)
            {
                currentWeaponModel = Instantiate(weaponData.handModel, weaponHolder);
                
                // Keep the prefab's original transform (position, rotation, scale)
                // This allows each weapon to have its own positioning
                // Note: If you want to override, set them after instantiation
                
                Debug.Log($"[PlayerWeaponSystem] Weapon model spawned at {weaponHolder.name}");
                Debug.Log($"[PlayerWeaponSystem] Weapon transform - Pos: {currentWeaponModel.transform.localPosition}, Rot: {currentWeaponModel.transform.localRotation.eulerAngles}, Scale: {currentWeaponModel.transform.localScale}");
            }
            else
            {
                if (weaponData.handModel == null)
                    Debug.LogWarning($"[PlayerWeaponSystem] {weaponData.itemName} has no hand model assigned!");
                if (weaponHolder == null)
                    Debug.LogWarning("[PlayerWeaponSystem] Weapon holder is not assigned!");
            }
            
            OnWeaponEquipped?.Invoke(weaponData);
        }
        
        /// <summary>
        /// Unequip current weapon
        /// </summary>
        private void UnequipWeapon()
        {
            if (currentWeapon == null) return;
            
            Debug.Log($"[PlayerWeaponSystem] Unequipped weapon: {currentWeapon.itemName}");
            currentWeapon = null;
            
            // Destroy weapon model
            if (currentWeaponModel != null)
            {
                Destroy(currentWeaponModel);
                currentWeaponModel = null;
                Debug.Log("[PlayerWeaponSystem] Weapon model destroyed");
            }
            
            OnWeaponUnequipped?.Invoke();
        }
        
        /// <summary>
        /// Try to attack with current weapon
        /// </summary>
        private void TryAttack()
        {
            // Check if weapon is equipped
            if (currentWeapon == null)
            {
                Debug.Log("[PlayerWeaponSystem] No weapon equipped");
                return;
            }
            
            // Check attack cooldown
            if (Time.time - lastAttackTime < currentWeapon.attackSpeed)
            {
                Debug.Log("[PlayerWeaponSystem] Attack on cooldown");
                return;
            }
            
            // Check if already attacking
            if (isAttacking)
            {
                return;
            }
            
            Debug.Log($"[PlayerWeaponSystem] Attacking with {currentWeapon.itemName}");
            lastAttackTime = Time.time;
            
            // Perform attack based on weapon type
            if (currentWeapon.isMeleeWeapon)
            {
                PerformMeleeAttack();
            }
            else
            {
                PerformRangedAttack();
            }
        }
        
        /// <summary>
        /// Perform a melee attack
        /// </summary>
        private void PerformMeleeAttack()
        {
            if (cameraTransform == null) return;
            
            isAttacking = true;
            
            // Raycast from camera
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, currentWeapon.attackRange, attackLayerMask))
            {
                Debug.Log($"[PlayerWeaponSystem] Melee hit: {hit.collider.gameObject.name}");
                
                // Check if hit object has PlayerNetwork (is a player)
                var targetPlayer = hit.collider.GetComponent<PlayerNetwork>();
                if (targetPlayer != null)
                {
                    // Deal damage to player
                    DealDamageToPlayerServerRpc(targetPlayer.NetworkObjectId, currentWeapon.weaponDamage);
                    Debug.Log($"[PlayerWeaponSystem] Dealt {currentWeapon.weaponDamage} damage to player");
                }
                
                // Check if hit object has EnemyHealth (is an enemy)
                var targetEnemy = hit.collider.GetComponent<EnemyHealth>();
                if (targetEnemy != null)
                {
                    // Deal damage to enemy
                    targetEnemy.TakeDamageServerRpc(currentWeapon.weaponDamage);
                    Debug.Log($"[PlayerWeaponSystem] Dealt {currentWeapon.weaponDamage} damage to enemy");
                }
                
                // Spawn hit effect
                if (meleeAttackEffect != null)
                {
                    SpawnAttackEffectServerRpc(hit.point, hit.normal, true);
                }
            }
            else
            {
                Debug.Log("[PlayerWeaponSystem] Melee attack missed");
            }
            
            OnAttack?.Invoke(currentWeapon.weaponDamage);
            
            // Reset attacking flag after a short delay
            Invoke(nameof(ResetAttacking), 0.1f);
        }
        
        /// <summary>
        /// Perform a ranged attack
        /// </summary>
        private void PerformRangedAttack()
        {
            if (cameraTransform == null) return;
            
            isAttacking = true;
            
            // Raycast from camera
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, currentWeapon.attackRange, attackLayerMask))
            {
                Debug.Log($"[PlayerWeaponSystem] Ranged hit: {hit.collider.gameObject.name}");
                
                // Check if hit object has PlayerNetwork (is a player)
                var targetPlayer = hit.collider.GetComponent<PlayerNetwork>();
                if (targetPlayer != null)
                {
                    // Deal damage to player
                    DealDamageToPlayerServerRpc(targetPlayer.NetworkObjectId, currentWeapon.weaponDamage);
                    Debug.Log($"[PlayerWeaponSystem] Dealt {currentWeapon.weaponDamage} damage to player");
                }
                
                // Check if hit object has EnemyHealth (is an enemy)
                var targetEnemy = hit.collider.GetComponent<EnemyHealth>();
                if (targetEnemy != null)
                {
                    // Deal damage to enemy
                    targetEnemy.TakeDamageServerRpc(currentWeapon.weaponDamage);
                    Debug.Log($"[PlayerWeaponSystem] Dealt {currentWeapon.weaponDamage} damage to enemy");
                }
                
                // Spawn hit effect
                if (rangedAttackEffect != null)
                {
                    SpawnAttackEffectServerRpc(hit.point, hit.normal, false);
                }
            }
            else
            {
                Debug.Log("[PlayerWeaponSystem] Ranged attack missed");
            }
            
            OnAttack?.Invoke(currentWeapon.weaponDamage);
            
            // Reset attacking flag after a short delay
            Invoke(nameof(ResetAttacking), 0.1f);
        }
        
        private void ResetAttacking()
        {
            isAttacking = false;
        }
        
        /// <summary>
        /// Deal damage to a target player (server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void DealDamageToPlayerServerRpc(ulong targetNetworkObjectId, float damage)
        {
            // Find target network object
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetNetObj))
            {
                var targetPlayer = targetNetObj.GetComponent<PlayerNetwork>();
                if (targetPlayer != null)
                {
                    // Deal damage
                    targetPlayer.ModifyHealthServerRpc(-damage);
                    Debug.Log($"[PlayerWeaponSystem] Server dealt {damage} damage to player {targetNetworkObjectId}");
                }
            }
        }
        
        /// <summary>
        /// Spawn attack effect at hit position (server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void SpawnAttackEffectServerRpc(Vector3 position, Vector3 normal, bool isMelee)
        {
            SpawnAttackEffectClientRpc(position, normal, isMelee);
        }
        
        [ClientRpc]
        private void SpawnAttackEffectClientRpc(Vector3 position, Vector3 normal, bool isMelee)
        {
            GameObject effectPrefab = isMelee ? meleeAttackEffect : rangedAttackEffect;
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, position, Quaternion.LookRotation(normal));
                Destroy(effect, 2f); // Clean up after 2 seconds
            }
        }
        
        /// <summary>
        /// Get currently equipped weapon
        /// </summary>
        public ItemData GetCurrentWeapon()
        {
            return currentWeapon;
        }
        
        /// <summary>
        /// Check if a weapon is currently equipped
        /// </summary>
        public bool HasWeaponEquipped()
        {
            return currentWeapon != null;
        }
        
        /// <summary>
        /// Get time until next attack is available
        /// </summary>
        public float GetAttackCooldown()
        {
            if (currentWeapon == null) return 0f;
            
            float timeSinceLastAttack = Time.time - lastAttackTime;
            float cooldownRemaining = currentWeapon.attackSpeed - timeSinceLastAttack;
            return Mathf.Max(0f, cooldownRemaining);
        }
        
        /// <summary>
        /// Set weapon holder offset (for fine-tuning position at runtime)
        /// </summary>
        public void SetWeaponHolderOffset(Vector3 offset)
        {
            positionOffset = offset;
        }
        
        /// <summary>
        /// Set weapon holder rotation offset (for fine-tuning rotation at runtime)
        /// </summary>
        public void SetWeaponHolderRotationOffset(Vector3 eulerAngles)
        {
            rotationOffset = eulerAngles;
            runtimeRotationOffset = Quaternion.Euler(eulerAngles);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (inventory != null)
            {
                inventory.OnSelectedSlotChanged -= OnSelectedSlotChanged;
            }
            
            // Clean up weapon model
            if (currentWeaponModel != null)
            {
                Destroy(currentWeaponModel);
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!IsOwner || cameraTransform == null || currentWeapon == null) return;
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * currentWeapon.attackRange);
        }
#endif
    }
}



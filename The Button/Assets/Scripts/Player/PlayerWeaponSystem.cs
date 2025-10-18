using Unity.Netcode;
using UnityEngine;
using TheButton.Items;
using TheButton.Network;

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
            
            // Create weapon holder if not assigned
            if (weaponHolder == null)
            {
                weaponHolder = new GameObject("WeaponHolder").transform;
                weaponHolder.SetParent(transform);
                weaponHolder.localPosition = Vector3.zero;
                weaponHolder.localRotation = Quaternion.identity;
            }
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
            
            // Handle attack input (left mouse button)
            if (Input.GetMouseButtonDown(0))
            {
                TryAttack();
            }
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
                currentWeaponModel.transform.localPosition = Vector3.zero;
                currentWeaponModel.transform.localRotation = Quaternion.identity;
                
                // Position weapon holder in front of camera
                if (cameraTransform != null)
                {
                    weaponHolder.SetParent(cameraTransform);
                    weaponHolder.localPosition = new Vector3(0.3f, -0.2f, 0.5f); // Adjust as needed
                    weaponHolder.localRotation = Quaternion.identity;
                }
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
            }
            
            // Reset weapon holder parent
            if (weaponHolder != null)
            {
                weaponHolder.SetParent(transform);
                weaponHolder.localPosition = Vector3.zero;
                weaponHolder.localRotation = Quaternion.identity;
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
                    DealDamageServerRpc(targetPlayer.NetworkObjectId, currentWeapon.weaponDamage);
                    Debug.Log($"[PlayerWeaponSystem] Dealt {currentWeapon.weaponDamage} damage to player");
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
                    DealDamageServerRpc(targetPlayer.NetworkObjectId, currentWeapon.weaponDamage);
                    Debug.Log($"[PlayerWeaponSystem] Dealt {currentWeapon.weaponDamage} damage to player");
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
        private void DealDamageServerRpc(ulong targetNetworkObjectId, float damage)
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


using Unity.Netcode;
using UnityEngine;

namespace TheButton.Player
{
    /// <summary>
    /// Handles player animation states
    /// Syncs animations across network
    /// Integrates with PlayerController for movement and PlayerWeaponSystem for attacks
    /// </summary>
    public class PlayerAnimationController : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerWeaponSystem weaponSystem;
        
        [Header("Animation Settings")]
        [Tooltip("Smoothing for animation transitions")]
        [SerializeField] private float animationSmoothTime = 0.1f;
        
        [Tooltip("Speed multiplier for animation (adjust if character moves too fast/slow)")]
        [SerializeField] private float speedMultiplier = 1f;
        
        // Animation parameter names (must match Animator Controller parameters)
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Attack = Animator.StringToHash("Attack");
        
        // State tracking
        private Vector3 lastPosition;
        private float currentSpeed;
        private float speedVelocity;
        
        private void Awake()
        {
            // Auto-find components
            if (animator == null)
                animator = GetComponent<Animator>();
            
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            
            if (weaponSystem == null)
                weaponSystem = GetComponent<PlayerWeaponSystem>();
        }
        
        private void Start()
        {
            lastPosition = transform.position;
            
            // Subscribe to weapon system events
            if (weaponSystem != null)
            {
                weaponSystem.OnAttack += OnWeaponAttack;
            }
        }
        
        private void Update()
        {
            if (!IsOwner) return;
            
            UpdateMovementAnimation();
        }
        
        /// <summary>
        /// Update movement-related animations based on player speed
        /// </summary>
        private void UpdateMovementAnimation()
        {
            // Calculate current speed
            Vector3 currentPosition = transform.position;
            float instantSpeed = (currentPosition - lastPosition).magnitude / Time.deltaTime;
            lastPosition = currentPosition;
            
            // Smooth the speed value
            currentSpeed = Mathf.SmoothDamp(currentSpeed, instantSpeed, ref speedVelocity, animationSmoothTime);
            
            // Apply speed multiplier for animation
            float animationSpeed = currentSpeed * speedMultiplier;
            
            // Update animator parameters locally
            if (animator != null)
            {
                animator.SetFloat(Speed, animationSpeed);
            }
            
            // Sync animation state to network
            UpdateAnimationStateServerRpc(animationSpeed);
        }
        
        /// <summary>
        /// Called when weapon system performs an attack
        /// </summary>
        private void OnWeaponAttack(float damage)
        {
            PlayAttackAnimation();
        }
        
        /// <summary>
        /// Play attack animation
        /// </summary>
        public void PlayAttackAnimation()
        {
            if (!IsOwner) return;
            
            if (animator != null)
            {
                animator.SetTrigger(Attack);
            }
            
            // Sync attack animation to network
            PlayAttackAnimationServerRpc();
        }
        
        /// <summary>
        /// Manually set animation speed (useful for external control)
        /// </summary>
        public void SetSpeed(float speed)
        {
            if (animator != null)
            {
                animator.SetFloat(Speed, speed);
            }
        }
        
        /// <summary>
        /// Get current animation speed
        /// </summary>
        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }
        
        /// <summary>
        /// Check if player is currently walking
        /// </summary>
        public bool IsPlayerWalking()
        {
            return currentSpeed > 0.1f;
        }
        
        #region Network Synchronization
        
        /// <summary>
        /// Update animation state on server and sync to all clients
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void UpdateAnimationStateServerRpc(float speed)
        {
            // Broadcast to all clients except the sender
            UpdateAnimationStateClientRpc(speed);
        }
        
        [ClientRpc]
        private void UpdateAnimationStateClientRpc(float speed)
        {
            // Don't update for owner (they handle it locally)
            if (IsOwner) return;
            
            if (animator != null)
            {
                animator.SetFloat(Speed, speed);
            }
        }
        
        /// <summary>
        /// Play attack animation on server and sync to all clients
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void PlayAttackAnimationServerRpc()
        {
            // Broadcast to all clients except the sender
            PlayAttackAnimationClientRpc();
        }
        
        [ClientRpc]
        private void PlayAttackAnimationClientRpc()
        {
            // Don't update for owner (they handle it locally)
            if (IsOwner) return;
            
            if (animator != null)
            {
                animator.SetTrigger(Attack);
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (weaponSystem != null)
            {
                weaponSystem.OnAttack -= OnWeaponAttack;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-assign components in editor
            if (animator == null)
                animator = GetComponent<Animator>();
            
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            
            if (weaponSystem == null)
                weaponSystem = GetComponent<PlayerWeaponSystem>();
        }
#endif
    }
}


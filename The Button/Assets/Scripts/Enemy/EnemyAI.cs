using Unity.Netcode;
using UnityEngine;
using TheButton.Player;

namespace TheButton.Enemy
{
    /// <summary>
    /// Simple enemy AI - chases and attacks player
    /// Direct movement without NavMesh
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyAI : NetworkBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private LayerMask playerLayer = ~0;
        
        [Header("Combat")]
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldown = 1.5f;
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float rotationSpeed = 5f;
        
        [Header("Movement Bounds (Optional)")]
        [SerializeField] private bool useMovementBounds = false;
        [SerializeField] private Vector3 boundsCenter = Vector3.zero;
        [SerializeField] private Vector3 boundsSize = new Vector3(20f, 10f, 20f);
        
        private EnemyHealth health;
        private Transform targetPlayer;
        private float lastAttackTime;
        private CharacterController characterController;
        
        public enum EnemyState
        {
            Idle,
            Chasing,
            Attacking
        }
        
        private EnemyState currentState = EnemyState.Idle;
        
        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            characterController = GetComponent<CharacterController>();
            
            // If no CharacterController, add one
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                characterController.radius = 0.5f;
                characterController.height = 2f;
                characterController.center = new Vector3(0, 1, 0);
            }
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Subscribe to death event
            if (health != null)
            {
                health.OnDeath += OnDeath;
            }
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (health != null)
            {
                health.OnDeath -= OnDeath;
            }
        }
        
        private void Update()
        {
            // Only server controls AI
            if (!IsServer) return;
            
            // Don't do anything if dead
            if (health != null && health.IsDead())
            {
                return;
            }
            
            // Find nearest player
            FindNearestPlayer();
            
            // Update state machine
            UpdateStateMachine();
        }
        
        private void FindNearestPlayer()
        {
            // Find all players in range
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
            
            float nearestDistance = float.MaxValue;
            Transform nearestPlayer = null;
            
            foreach (var hit in hits)
            {
                // Check if it's a player (has PlayerNetwork component)
                if (hit.GetComponent<PlayerNetwork>() != null)
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPlayer = hit.transform;
                    }
                }
            }
            
            targetPlayer = nearestPlayer;
        }
        
        private void UpdateStateMachine()
        {
            if (targetPlayer == null)
            {
                currentState = EnemyState.Idle;
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
            
            // State transitions
            if (distanceToPlayer <= attackRange)
            {
                currentState = EnemyState.Attacking;
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = EnemyState.Chasing;
            }
            else
            {
                currentState = EnemyState.Idle;
            }
            
            // Execute state behavior
            switch (currentState)
            {
                case EnemyState.Idle:
                    Idle();
                    break;
                    
                case EnemyState.Chasing:
                    Chase();
                    break;
                    
                case EnemyState.Attacking:
                    Attack();
                    break;
            }
        }
        
        private void Idle()
        {
            // Do nothing
        }
        
        private void Chase()
        {
            if (targetPlayer == null) return;
            
            // Calculate direction to player
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            direction.y = 0; // Keep on same Y level
            
            if (direction != Vector3.zero)
            {
                // Rotate towards player
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                
                // Move towards player
                Vector3 movement = direction * moveSpeed * Time.deltaTime;
                
                // Apply gravity
                movement.y = -9.81f * Time.deltaTime;
                
                // Check bounds
                Vector3 newPosition = transform.position + movement;
                if (useMovementBounds && !IsWithinBounds(newPosition))
                {
                    return; // Don't move if out of bounds
                }
                
                // Move
                if (characterController != null)
                {
                    characterController.Move(movement);
                }
                else
                {
                    transform.position += movement;
                }
            }
        }
        
        private void Attack()
        {
            if (targetPlayer == null) return;
            
            // Look at player
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
            
            // Attack if cooldown is ready
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
        
        private void PerformAttack()
        {
            if (targetPlayer == null) return;
            
            // Check if player is still in range
            float distance = Vector3.Distance(transform.position, targetPlayer.position);
            if (distance > attackRange) return;
            
            // Deal damage to player
            var playerNetwork = targetPlayer.GetComponent<PlayerNetwork>();
            if (playerNetwork != null)
            {
                playerNetwork.ModifyHealthServerRpc(-attackDamage);
                Debug.Log($"[EnemyAI] Attacked player for {attackDamage} damage");
                
                // Play attack animation/effect on all clients
                PlayAttackEffectClientRpc();
            }
        }
        
        [ClientRpc]
        private void PlayAttackEffectClientRpc()
        {
            // TODO: Play attack animation or particle effect
            Debug.Log("[EnemyAI] Attack effect played");
        }
        
        private void OnDeath()
        {
            // Stop AI when dead
            Debug.Log("[EnemyAI] Enemy AI stopped (dead)");
        }
        
        /// <summary>
        /// Check if position is within movement bounds
        /// </summary>
        private bool IsWithinBounds(Vector3 position)
        {
            Vector3 localPos = position - boundsCenter;
            return Mathf.Abs(localPos.x) <= boundsSize.x / 2f &&
                   Mathf.Abs(localPos.y) <= boundsSize.y / 2f &&
                   Mathf.Abs(localPos.z) <= boundsSize.z / 2f;
        }
        
        /// <summary>
        /// Set movement bounds at runtime
        /// </summary>
        public void SetMovementBounds(Vector3 center, Vector3 size)
        {
            boundsCenter = center;
            boundsSize = size;
            useMovementBounds = true;
        }
        
        /// <summary>
        /// Disable movement bounds
        /// </summary>
        public void DisableMovementBounds()
        {
            useMovementBounds = false;
        }
        
        /// <summary>
        /// Get current state (for debugging)
        /// </summary>
        public EnemyState GetCurrentState()
        {
            return currentState;
        }
        
        /// <summary>
        /// Get current target
        /// </summary>
        public Transform GetTarget()
        {
            return targetPlayer;
        }
        
        /// <summary>
        /// Set enemy stats (called by EnemySpawnButton)
        /// </summary>
        public void SetStats(float speed, float detection, float attack, float damage, float cooldown)
        {
            moveSpeed = speed;
            detectionRange = detection;
            attackRange = attack;
            attackDamage = damage;
            attackCooldown = cooldown;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw movement bounds
            if (useMovementBounds)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(boundsCenter, boundsSize);
            }
            
            // Draw line to target
            if (targetPlayer != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetPlayer.position);
            }
        }
#endif
    }
}


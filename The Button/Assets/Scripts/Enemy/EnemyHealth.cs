using Unity.Netcode;
using UnityEngine;

namespace TheButton.Enemy
{
    /// <summary>
    /// Enemy health system - takes damage and dies
    /// Network synchronized
    /// </summary>
    public class EnemyHealth : NetworkBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        
        [Header("Death Settings")]
        [SerializeField] private float despawnDelay = 5f;
        [SerializeField] private GameObject deathEffectPrefab;
        
        // Network synced health
        private NetworkVariable<float> currentHealth = new NetworkVariable<float>(
            100f, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server
        );
        
        // Events
        public event System.Action<float, float> OnHealthChanged; // current, max
        public event System.Action OnDeath;
        
        private bool isDead = false;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                currentHealth.Value = maxHealth;
            }
            
            // Subscribe to health changes
            currentHealth.OnValueChanged += OnHealthValueChanged;
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            currentHealth.OnValueChanged -= OnHealthValueChanged;
        }
        
        private void OnHealthValueChanged(float oldValue, float newValue)
        {
            OnHealthChanged?.Invoke(newValue, maxHealth);
            
            // Check if dead
            if (newValue <= 0 && !isDead)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Take damage (server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(float damage)
        {
            if (isDead) return;
            
            currentHealth.Value = Mathf.Max(0, currentHealth.Value - damage);
            Debug.Log($"[EnemyHealth] Took {damage} damage. Health: {currentHealth.Value}/{maxHealth}");
        }
        
        /// <summary>
        /// Heal (server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void HealServerRpc(float amount)
        {
            if (isDead) return;
            
            currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
            Debug.Log($"[EnemyHealth] Healed {amount}. Health: {currentHealth.Value}/{maxHealth}");
        }
        
        /// <summary>
        /// Die
        /// </summary>
        private void Die()
        {
            if (isDead) return;
            
            isDead = true;
            Debug.Log("[EnemyHealth] Enemy died!");
            
            OnDeath?.Invoke();
            
            // Spawn death effect
            if (deathEffectPrefab != null)
            {
                SpawnDeathEffectClientRpc(transform.position, transform.rotation);
            }
            
            // Despawn after delay
            if (IsServer)
            {
                Invoke(nameof(DespawnEnemy), despawnDelay);
            }
        }
        
        [ClientRpc]
        private void SpawnDeathEffectClientRpc(Vector3 position, Quaternion rotation)
        {
            if (deathEffectPrefab != null)
            {
                GameObject effect = Instantiate(deathEffectPrefab, position, rotation);
                Destroy(effect, 3f);
            }
        }
        
        private void DespawnEnemy()
        {
            if (IsServer && NetworkObject != null)
            {
                NetworkObject.Despawn();
            }
        }
        
        /// <summary>
        /// Get current health
        /// </summary>
        public float GetCurrentHealth()
        {
            return currentHealth.Value;
        }
        
        /// <summary>
        /// Get max health
        /// </summary>
        public float GetMaxHealth()
        {
            return maxHealth;
        }
        
        /// <summary>
        /// Check if dead
        /// </summary>
        public bool IsDead()
        {
            return isDead;
        }
        
        /// <summary>
        /// Get health percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return currentHealth.Value / maxHealth;
        }
        
        /// <summary>
        /// Set max health (called by EnemySpawnButton)
        /// </summary>
        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            
            if (IsServer)
            {
                currentHealth.Value = maxHealth;
            }
        }
    }
}

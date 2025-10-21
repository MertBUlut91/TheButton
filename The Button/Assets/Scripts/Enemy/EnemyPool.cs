using UnityEngine;
using System.Collections.Generic;

namespace TheButton.Enemy
{
    /// <summary>
    /// Pool of enemies that can spawn in a room
    /// Similar to ItemPool but for enemies
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyPool", menuName = "The Button/Enemy Pool", order = 3)]
    public class EnemyPool : ScriptableObject
    {
        [Header("Enemy Pool")]
        [Tooltip("List of all possible enemies that can spawn")]
        public List<EnemyData> availableEnemies = new List<EnemyData>();
        
        [Header("Spawn Weights (Optional)")]
        [Tooltip("If set, enemies will spawn based on these weights. Must match availableEnemies count.")]
        public List<float> spawnWeights = new List<float>();
        
        /// <summary>
        /// Get a random enemy from the pool
        /// </summary>
        public EnemyData GetRandomEnemy()
        {
            if (availableEnemies == null || availableEnemies.Count == 0)
            {
                Debug.LogError("[EnemyPool] No enemies available in pool!");
                return null;
            }
            
            // If weights are defined and valid, use weighted random
            if (spawnWeights != null && spawnWeights.Count == availableEnemies.Count)
            {
                return GetWeightedRandomEnemy();
            }
            
            // Otherwise, use uniform random
            int randomIndex = Random.Range(0, availableEnemies.Count);
            return availableEnemies[randomIndex];
        }
        
        /// <summary>
        /// Get a weighted random enemy
        /// </summary>
        private EnemyData GetWeightedRandomEnemy()
        {
            float totalWeight = 0f;
            foreach (float weight in spawnWeights)
            {
                totalWeight += weight;
            }
            
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            for (int i = 0; i < availableEnemies.Count; i++)
            {
                currentWeight += spawnWeights[i];
                if (randomValue <= currentWeight)
                {
                    return availableEnemies[i];
                }
            }
            
            // Fallback (should never reach here)
            return availableEnemies[availableEnemies.Count - 1];
        }
        
        /// <summary>
        /// Get multiple random enemies
        /// </summary>
        public List<EnemyData> GetRandomEnemies(int count)
        {
            List<EnemyData> enemies = new List<EnemyData>();
            
            for (int i = 0; i < count; i++)
            {
                EnemyData enemy = GetRandomEnemy();
                if (enemy != null)
                {
                    enemies.Add(enemy);
                }
            }
            
            return enemies;
        }
        
        /// <summary>
        /// Validate the pool
        /// </summary>
        public bool IsValid()
        {
            Debug.Log($"[EnemyPool] Validating pool: {name}");
            
            if (availableEnemies == null || availableEnemies.Count == 0)
            {
                Debug.LogWarning("[EnemyPool] No enemies in pool!");
                return false;
            }
            
            Debug.Log($"[EnemyPool] Found {availableEnemies.Count} enemies in pool");
            
            // Check each enemy
            for (int i = 0; i < availableEnemies.Count; i++)
            {
                EnemyData enemy = availableEnemies[i];
                if (enemy == null)
                {
                    Debug.LogWarning($"[EnemyPool] Enemy at index {i} is NULL!");
                    return false;
                }
                
                Debug.Log($"[EnemyPool] Enemy {i}: {enemy.enemyName}");
                
                if (enemy.enemyPrefab == null)
                {
                    Debug.LogWarning($"[EnemyPool] Enemy '{enemy.enemyName}' has no prefab assigned!");
                    return false;
                }
                
                Debug.Log($"[EnemyPool] Enemy {i} prefab: {enemy.enemyPrefab.name}");
            }
            
            // Check if weights are valid (if defined)
            if (spawnWeights != null && spawnWeights.Count > 0)
            {
                if (spawnWeights.Count != availableEnemies.Count)
                {
                    Debug.LogWarning($"[EnemyPool] Spawn weights count ({spawnWeights.Count}) doesn't match enemies count ({availableEnemies.Count})!");
                    return false;
                }
            }
            
            Debug.Log("[EnemyPool] Validation PASSED!");
            return true;
        }
    }
}


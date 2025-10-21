using UnityEngine;

namespace TheButton.Enemy
{
    /// <summary>
    /// ScriptableObject that defines an enemy type
    /// Similar to ItemData but for enemies
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "The Button/Enemy Data", order = 2)]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Display name of the enemy")]
        public string enemyName = "Enemy";
        
        [Tooltip("Description of this enemy type")]
        [TextArea(2, 4)]
        public string description = "A hostile enemy";
        
        [Header("Enemy Prefab")]
        [Tooltip("The enemy prefab to spawn (must have NetworkObject, EnemyHealth, EnemyAI)")]
        public GameObject enemyPrefab;
        
        [Header("Stats")]
        [Tooltip("Maximum health of this enemy")]
        [Range(10f, 1000f)]
        public float maxHealth = 100f;
        
        [Tooltip("Movement speed")]
        [Range(1f, 20f)]
        public float moveSpeed = 3.5f;
        
        [Tooltip("Detection range")]
        [Range(5f, 50f)]
        public float detectionRange = 10f;
        
        [Tooltip("Attack range")]
        [Range(1f, 10f)]
        public float attackRange = 2f;
        
        [Tooltip("Attack damage")]
        [Range(1f, 100f)]
        public float attackDamage = 10f;
        
        [Tooltip("Attack cooldown in seconds")]
        [Range(0.5f, 5f)]
        public float attackCooldown = 1.5f;
        
        [Header("Optional")]
        [Tooltip("Icon for UI (optional)")]
        public Sprite icon;
    }
}



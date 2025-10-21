using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using TheButton.Enemy;

namespace TheButton.Interactables
{
    /// <summary>
    /// Interactive button that spawns enemies when pressed
    /// Networked and server-authoritative
    /// Similar to SpawnButton but for enemies
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class EnemySpawnButton : NetworkBehaviour, IInteractable
    {
        [Header("Button Configuration")]
        [Tooltip("EnemyData to spawn when button is pressed")]
        [SerializeField] private EnemyData enemyToSpawn;
        
        [Tooltip("Where the enemy will spawn")]
        [SerializeField] private Transform spawnPoint;
        
        [Tooltip("Cooldown time in seconds between presses")]
        [SerializeField] private float cooldownTime = 10f;
        
        [Header("Visual Feedback")]
        [Tooltip("The mesh renderer to change color")]
        [SerializeField] private MeshRenderer buttonRenderer;
        
        [Tooltip("Normal button color")]
        [SerializeField] private Color normalColor = new Color(1f, 0.5f, 0f); // Orange
        
        [Tooltip("Color when on cooldown")]
        [SerializeField] private Color cooldownColor = Color.red;
        
        [Tooltip("Color when being pressed")]
        [SerializeField] private Color pressedColor = Color.yellow;
        
        [Header("Audio (Optional)")]
        [Tooltip("Sound to play when button is pressed")]
        [SerializeField] private AudioClip pressSound;
        
        private NetworkVariable<bool> isOnCooldown = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private NetworkVariable<float> cooldownEndTime = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        // Network sync for enemy data asset name
        private NetworkVariable<FixedString128Bytes> enemyDataAssetName = new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes(""),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private AudioSource audioSource;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            
            // Auto-find button renderer if not set
            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponent<MeshRenderer>();
            }
        }
        
        /// <summary>
        /// Set enemy data (for procedural generation)
        /// </summary>
        public void SetEnemyData(EnemyData enemyData)
        {
            if (enemyData == null)
            {
                Debug.LogError("[EnemySpawnButton] SetEnemyData called with null EnemyData!");
                return;
            }
            
            enemyToSpawn = enemyData;
            
            // Set network variable (will be synced to clients when spawned)
            if (IsSpawned && IsServer)
            {
                enemyDataAssetName.Value = enemyData.name;
            }
            
            Debug.Log($"[EnemySpawnButton] Configured to spawn {enemyData.enemyName} (asset: {enemyData.name})");
        }
        
        /// <summary>
        /// Find the global spawn point in the scene
        /// </summary>
        private Transform FindGlobalSpawnPoint()
        {
            GameObject spawnPointObj = GameObject.FindGameObjectWithTag("EnemySpawnPoint");
            if (spawnPointObj != null)
            {
                return spawnPointObj.transform;
            }
            
            Debug.LogWarning("[EnemySpawnButton] Global EnemySpawnPoint not found! Using button position + offset.");
            return transform;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            isOnCooldown.OnValueChanged += OnCooldownChanged;
            enemyDataAssetName.OnValueChanged += OnEnemyDataAssetNameChanged;
            
            // If server, sync the enemy data asset name
            if (IsServer && enemyToSpawn != null)
            {
                enemyDataAssetName.Value = enemyToSpawn.name;
                Debug.Log($"[EnemySpawnButton] Server set enemyDataAssetName to: {enemyToSpawn.name}");
            }
            // If client, load enemy data from asset name
            else if (!IsServer && !string.IsNullOrEmpty(enemyDataAssetName.Value.ToString()))
            {
                LoadEnemyDataFromAssetName(enemyDataAssetName.Value.ToString());
            }
            
            UpdateVisuals();
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            isOnCooldown.OnValueChanged -= OnCooldownChanged;
            enemyDataAssetName.OnValueChanged -= OnEnemyDataAssetNameChanged;
        }
        
        private void OnEnemyDataAssetNameChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
        {
            // Client: Load EnemyData when network variable changes
            if (!IsServer && !string.IsNullOrEmpty(newValue.ToString()))
            {
                LoadEnemyDataFromAssetName(newValue.ToString());
            }
        }
        
        private void LoadEnemyDataFromAssetName(string assetName)
        {
            // Load from Resources/Enemies/ folder
            enemyToSpawn = Resources.Load<EnemyData>($"Enemies/{assetName}");
            
            if (enemyToSpawn != null)
            {
                Debug.Log($"[EnemySpawnButton] Client loaded EnemyData: {enemyToSpawn.enemyName} from Resources");
            }
            else
            {
                Debug.LogError($"[EnemySpawnButton] Client failed to load EnemyData from Resources/Enemies/{assetName}");
            }
        }
        
        private void Update()
        {
            if (IsServer && isOnCooldown.Value)
            {
                // Check if cooldown is over
                if (Time.time >= cooldownEndTime.Value)
                {
                    isOnCooldown.Value = false;
                    Debug.Log($"[EnemySpawnButton] Cooldown ended");
                }
            }
        }
        
        public void Interact(GameObject playerGameObject)
        {
            if (!CanInteract())
            {
                Debug.Log("[EnemySpawnButton] Button is on cooldown!");
                return;
            }
            
            // Request button press on server
            PressButtonServerRpc();
        }
        
        public string GetInteractionPrompt()
        {
            if (isOnCooldown.Value)
            {
                float remainingTime = cooldownEndTime.Value - Time.time;
                return $"Enemy Spawn on cooldown ({Mathf.CeilToInt(remainingTime)}s)";
            }
            
            string enemyName = enemyToSpawn != null ? enemyToSpawn.enemyName : "Unknown Enemy";
            return $"Press E to spawn {enemyName}";
        }
        
        public bool CanInteract()
        {
            // Can interact if not on cooldown and has enemy data
            return !isOnCooldown.Value && enemyToSpawn != null;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void PressButtonServerRpc(ServerRpcParams rpcParams = default)
        {
            if (isOnCooldown.Value)
            {
                Debug.LogWarning("[EnemySpawnButton] Button pressed while on cooldown!");
                return;
            }
            
            if (enemyToSpawn == null)
            {
                Debug.LogError("[EnemySpawnButton] EnemyData is not assigned!");
                return;
            }
            
            if (enemyToSpawn.enemyPrefab == null)
            {
                Debug.LogError($"[EnemySpawnButton] EnemyData '{enemyToSpawn.enemyName}' has no prefab assigned!");
                return;
            }
            
            // Find spawn point if not assigned
            if (spawnPoint == null)
            {
                spawnPoint = FindGlobalSpawnPoint();
            }
            
            if (spawnPoint == null)
            {
                Debug.LogError("[EnemySpawnButton] Could not find spawn point!");
                return;
            }
            
            // Start cooldown
            isOnCooldown.Value = true;
            cooldownEndTime.Value = Time.time + cooldownTime;
            
            // Spawn the enemy
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;
            
            GameObject enemyInstance = Instantiate(enemyToSpawn.enemyPrefab, spawnPosition, spawnRotation);
            
            // Configure enemy with data
            EnemyHealth health = enemyInstance.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.SetMaxHealth(enemyToSpawn.maxHealth);
            }
            
            EnemyAI ai = enemyInstance.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.SetStats(
                    enemyToSpawn.moveSpeed,
                    enemyToSpawn.detectionRange,
                    enemyToSpawn.attackRange,
                    enemyToSpawn.attackDamage,
                    enemyToSpawn.attackCooldown
                );
            }
            
            // Spawn as network object
            NetworkObject networkObject = enemyInstance.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(true); // true = destroy with scene
                Debug.Log($"[EnemySpawnButton] Spawned enemy {enemyToSpawn.enemyName} at {spawnPosition}");
            }
            else
            {
                Debug.LogError($"[EnemySpawnButton] Enemy prefab '{enemyToSpawn.enemyName}' is missing NetworkObject component!");
                Destroy(enemyInstance);
            }
            
            // Play visual feedback
            PlayPressEffectClientRpc();
        }
        
        [ClientRpc]
        private void PlayPressEffectClientRpc()
        {
            // Play sound
            if (audioSource != null && pressSound != null)
            {
                audioSource.PlayOneShot(pressSound);
            }
            
            // Flash the button color
            StartCoroutine(PressFlashCoroutine());
        }
        
        private System.Collections.IEnumerator PressFlashCoroutine()
        {
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = pressedColor;
                yield return new UnityEngine.WaitForSeconds(0.2f);
                UpdateVisuals();
            }
        }
        
        private void OnCooldownChanged(bool oldValue, bool newValue)
        {
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (buttonRenderer == null) return;
            
            Color targetColor = isOnCooldown.Value ? cooldownColor : normalColor;
            buttonRenderer.material.color = targetColor;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw spawn point
            if (spawnPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(transform.position, spawnPoint.position);
            }
        }
    }
}


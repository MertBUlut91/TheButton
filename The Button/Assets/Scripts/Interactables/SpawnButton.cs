using Unity.Netcode;
using UnityEngine;
using TheButton.Items;
using TheButton.Network;

namespace TheButton.Interactables
{
    /// <summary>
    /// Interactive button that spawns items when pressed
    /// Networked and server-authoritative
    /// Uses ItemData ScriptableObject reference instead of ID
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class SpawnButton : NetworkBehaviour, IInteractable
    {
        [Header("Button Configuration")]
        [Tooltip("ItemData to spawn when button is pressed")]
        [SerializeField] private ItemData itemToSpawn;
        
        [Tooltip("Where the item will spawn")]
        [SerializeField] private Transform spawnPoint;
        
        [Tooltip("Cooldown time in seconds between presses")]
        [SerializeField] private float cooldownTime = 5f;
        
        [Header("Visual Feedback")]
        [Tooltip("The mesh renderer to change color")]
        [SerializeField] private MeshRenderer buttonRenderer;
        
        [Tooltip("Normal button color")]
        [SerializeField] private Color normalColor = Color.green;
        
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
        
        // Network sync for item data asset name
        private NetworkVariable<NetworkString> itemDataAssetName = new NetworkVariable<NetworkString>(
            new NetworkString(""),
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
        /// Set item data (for procedural generation)
        /// Spawn point will be found via GlobalItemSpawnPoint tag
        /// </summary>
        public void SetItemData(ItemData itemData)
        {
            if (itemData == null)
            {
                Debug.LogError("[SpawnButton] SetItemData called with null ItemData!");
                return;
            }
            
            itemToSpawn = itemData;
            
            // Set network variable (will be synced to clients when spawned)
            if (IsSpawned && IsServer)
            {
                itemDataAssetName.Value = new NetworkString(itemData.name);
            }
            
            Debug.Log($"[SpawnButton] Configured to spawn {itemData.itemName} (asset: {itemData.name})");
        }
        
        /// <summary>
        /// Find the global spawn point in the scene
        /// </summary>
        private Transform FindGlobalSpawnPoint()
        {
            GameObject spawnPointObj = GameObject.FindGameObjectWithTag("ItemSpawnPoint");
            if (spawnPointObj != null)
            {
                return spawnPointObj.transform;
            }
            
            Debug.LogWarning("[SpawnButton] Global ItemSpawnPoint not found! Using button position.");
            return transform;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            isOnCooldown.OnValueChanged += OnCooldownChanged;
            itemDataAssetName.OnValueChanged += OnItemDataAssetNameChanged;
            
            // If server, sync the item data asset name
            if (IsServer && itemToSpawn != null)
            {
                itemDataAssetName.Value = new NetworkString(itemToSpawn.name);
                Debug.Log($"[SpawnButton] Server set itemDataAssetName to: {itemToSpawn.name}");
            }
            // If client, load item data from asset name
            else if (!IsServer && !string.IsNullOrEmpty(itemDataAssetName.Value))
            {
                LoadItemDataFromAssetName(itemDataAssetName.Value);
            }
            
            UpdateVisuals();
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            isOnCooldown.OnValueChanged -= OnCooldownChanged;
            itemDataAssetName.OnValueChanged -= OnItemDataAssetNameChanged;
        }
        
        private void OnItemDataAssetNameChanged(NetworkString oldValue, NetworkString newValue)
        {
            // Client: Load ItemData when network variable changes
            if (!IsServer && !string.IsNullOrEmpty(newValue))
            {
                LoadItemDataFromAssetName(newValue);
            }
        }
        
        private void LoadItemDataFromAssetName(string assetName)
        {
            // Load from Resources/Items/ folder
            itemToSpawn = Resources.Load<ItemData>($"Items/{assetName}");
            
            if (itemToSpawn != null)
            {
                Debug.Log($"[SpawnButton] Client loaded ItemData: {itemToSpawn.itemName} from Resources");
            }
            else
            {
                Debug.LogError($"[SpawnButton] Client failed to load ItemData from Resources/Items/{assetName}");
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
                    Debug.Log($"[SpawnButton] Cooldown ended");
                }
            }
        }
        
        public void Interact(GameObject playerGameObject)
        {
            if (!CanInteract())
            {
                Debug.Log("[SpawnButton] Button is on cooldown!");
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
                return $"Button on cooldown ({Mathf.CeilToInt(remainingTime)}s)";
            }
            
            string itemName = itemToSpawn != null ? itemToSpawn.itemName : "Unknown Item";
            return $"Press E to spawn {itemName}";
        }
        
        public bool CanInteract()
        {
            // Can interact if not on cooldown and has item data
            return !isOnCooldown.Value && itemToSpawn != null;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void PressButtonServerRpc(ServerRpcParams rpcParams = default)
        {
            if (isOnCooldown.Value)
            {
                Debug.LogWarning("[SpawnButton] Button pressed while on cooldown!");
                return;
            }
            
            if (itemToSpawn == null)
            {
                Debug.LogError("[SpawnButton] ItemData is not assigned!");
                return;
            }
            
            // Find spawn point if not assigned
            if (spawnPoint == null)
            {
                spawnPoint = FindGlobalSpawnPoint();
            }
            
            if (spawnPoint == null)
            {
                Debug.LogError("[SpawnButton] Could not find spawn point!");
                return;
            }
            
            // Start cooldown
            isOnCooldown.Value = true;
            cooldownEndTime.Value = Time.time + cooldownTime;
            
            // Spawn the item at global spawn point
            if (ItemSpawner.Instance != null)
            {
                ItemSpawner.Instance.SpawnItemAtTransform(itemToSpawn, spawnPoint);
                Debug.Log($"[SpawnButton] Spawned item {itemToSpawn.itemName} at global spawn point {spawnPoint.position}");
            }
            else
            {
                Debug.LogError("[SpawnButton] ItemSpawner instance not found!");
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
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = isOnCooldown.Value ? cooldownColor : normalColor;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-find button renderer if not set
            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponent<MeshRenderer>();
            }
        }
        
        private void OnDrawGizmos()
        {
            // Draw line to spawn point
            if (spawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, spawnPoint.position);
                Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            }
        }
#endif
    }
}


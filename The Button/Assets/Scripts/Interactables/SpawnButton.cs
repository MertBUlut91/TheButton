using Unity.Netcode;
using UnityEngine;
using TheButton.Items;

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
        
        private AudioSource audioSource;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            isOnCooldown.OnValueChanged += OnCooldownChanged;
            UpdateVisuals();
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            isOnCooldown.OnValueChanged -= OnCooldownChanged;
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
            return !isOnCooldown.Value && spawnPoint != null;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void PressButtonServerRpc(ServerRpcParams rpcParams = default)
        {
            if (isOnCooldown.Value)
            {
                Debug.LogWarning("[SpawnButton] Button pressed while on cooldown!");
                return;
            }
            
            if (spawnPoint == null)
            {
                Debug.LogError("[SpawnButton] Spawn point is not assigned!");
                return;
            }
            
            if (itemToSpawn == null)
            {
                Debug.LogError("[SpawnButton] ItemData is not assigned!");
                return;
            }
            
            // Start cooldown
            isOnCooldown.Value = true;
            cooldownEndTime.Value = Time.time + cooldownTime;
            
            // Spawn the item
            if (ItemSpawner.Instance != null)
            {
                ItemSpawner.Instance.SpawnItemAtTransform(itemToSpawn, spawnPoint);
                Debug.Log($"[SpawnButton] Spawned item {itemToSpawn.itemName}");
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


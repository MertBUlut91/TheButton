using Unity.Netcode;
using UnityEngine;
using TheButton.Items;

namespace TheButton.Interactables
{
    /// <summary>
    /// Exit door that can be unlocked with a key
    /// When unlocked, players can interact to win the game
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class ExitDoor : NetworkBehaviour, IInteractable
    {
        [Header("Door Settings")]
        [Tooltip("Is the door locked at the start?")]
        [SerializeField] private bool startLocked = true;
        
        [Header("Visual Feedback")]
        [Tooltip("The mesh renderer to change color")]
        [SerializeField] private MeshRenderer doorRenderer;
        
        [Tooltip("Locked door color")]
        [SerializeField] private Color lockedColor = Color.red;
        
        [Tooltip("Unlocked door color")]
        [SerializeField] private Color unlockedColor = Color.green;
        
        [Tooltip("Light component for locked indicator")]
        [SerializeField] private Light doorLight;
        
        [Header("Audio (Optional)")]
        [Tooltip("Sound when door is unlocked")]
        [SerializeField] private AudioClip unlockSound;
        
        [Tooltip("Sound when trying to open locked door")]
        [SerializeField] private AudioClip lockedSound;
        
        [Tooltip("Sound when entering the door")]
        [SerializeField] private AudioClip enterSound;
        
        private NetworkVariable<bool> isLocked = new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private AudioSource audioSource;
        
        public bool IsLocked => isLocked.Value;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                isLocked.Value = startLocked;
            }
            
            isLocked.OnValueChanged += OnLockStateChanged;
            UpdateVisuals();
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            isLocked.OnValueChanged -= OnLockStateChanged;
        }
        
        public void Interact(GameObject playerGameObject)
        {
            var playerInventory = playerGameObject.GetComponent<Player.PlayerInventory>();
            
            if (isLocked.Value)
            {
                // Check if player has a key (category: Key)
                if (playerInventory != null && playerInventory.HasItemOfCategory(Items.ItemCategory.Key))
                {
                    // Use the key to unlock the door
                    int keySlot = playerInventory.GetFirstItemOfCategory(Items.ItemCategory.Key);
                    if (keySlot >= 0)
                    {
                        UnlockDoorServerRpc();
                        playerInventory.UseItemServerRpc(keySlot);
                    }
                }
                else
                {
                    // Door is locked and player has no key
                    PlayLockedSoundClientRpc();
                    Debug.Log("[ExitDoor] Door is locked! Need a key.");
                }
            }
            else
            {
                // Door is unlocked, player can exit
                PlayerEnterDoorServerRpc(playerGameObject.GetComponent<NetworkObject>().OwnerClientId);
            }
        }
        
        public string GetInteractionPrompt()
        {
            if (isLocked.Value)
            {
                return "Press E to unlock (needs Key)";
            }
            else
            {
                return "Press E to Exit and Win!";
            }
        }
        
        public bool CanInteract()
        {
            return true; // Can always interact (to show locked message or enter)
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void UnlockDoorServerRpc()
        {
            if (isLocked.Value)
            {
                isLocked.Value = false;
                Debug.Log("[ExitDoor] Door unlocked!");
                PlayUnlockSoundClientRpc();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void PlayerEnterDoorServerRpc(ulong clientId)
        {
            if (!isLocked.Value)
            {
                Debug.Log($"[ExitDoor] Player {clientId} entered the door and won!");
                
                // Notify GameManager that player won
                var gameManager = FindObjectOfType<Game.GameManager>();
                if (gameManager != null)
                {
                    gameManager.PlayerWon(clientId);
                }
                
                PlayEnterSoundClientRpc();
            }
        }
        
        [ClientRpc]
        private void PlayUnlockSoundClientRpc()
        {
            if (audioSource != null && unlockSound != null)
            {
                audioSource.PlayOneShot(unlockSound);
            }
        }
        
        [ClientRpc]
        private void PlayLockedSoundClientRpc()
        {
            if (audioSource != null && lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
        }
        
        [ClientRpc]
        private void PlayEnterSoundClientRpc()
        {
            if (audioSource != null && enterSound != null)
            {
                audioSource.PlayOneShot(enterSound);
            }
        }
        
        private void OnLockStateChanged(bool oldValue, bool newValue)
        {
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (doorRenderer != null)
            {
                doorRenderer.material.color = isLocked.Value ? lockedColor : unlockedColor;
            }
            
            if (doorLight != null)
            {
                doorLight.color = isLocked.Value ? lockedColor : unlockedColor;
                doorLight.enabled = true;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-find door renderer if not set
            if (doorRenderer == null)
            {
                doorRenderer = GetComponent<MeshRenderer>();
            }
            
            // Auto-find light if not set
            if (doorLight == null)
            {
                doorLight = GetComponentInChildren<Light>();
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = startLocked ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
            
            // Draw arrow pointing forward
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
#endif
    }
}


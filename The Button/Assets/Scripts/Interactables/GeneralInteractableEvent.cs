using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TheButton.Items;
using TheButton.Player;

namespace TheButton.Interactables
{
    /// <summary>
    /// General purpose interactable event system
    /// - Requires specific items from inventory
    /// - Hold-to-interact with configurable duration
    /// - Rotates objects during interaction
    /// - Plays animations and sound effects
    /// - One-time or reusable
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class GeneralInteractableEvent : NetworkBehaviour, IInteractable
    {
        [Header("Item Requirements")]
        [Tooltip("Items required in inventory to interact")]
        [SerializeField] private List<ItemData> requiredItems = new List<ItemData>();
        
        [Tooltip("Should items be consumed when event is activated?")]
        [SerializeField] private bool consumeItems = true;
        
        [Header("Interaction Settings")]
        [Tooltip("Can only be used once?")]
        [SerializeField] private bool oneTimeUse = false;
        
        [Tooltip("Time in seconds to hold E to complete interaction (0 = instant)")]
        [SerializeField] private float holdDuration = 2f;
        
        [Header("Rotation Settings")]
        [Tooltip("Objects to rotate during interaction")]
        [SerializeField] private List<Transform> rotatingObjects = new List<Transform>();
        
        [Tooltip("Rotation speed in degrees per second")]
        [SerializeField] private float rotationSpeed = 90f;
        
        [Tooltip("Rotation axis (normalized)")]
        [SerializeField] private Vector3 rotationAxis = Vector3.forward;
        
        [Tooltip("Should rotation continue after completion?")]
        [SerializeField] private bool continueRotationAfterComplete = false;
        
        [Header("Animation")]
        [Tooltip("Animator component (optional)")]
        [SerializeField] private Animator animator;
        
        [Tooltip("Animation trigger name for activation")]
        [SerializeField] private string activationTrigger = "Activate";
        
        [Tooltip("Animation trigger name for success")]
        [SerializeField] private string successTrigger = "Success";
        
        [Tooltip("Animation trigger name for failure")]
        [SerializeField] private string failureTrigger = "Fail";
        
        [Header("Audio")]
        [Tooltip("Sound when interaction starts")]
        [SerializeField] private AudioClip startSound;
        
        [Tooltip("Sound when event completes successfully")]
        [SerializeField] private AudioClip successSound;
        
        [Tooltip("Sound when interaction fails/cancelled")]
        [SerializeField] private AudioClip failSound;
        
        [Tooltip("Sound when missing required items")]
        [SerializeField] private AudioClip deniedSound;
        
        [Tooltip("Looping sound during hold interaction")]
        [SerializeField] private AudioClip holdLoopSound;
        
        [Header("Visual Feedback")]
        [Tooltip("Renderer to change color for visual feedback")]
        [SerializeField] private MeshRenderer visualRenderer;
        
        [Tooltip("Color when locked/inactive")]
        [SerializeField] private Color lockedColor = Color.red;
        
        [Tooltip("Color when unlocked/active")]
        [SerializeField] private Color unlockedColor = Color.green;
        
        [Tooltip("Color during interaction")]
        [SerializeField] private Color interactingColor = Color.yellow;
        
        [Header("Particle Effects")]
        [Tooltip("Particle effect when interaction starts")]
        [SerializeField] private ParticleSystem startEffect;
        
        [Tooltip("Particle effect when successfully completed")]
        [SerializeField] private ParticleSystem successEffect;
        
        [Tooltip("Particle effect when failed/cancelled")]
        [SerializeField] private ParticleSystem failEffect;
        
        [Header("Fake Cover System")]
        [Tooltip("Enable fake cover that hides the event until clicked")]
        [SerializeField] private bool useFakeCover = false;
        
        [Tooltip("Cover objects to hide/remove when clicked (e.g., panels, doors)")]
        [SerializeField] private List<GameObject> coverObjects = new List<GameObject>();
        
        [Tooltip("Text shown when hovering over cover")]
        [SerializeField] private string coverPromptText = "Press E to remove cover";
        
        [Tooltip("Sound when cover is removed")]
        [SerializeField] private AudioClip coverRemoveSound;
        
        [Tooltip("Particle effect when cover is removed")]
        [SerializeField] private ParticleSystem coverRemoveEffect;
        
        [Tooltip("Should cover be destroyed or just disabled?")]
        [SerializeField] private bool destroyCover = false;
        
        // Network state
        private NetworkVariable<bool> isActivated = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private NetworkVariable<bool> isInteracting = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private NetworkVariable<bool> isCoverRemoved = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        // Local state
        private AudioSource audioSource;
        private AudioSource loopAudioSource;
        private float interactionProgress = 0f;
        private bool isLocalPlayerInteracting = false;
        private GameObject currentInteractingPlayer;
        
        public bool IsActivated => isActivated.Value;
        public bool IsInteracting => isInteracting.Value;
        public bool IsCoverRemoved => isCoverRemoved.Value;
        public float InteractionProgress => interactionProgress;
        
        private void Awake()
        {
            // Setup audio sources
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            
            // Create second audio source for looping sounds
            loopAudioSource = gameObject.AddComponent<AudioSource>();
            loopAudioSource.playOnAwake = false;
            loopAudioSource.spatialBlend = 1f;
            loopAudioSource.loop = true;
            
            // Normalize rotation axis
            if (rotationAxis != Vector3.zero)
            {
                rotationAxis = rotationAxis.normalized;
            }
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsClient)
            {
                isActivated.OnValueChanged += OnActivatedStateChanged;
                isInteracting.OnValueChanged += OnInteractingStateChanged;
                isCoverRemoved.OnValueChanged += OnCoverRemovedStateChanged;
                UpdateVisuals();
                UpdateCoverVisibility();
            }
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                isActivated.OnValueChanged -= OnActivatedStateChanged;
                isInteracting.OnValueChanged -= OnInteractingStateChanged;
                isCoverRemoved.OnValueChanged -= OnCoverRemovedStateChanged;
            }
            
            base.OnNetworkDespawn();
        }
        
        private void Update()
        {
            // Handle rotation
            if ((isInteracting.Value || (continueRotationAfterComplete && isActivated.Value)) 
                && rotatingObjects.Count > 0)
            {
                float rotationAmount = rotationSpeed * Time.deltaTime;
                foreach (var obj in rotatingObjects)
                {
                    if (obj != null)
                    {
                        obj.Rotate(rotationAxis, rotationAmount, Space.Self);
                    }
                }
            }
            
            // Handle local player hold interaction
            if (isLocalPlayerInteracting)
            {
                // Check if player is still holding E
                if (Input.GetKey(KeyCode.E))
                {
                    interactionProgress += Time.deltaTime;
                    
                    // Check if completed
                    if (interactionProgress >= holdDuration)
                    {
                        CompleteInteractionServerRpc();
                        StopLocalInteraction();
                    }
                }
                else
                {
                    // Player released E, cancel interaction
                    CancelInteractionServerRpc();
                    StopLocalInteraction();
                }
            }
        }
        
        public void Interact(GameObject playerGameObject)
        {
            // If cover is active, remove it first
            if (useFakeCover && !isCoverRemoved.Value)
            {
                RemoveCoverServerRpc();
                return;
            }
            
            // Check if already activated and one-time use
            if (oneTimeUse && isActivated.Value)
            {
                Debug.Log($"[GeneralInteractableEvent] {gameObject.name} has already been used!");
                return;
            }
            
            // Check if already interacting
            if (isInteracting.Value)
            {
                Debug.Log($"[GeneralInteractableEvent] {gameObject.name} is already being interacted with!");
                return;
            }
            
            var playerInventory = playerGameObject.GetComponent<PlayerInventory>();
            if (playerInventory == null)
            {
                Debug.LogWarning("[GeneralInteractableEvent] Player has no inventory!");
                return;
            }
            
            // Check if player has all required items
            if (requiredItems.Count > 0 && !PlayerHasAllRequiredItems(playerInventory))
            {
                // Player doesn't have required items
                PlayDeniedSoundClientRpc();
                Debug.Log($"[GeneralInteractableEvent] Player doesn't have required items for {gameObject.name}");
                return;
            }
            
            // Start interaction
            ulong clientId = playerGameObject.GetComponent<NetworkObject>().OwnerClientId;
            
            if (holdDuration > 0)
            {
                // Start hold interaction
                StartInteractionServerRpc(clientId);
                
                // Start local interaction tracking
                if (NetworkManager.Singleton.LocalClientId == clientId)
                {
                    StartLocalInteraction(playerGameObject);
                }
            }
            else
            {
                // Instant activation
                ActivateEventServerRpc(clientId);
                
                // Consume items if needed
                if (consumeItems && requiredItems.Count > 0)
                {
                    ConsumeRequiredItems(playerInventory);
                }
            }
        }
        
        private void StartLocalInteraction(GameObject player)
        {
            isLocalPlayerInteracting = true;
            interactionProgress = 0f;
            currentInteractingPlayer = player;
        }
        
        private void StopLocalInteraction()
        {
            isLocalPlayerInteracting = false;
            interactionProgress = 0f;
            currentInteractingPlayer = null;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void StartInteractionServerRpc(ulong clientId)
        {
            if (isInteracting.Value || (oneTimeUse && isActivated.Value))
            {
                return;
            }
            
            isInteracting.Value = true;
            
            // Play start effects
            PlayStartEffectsClientRpc();
            
            Debug.Log($"[GeneralInteractableEvent] Started interaction by client {clientId}");
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void CompleteInteractionServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!isInteracting.Value)
            {
                return;
            }
            
            isInteracting.Value = false;
            isActivated.Value = true;
            
            // Consume items if needed
            if (consumeItems && requiredItems.Count > 0)
            {
                // Find the player who completed the interaction
                ulong clientId = rpcParams.Receive.SenderClientId;
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
                {
                    var playerObject = client.PlayerObject;
                    if (playerObject != null)
                    {
                        var playerInventory = playerObject.GetComponent<PlayerInventory>();
                        if (playerInventory != null)
                        {
                            ConsumeRequiredItems(playerInventory);
                            Debug.Log($"[GeneralInteractableEvent] Consumed items from player {clientId}");
                        }
                    }
                }
            }
            
            // Play success effects
            PlaySuccessEffectsClientRpc();
            
            Debug.Log($"[GeneralInteractableEvent] Interaction completed successfully!");
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void CancelInteractionServerRpc()
        {
            if (!isInteracting.Value)
            {
                return;
            }
            
            isInteracting.Value = false;
            
            // Play fail effects
            PlayFailEffectsClientRpc();
            
            Debug.Log($"[GeneralInteractableEvent] Interaction cancelled!");
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void ActivateEventServerRpc(ulong clientId)
        {
            if (oneTimeUse && isActivated.Value)
            {
                return;
            }
            
            isActivated.Value = true;
            
            // Play success effects immediately
            PlaySuccessEffectsClientRpc();
            
            Debug.Log($"[GeneralInteractableEvent] Event activated by client {clientId}!");
        }
        
        #region Client RPC Effects
        
        [ClientRpc]
        private void PlayStartEffectsClientRpc()
        {
            // Play start sound
            if (audioSource != null && startSound != null)
            {
                audioSource.PlayOneShot(startSound);
            }
            
            // Play loop sound
            if (loopAudioSource != null && holdLoopSound != null)
            {
                loopAudioSource.clip = holdLoopSound;
                loopAudioSource.Play();
            }
            
            // Play start particle effect
            if (startEffect != null)
            {
                startEffect.Play();
            }
            
            // Trigger activation animation
            if (animator != null && !string.IsNullOrEmpty(activationTrigger))
            {
                animator.SetTrigger(activationTrigger);
            }
            
            UpdateVisuals();
        }
        
        [ClientRpc]
        private void PlaySuccessEffectsClientRpc()
        {
            // Stop loop sound
            if (loopAudioSource != null && loopAudioSource.isPlaying)
            {
                loopAudioSource.Stop();
            }
            
            // Play success sound
            if (audioSource != null && successSound != null)
            {
                audioSource.PlayOneShot(successSound);
            }
            
            // Play success particle effect
            if (successEffect != null)
            {
                successEffect.Play();
            }
            
            // Trigger success animation
            if (animator != null && !string.IsNullOrEmpty(successTrigger))
            {
                animator.SetTrigger(successTrigger);
            }
            
            UpdateVisuals();
        }
        
        [ClientRpc]
        private void PlayFailEffectsClientRpc()
        {
            // Stop loop sound
            if (loopAudioSource != null && loopAudioSource.isPlaying)
            {
                loopAudioSource.Stop();
            }
            
            // Play fail sound
            if (audioSource != null && failSound != null)
            {
                audioSource.PlayOneShot(failSound);
            }
            
            // Play fail particle effect
            if (failEffect != null)
            {
                failEffect.Play();
            }
            
            // Trigger failure animation
            if (animator != null && !string.IsNullOrEmpty(failureTrigger))
            {
                animator.SetTrigger(failureTrigger);
            }
            
            UpdateVisuals();
        }
        
        [ClientRpc]
        private void PlayDeniedSoundClientRpc()
        {
            if (audioSource != null && deniedSound != null)
            {
                audioSource.PlayOneShot(deniedSound);
            }
        }
        
        #endregion
        
        #region Item Management
        
        /// <summary>
        /// Set the required items for this event (called by room generator)
        /// </summary>
        public void SetRequiredItems(List<ItemData> items)
        {
            requiredItems = items != null ? new List<ItemData>(items) : new List<ItemData>();
        }
        
        /// <summary>
        /// Check if player has all required items
        /// </summary>
        private bool PlayerHasAllRequiredItems(PlayerInventory inventory)
        {
            foreach (var item in requiredItems)
            {
                if (item != null && !inventory.HasItem(item.name))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Consume all required items from player inventory
        /// </summary>
        private void ConsumeRequiredItems(PlayerInventory inventory)
        {
            foreach (var item in requiredItems)
            {
                if (item != null)
                {
                    int slot = inventory.GetFirstItemSlot(item.name);
                    if (slot >= 0)
                    {
                        inventory.UseItemServerRpc(slot);
                    }
                }
            }
        }
        
        /// <summary>
        /// Get comma-separated list of required item names
        /// </summary>
        private string GetRequiredItemNames()
        {
            if (requiredItems.Count == 0)
            {
                return "none";
            }
            
            List<string> names = new List<string>();
            foreach (var item in requiredItems)
            {
                if (item != null)
                {
                    names.Add(item.itemName);
                }
            }
            
            return string.Join(", ", names);
        }
        
        #endregion
        
        #region IInteractable Implementation
        
        public string GetInteractionPrompt()
        {
            // If cover is active, show cover prompt
            if (useFakeCover && !isCoverRemoved.Value)
            {
                return coverPromptText;
            }
            
            if (oneTimeUse && isActivated.Value)
            {
                return "Already activated";
            }
            
            if (isInteracting.Value)
            {
                if (isLocalPlayerInteracting)
                {
                    float progress = (interactionProgress / holdDuration) * 100f;
                    return $"Hold E ({progress:F0}%)";
                }
                return "Someone is using this...";
            }
            
            string basePrompt = holdDuration > 0 
                ? $"Hold E for {holdDuration}s" 
                : "Press E to interact";
            
            if (requiredItems.Count > 0)
            {
                string itemNames = GetRequiredItemNames();
                return $"{basePrompt} (needs: {itemNames})";
            }
            
            return basePrompt;
        }
        
        public bool CanInteract()
        {
            if (oneTimeUse && isActivated.Value)
            {
                return false;
            }
            
            if (isInteracting.Value)
            {
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Fake Cover System
        
        [ServerRpc(RequireOwnership = false)]
        private void RemoveCoverServerRpc()
        {
            if (isCoverRemoved.Value)
            {
                return;
            }
            
            isCoverRemoved.Value = true;
            
            // Play cover removal effects
            PlayCoverRemovalEffectsClientRpc();
            
            Debug.Log($"[GeneralInteractableEvent] Cover removed from {gameObject.name}");
        }
        
        [ClientRpc]
        private void PlayCoverRemovalEffectsClientRpc()
        {
            // Play cover removal sound
            if (audioSource != null && coverRemoveSound != null)
            {
                audioSource.PlayOneShot(coverRemoveSound);
            }
            
            // Play cover removal particle effect
            if (coverRemoveEffect != null)
            {
                coverRemoveEffect.Play();
            }
            
            // Update cover visibility
            UpdateCoverVisibility();
        }
        
        private void OnCoverRemovedStateChanged(bool oldValue, bool newValue)
        {
            UpdateCoverVisibility();
        }
        
        private void UpdateCoverVisibility()
        {
            if (!useFakeCover || coverObjects.Count == 0)
            {
                return;
            }
            
            foreach (var cover in coverObjects)
            {
                if (cover != null)
                {
                    if (isCoverRemoved.Value)
                    {
                        if (destroyCover)
                        {
                            // Destroy the cover
                            if (IsServer)
                            {
                                Destroy(cover);
                            }
                        }
                        else
                        {
                            // Just disable it
                            cover.SetActive(false);
                        }
                    }
                    else
                    {
                        // Make sure cover is visible
                        cover.SetActive(true);
                    }
                }
            }
        }
        
        #endregion
        
        #region Visual Updates
        
        private void OnActivatedStateChanged(bool oldValue, bool newValue)
        {
            UpdateVisuals();
        }
        
        private void OnInteractingStateChanged(bool oldValue, bool newValue)
        {
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (visualRenderer != null)
            {
                Color targetColor = lockedColor;
                
                if (isActivated.Value)
                {
                    targetColor = unlockedColor;
                }
                else if (isInteracting.Value)
                {
                    targetColor = interactingColor;
                }
                
                visualRenderer.material.color = targetColor;
            }
        }
        
        #endregion
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure hold duration is not negative
            if (holdDuration < 0)
            {
                holdDuration = 0;
            }
            
            // Normalize rotation axis
            if (rotationAxis != Vector3.zero)
            {
                rotationAxis = rotationAxis.normalized;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw rotation axis for rotating objects
            if (rotatingObjects.Count > 0)
            {
                Gizmos.color = Color.cyan;
                foreach (var obj in rotatingObjects)
                {
                    if (obj != null)
                    {
                        Vector3 worldAxis = obj.TransformDirection(rotationAxis);
                        Gizmos.DrawRay(obj.position, worldAxis * 0.5f);
                    }
                }
            }
        }
#endif
    }
}


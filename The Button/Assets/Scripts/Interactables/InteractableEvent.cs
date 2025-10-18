using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TheButton.Items;
using TheButton.Player;

namespace TheButton.Interactables
{
    /// <summary>
    /// Base class for interactive events in the room
    /// Events can require specific items to unlock/activate
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public abstract class InteractableEvent : NetworkBehaviour, IInteractable
    {
        [Header("Event Settings")]
        [Tooltip("Items required to interact with this event")]
        [SerializeField] protected List<ItemData> requiredItems = new List<ItemData>();
        
        [Tooltip("Can only be used once?")]
        [SerializeField] protected bool oneTimeUse = false;
        
        [Header("Visual Feedback")]
        [Tooltip("Renderer to change color for visual feedback")]
        [SerializeField] protected MeshRenderer visualRenderer;
        
        [Tooltip("Color when locked/inactive")]
        [SerializeField] protected Color lockedColor = Color.red;
        
        [Tooltip("Color when unlocked/active")]
        [SerializeField] protected Color unlockedColor = Color.green;
        
        [Header("Audio (Optional)")]
        [Tooltip("Sound when event is activated")]
        [SerializeField] protected AudioClip activateSound;
        
        [Tooltip("Sound when trying to interact without required items")]
        [SerializeField] protected AudioClip deniedSound;
        
        // Network state
        protected NetworkVariable<bool> isActivated = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        protected AudioSource audioSource;
        
        public bool IsActivated => isActivated.Value;
        
        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsClient)
            {
                isActivated.OnValueChanged += OnActivatedStateChanged;
                UpdateVisuals();
            }
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                isActivated.OnValueChanged -= OnActivatedStateChanged;
            }
            
            base.OnNetworkDespawn();
        }
        
        /// <summary>
        /// Set the required items for this event (called by room generator)
        /// </summary>
        public virtual void SetRequiredItems(List<ItemData> items)
        {
            requiredItems = items != null ? new List<ItemData>(items) : new List<ItemData>();
        }
        
        public virtual void Interact(GameObject playerGameObject)
        {
            if (oneTimeUse && isActivated.Value)
            {
                Debug.Log($"[InteractableEvent] {gameObject.name} has already been used!");
                return;
            }
            
            var playerInventory = playerGameObject.GetComponent<PlayerInventory>();
            
            // Check if player has all required items
            if (HasRequiredItems())
            {
                if (playerInventory != null && PlayerHasAllRequiredItems(playerInventory))
                {
                    // Player has all items, activate event
                    ActivateEventServerRpc(playerGameObject.GetComponent<NetworkObject>().OwnerClientId);
                    
                    // Consume required items
                    ConsumeRequiredItems(playerInventory);
                }
                else
                {
                    // Player doesn't have required items
                    PlayDeniedSoundClientRpc();
                    Debug.Log($"[InteractableEvent] Player doesn't have required items for {gameObject.name}");
                }
            }
            else
            {
                // No items required, activate directly
                ActivateEventServerRpc(playerGameObject.GetComponent<NetworkObject>().OwnerClientId);
            }
        }
        
        public virtual string GetInteractionPrompt()
        {
            if (oneTimeUse && isActivated.Value)
            {
                return "Already activated";
            }
            
            if (HasRequiredItems())
            {
                string itemNames = GetRequiredItemNames();
                return $"Press E (needs: {itemNames})";
            }
            
            return "Press E to interact";
        }
        
        public virtual bool CanInteract()
        {
            if (oneTimeUse && isActivated.Value)
            {
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Check if this event has any required items
        /// </summary>
        protected bool HasRequiredItems()
        {
            return requiredItems != null && requiredItems.Count > 0;
        }
        
        /// <summary>
        /// Check if player has all required items
        /// </summary>
        protected bool PlayerHasAllRequiredItems(PlayerInventory inventory)
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
        protected void ConsumeRequiredItems(PlayerInventory inventory)
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
        protected string GetRequiredItemNames()
        {
            if (!HasRequiredItems())
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
        
        [ServerRpc(RequireOwnership = false)]
        protected virtual void ActivateEventServerRpc(ulong clientId)
        {
            if (oneTimeUse && isActivated.Value)
            {
                return;
            }
            
            isActivated.Value = true;
            
            // Call the abstract method for child classes to implement
            OnEventActivated(clientId);
            
            PlayActivateSoundClientRpc();
        }
        
        /// <summary>
        /// Called when the event is activated (override in child classes)
        /// </summary>
        protected abstract void OnEventActivated(ulong clientId);
        
        [ClientRpc]
        protected virtual void PlayActivateSoundClientRpc()
        {
            if (audioSource != null && activateSound != null)
            {
                audioSource.PlayOneShot(activateSound);
            }
        }
        
        [ClientRpc]
        protected virtual void PlayDeniedSoundClientRpc()
        {
            if (audioSource != null && deniedSound != null)
            {
                audioSource.PlayOneShot(deniedSound);
            }
        }
        
        protected virtual void OnActivatedStateChanged(bool oldValue, bool newValue)
        {
            UpdateVisuals();
        }
        
        protected virtual void UpdateVisuals()
        {
            if (visualRenderer != null)
            {
                visualRenderer.material.color = isActivated.Value ? unlockedColor : lockedColor;
            }
        }
    }
}


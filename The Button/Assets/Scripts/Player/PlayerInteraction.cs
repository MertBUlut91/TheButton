using Unity.Netcode;
using UnityEngine;
using TheButton.Interactables;

namespace TheButton.Player
{
    /// <summary>
    /// Handles player interaction with interactable objects
    /// Uses raycast to detect objects and E key to interact
    /// </summary>
    public class PlayerInteraction : NetworkBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Maximum distance to interact with objects")]
        [SerializeField] private float interactionRange = 3f;
        
        [Tooltip("Layer mask for interactable objects")]
        [SerializeField] private LayerMask interactableLayer = ~0;
        
        [Tooltip("Key to interact with objects")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        
        [Header("Raycast Settings")]
        [Tooltip("Camera transform for raycasting (auto-assigned if null)")]
        [SerializeField] private Transform cameraTransform;
        
        private IInteractable currentInteractable;
        private GameObject currentInteractableObject;
        private PlayerItemUsage playerItemUsage;
        
        // Event for UI to subscribe to
        public event System.Action<string> OnInteractionPromptChanged;
        
        private void Start()
        {
            // Only initialize for local player
            if (!IsOwner) return;
            
            // Find PlayerItemUsage component
            playerItemUsage = GetComponent<PlayerItemUsage>();
            if (playerItemUsage == null)
            {
                Debug.LogWarning("[PlayerInteraction] PlayerItemUsage component not found!");
            }
            
            // Auto-find camera if not assigned
            if (cameraTransform == null)
            {
                // First try to find child camera
                cameraTransform = transform.Find("PlayerCamera");
                
                if (cameraTransform == null)
                {
                    // Try to find in nested children
                    var cameras = GetComponentsInChildren<Camera>(true);
                    foreach (var cam in cameras)
                    {
                        if (cam.gameObject.activeInHierarchy)
                        {
                            cameraTransform = cam.transform;
                            Debug.Log($"[PlayerInteraction] Found camera: {cam.name}");
                            break;
                        }
                    }
                }
                
                if (cameraTransform == null)
                {
                    Debug.LogError($"[PlayerInteraction] Camera not found for player {OwnerClientId}!");
                }
                else
                {
                    Debug.Log($"[PlayerInteraction] Player {OwnerClientId} camera setup: {cameraTransform.name}");
                }
            }
        }
        
        private void Update()
        {
            // Only local player can interact
            if (!IsOwner) return;
            
            // Don't interact if in placement mode
            if (playerItemUsage != null && playerItemUsage.IsInPlacementMode())
            {
                // Clear current interactable while in placement mode
                if (currentInteractable != null)
                {
                    currentInteractable = null;
                    currentInteractableObject = null;
                    UpdateInteractionPrompt();
                }
                return;
            }
            
            // Check for interactable objects
            DetectInteractable();
            
            // Handle interaction input
            if (Input.GetKeyDown(interactKey))
            {
                if (currentInteractable != null)
                {
                    Debug.Log($"[PlayerInteraction] Player {OwnerClientId} attempting to interact with {currentInteractableObject?.name}");
                    
                    if (currentInteractable.CanInteract())
                    {
                        Debug.Log($"[PlayerInteraction] Player {OwnerClientId} interacting!");
                        currentInteractable.Interact(gameObject);
                    }
                    else
                    {
                        Debug.Log($"[PlayerInteraction] Player {OwnerClientId} - CanInteract returned false");
                    }
                }
                else
                {
                    Debug.Log($"[PlayerInteraction] Player {OwnerClientId} pressed E but no interactable found");
                }
            }
        }
        
        private void DetectInteractable()
        {
            if (cameraTransform == null)
            {
                Debug.LogWarning($"[PlayerInteraction] Player {OwnerClientId} - Camera is null!");
                return;
            }
            
            IInteractable previousInteractable = currentInteractable;
            currentInteractable = null;
            currentInteractableObject = null;
            
            // Perform raycast from camera
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
            {
                // Check if hit object has IInteractable
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    currentInteractable = interactable;
                    currentInteractableObject = hit.collider.gameObject;
                    
                    // Only log when first detecting a new interactable
                    if (previousInteractable != currentInteractable)
                    {
                        Debug.Log($"[PlayerInteraction] Player {OwnerClientId} detected interactable: {hit.collider.gameObject.name}");
                    }
                }
            }
            
            // Update UI if interactable changed
            if (currentInteractable != previousInteractable)
            {
                UpdateInteractionPrompt();
            }
        }
        
        private void UpdateInteractionPrompt()
        {
            if (currentInteractable != null)
            {
                string prompt = currentInteractable.GetInteractionPrompt();
                OnInteractionPromptChanged?.Invoke(prompt);
            }
            else
            {
                OnInteractionPromptChanged?.Invoke(string.Empty);
            }
        }
        
        public bool IsLookingAtInteractable()
        {
            return currentInteractable != null;
        }
        
        public IInteractable GetCurrentInteractable()
        {
            return currentInteractable;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!IsOwner || cameraTransform == null) return;
            
            // Draw interaction ray
            Gizmos.color = currentInteractable != null ? Color.green : Color.red;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactionRange);
        }
#endif
    }
}


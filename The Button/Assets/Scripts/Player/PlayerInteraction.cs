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
        
        // Event for UI to subscribe to
        public event System.Action<string> OnInteractionPromptChanged;
        
        private void Start()
        {
            // Auto-find camera if not assigned
            if (cameraTransform == null)
            {
                var playerController = GetComponent<PlayerController>();
                if (playerController != null)
                {
                    cameraTransform = playerController.transform.Find("PlayerCamera");
                }
                
                if (cameraTransform == null)
                {
                    cameraTransform = Camera.main?.transform;
                }
            }
        }
        
        private void Update()
        {
            // Only local player can interact
            if (!IsOwner) return;
            
            // Check for interactable objects
            DetectInteractable();
            
            // Handle interaction input
            if (Input.GetKeyDown(interactKey) && currentInteractable != null)
            {
                if (currentInteractable.CanInteract())
                {
                    currentInteractable.Interact(gameObject);
                }
            }
        }
        
        private void DetectInteractable()
        {
            if (cameraTransform == null) return;
            
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


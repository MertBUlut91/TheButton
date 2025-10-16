using UnityEngine;
using TMPro;
using TheButton.Player;

namespace TheButton.UI
{
    /// <summary>
    /// Displays interaction prompts when player looks at interactable objects
    /// Shows "Press E to..." messages
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Text to display the interaction prompt")]
        [SerializeField] private TextMeshProUGUI promptText;
        
        [Tooltip("Container to show/hide the prompt")]
        [SerializeField] private GameObject promptContainer;
        
        private PlayerInteraction playerInteraction;
        
        private void Start()
        {
            // Find local player's interaction component
            FindLocalPlayerInteraction();
            
            // Hide prompt initially
            if (promptContainer != null)
            {
                promptContainer.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (playerInteraction == null)
            {
                FindLocalPlayerInteraction();
            }
        }
        
        private void FindLocalPlayerInteraction()
        {
            // Find all player interactions
            var interactions = FindObjectsOfType<PlayerInteraction>();
            foreach (var interaction in interactions)
            {
                if (interaction.IsOwner)
                {
                    playerInteraction = interaction;
                    
                    // Subscribe to prompt changes
                    playerInteraction.OnInteractionPromptChanged += UpdatePrompt;
                    
                    Debug.Log("[InteractionPromptUI] Found local player interaction");
                    break;
                }
            }
        }
        
        private void UpdatePrompt(string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                // Hide prompt
                if (promptContainer != null)
                {
                    promptContainer.SetActive(false);
                }
            }
            else
            {
                // Show prompt
                if (promptText != null)
                {
                    promptText.text = prompt;
                }
                
                if (promptContainer != null)
                {
                    promptContainer.SetActive(true);
                }
            }
        }
        
        private void OnDestroy()
        {
            if (playerInteraction != null)
            {
                playerInteraction.OnInteractionPromptChanged -= UpdatePrompt;
            }
        }
    }
}


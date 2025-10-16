using UnityEngine;

namespace TheButton.Interactables
{
    /// <summary>
    /// Interface for objects that can be interacted with by players
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when a player interacts with this object
        /// </summary>
        /// <param name="playerGameObject">The player GameObject that interacted</param>
        void Interact(GameObject playerGameObject);
        
        /// <summary>
        /// Get the interaction prompt text to display
        /// </summary>
        string GetInteractionPrompt();
        
        /// <summary>
        /// Check if this interactable can currently be interacted with
        /// </summary>
        bool CanInteract();
    }
}


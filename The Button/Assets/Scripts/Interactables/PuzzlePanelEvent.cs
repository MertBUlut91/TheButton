using UnityEngine;
using TheButton.Game;

namespace TheButton.Interactables
{
    /// <summary>
    /// Puzzle panel event that requires a screwdriver to open and solve
    /// Can be used for electrical panels, control boxes, etc.
    /// </summary>
    public class PuzzlePanelEvent : InteractableEvent
    {
        [Header("Panel Settings")]
        [Tooltip("Panel door to open")]
        [SerializeField] private Transform panelDoor;
        
        [Tooltip("Door open rotation (Euler angles)")]
        [SerializeField] private Vector3 openRotation = new Vector3(0, 90, 0);
        
        [Tooltip("Door opening speed")]
        [SerializeField] private float openSpeed = 2f;
        
        [Header("Puzzle Elements")]
        [Tooltip("Lights that turn on when activated")]
        [SerializeField] private Light[] puzzleLights;
        
        [Tooltip("Materials to change when solved")]
        [SerializeField] private MeshRenderer[] indicatorRenderers;
        
        private Quaternion targetRotation;
        private Quaternion startRotation;
        private bool isOpening = false;
        private float openProgress = 0f;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (panelDoor != null)
            {
                startRotation = panelDoor.localRotation;
                targetRotation = Quaternion.Euler(openRotation);
            }
        }
        
        protected override void OnEventActivated(ulong clientId)
        {
            Debug.Log($"[PuzzlePanelEvent] Puzzle panel opened by player {clientId}!");
            
            // Open the panel door
            if (panelDoor != null)
            {
                OpenPanelDoorClientRpc();
            }
            
            // Activate puzzle lights
            if (puzzleLights != null && puzzleLights.Length > 0)
            {
                ActivateLightsClientRpc();
            }
            
            // Change indicator colors
            UpdateIndicatorsClientRpc();
            
            // You can add more logic here:
            // - Start a mini-game
            // - Unlock a secret area
            // - Activate other events
            // - Give rewards
        }
        
        [Unity.Netcode.ClientRpc]
        private void OpenPanelDoorClientRpc()
        {
            isOpening = true;
            openProgress = 0f;
        }
        
        [Unity.Netcode.ClientRpc]
        private void ActivateLightsClientRpc()
        {
            if (puzzleLights != null)
            {
                foreach (var light in puzzleLights)
                {
                    if (light != null)
                    {
                        light.enabled = true;
                        light.color = unlockedColor;
                    }
                }
            }
        }
        
        [Unity.Netcode.ClientRpc]
        private void UpdateIndicatorsClientRpc()
        {
            if (indicatorRenderers != null)
            {
                foreach (var renderer in indicatorRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.material.color = unlockedColor;
                    }
                }
            }
        }
        
        private void Update()
        {
            if (isOpening && panelDoor != null && openProgress < 1f)
            {
                openProgress += Time.deltaTime * openSpeed;
                openProgress = Mathf.Clamp01(openProgress);
                
                panelDoor.localRotation = Quaternion.Lerp(startRotation, targetRotation, openProgress);
                
                if (openProgress >= 1f)
                {
                    isOpening = false;
                }
            }
        }
        
        public override string GetInteractionPrompt()
        {
            if (oneTimeUse && isActivated.Value)
            {
                return "Panel opened";
            }
            
            if (HasRequiredItems())
            {
                return "Press E to open panel (needs Screwdriver)";
            }
            
            return "Press E to open panel";
        }
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            // Update puzzle lights
            if (puzzleLights != null)
            {
                foreach (var light in puzzleLights)
                {
                    if (light != null)
                    {
                        light.color = isActivated.Value ? unlockedColor : lockedColor;
                    }
                }
            }
        }
    }
}


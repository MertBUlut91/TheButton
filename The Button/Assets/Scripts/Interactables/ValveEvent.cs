using UnityEngine;
using TheButton.Game;

namespace TheButton.Interactables
{
    /// <summary>
    /// Valve event that requires a wrench to activate
    /// Once activated, could trigger water flow, door unlock, etc.
    /// </summary>
    public class ValveEvent : InteractableEvent
    {
        [Header("Valve Settings")]
        [Tooltip("Rotation speed when activated")]
        [SerializeField] private float rotationSpeed = 90f;
        
        [Tooltip("Transform to rotate (usually the valve handle)")]
        [SerializeField] private Transform valveHandle;
        
        [Header("Effects")]
        [Tooltip("Particle effect when valve is opened")]
        [SerializeField] private ParticleSystem activationEffect;
        
        private bool isRotating = false;
        
        protected override void OnEventActivated(ulong clientId)
        {
            Debug.Log($"[ValveEvent] Valve activated by player {clientId}!");
            
            // Trigger valve opening effects
            if (activationEffect != null)
            {
                ActivateEffectClientRpc();
            }
            
            // Start rotating the valve handle
            if (valveHandle != null)
            {
                StartRotatingValveClientRpc();
            }
            
            // You can add more logic here:
            // - Open a door
            // - Start water flow
            // - Unlock another area
            // - Trigger puzzle completion
        }
        
        [Unity.Netcode.ClientRpc]
        private void ActivateEffectClientRpc()
        {
            if (activationEffect != null && !activationEffect.isPlaying)
            {
                activationEffect.Play();
            }
        }
        
        [Unity.Netcode.ClientRpc]
        private void StartRotatingValveClientRpc()
        {
            isRotating = true;
        }
        
        private void Update()
        {
            if (isRotating && valveHandle != null)
            {
                // Rotate the valve handle
                valveHandle.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
        }
        
        public override string GetInteractionPrompt()
        {
            if (oneTimeUse && isActivated.Value)
            {
                return "Valve opened";
            }
            
            if (HasRequiredItems())
            {
                return "Press E to turn valve (needs Wrench)";
            }
            
            return "Press E to turn valve";
        }
    }
}


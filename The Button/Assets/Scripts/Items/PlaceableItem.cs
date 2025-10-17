using UnityEngine;

namespace TheButton.Items
{
    /// <summary>
    /// Component for collectible items that can be placed in the world
    /// Manages visual states and placement preview
    /// </summary>
    public class PlaceableItem : MonoBehaviour
    {
        [Header("Visual References")]
        [Tooltip("The normal visual GameObject (shown after placement)")]
        [SerializeField] private GameObject normalVisual;
        
        [Tooltip("The placement preview visual GameObject (shown during placement)")]
        [SerializeField] private GameObject placementVisual;
        
        [Header("Placement Materials")]
        [Tooltip("Material(s) to use for placement preview - will be colored based on validity")]
        [SerializeField] private Material[] placementMaterials;
        
        [Header("Preview Settings")]
        [Tooltip("Color when placement is valid")]
        [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.5f);
        
        [Tooltip("Color when placement is invalid")]
        [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);
        
        private Renderer[] placementRenderers;
        private Material[][] originalMaterials; // Store originals in case we need them
        
        private void Awake()
        {
            // Cache placement renderers
            if (placementVisual != null)
            {
                placementRenderers = placementVisual.GetComponentsInChildren<Renderer>();
            }
        }
        
        /// <summary>
        /// Set the visual mode (normal or placement)
        /// </summary>
        public void SetVisualMode(VisualMode mode)
        {
            switch (mode)
            {
                case VisualMode.Normal:
                    if (normalVisual != null) normalVisual.SetActive(true);
                    if (placementVisual != null) placementVisual.SetActive(false);
                    break;
                    
                case VisualMode.Placement:
                    if (normalVisual != null) normalVisual.SetActive(false);
                    if (placementVisual != null) placementVisual.SetActive(true);
                    ApplyPlacementMaterials();
                    break;
                    
                case VisualMode.Hidden:
                    if (normalVisual != null) normalVisual.SetActive(false);
                    if (placementVisual != null) placementVisual.SetActive(false);
                    break;
            }
        }
        
        /// <summary>
        /// Apply the placement materials to the placement visual
        /// </summary>
        private void ApplyPlacementMaterials()
        {
            if (placementRenderers == null || placementRenderers.Length == 0)
                return;
            
            if (placementMaterials == null || placementMaterials.Length == 0)
            {
                Debug.LogWarning($"[PlaceableItem] No placement materials assigned on {gameObject.name}");
                return;
            }
            
            // Apply placement materials to all renderers
            foreach (var renderer in placementRenderers)
            {
                if (renderer == null) continue;
                
                // Create material instances to avoid modifying the original assets
                Material[] materialInstances = new Material[placementMaterials.Length];
                for (int i = 0; i < placementMaterials.Length; i++)
                {
                    materialInstances[i] = new Material(placementMaterials[i]);
                }
                
                renderer.materials = materialInstances;
            }
        }
        
        /// <summary>
        /// Update the placement color based on validity
        /// </summary>
        public void SetPlacementValid(bool isValid)
        {
            if (placementRenderers == null || placementRenderers.Length == 0)
                return;
            
            Color targetColor = isValid ? validColor : invalidColor;
            
            foreach (var renderer in placementRenderers)
            {
                if (renderer == null) continue;
                
                foreach (var mat in renderer.materials)
                {
                    if (mat == null) continue;
                    
                    // Update color/emission based on shader properties
                    if (mat.HasProperty("_Color"))
                    {
                        mat.color = targetColor;
                    }
                    
                    if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", targetColor);
                    }
                    
                    // Optional: Update emission for better visibility
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        Color emissionColor = targetColor * 0.5f;
                        mat.SetColor("_EmissionColor", emissionColor);
                        mat.EnableKeyword("_EMISSION");
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the placement visual renderers
        /// </summary>
        public Renderer[] GetPlacementRenderers()
        {
            return placementRenderers;
        }
        
        /// <summary>
        /// Validate the setup in editor
        /// </summary>
        public bool ValidateSetup()
        {
            bool isValid = true;
            
            if (normalVisual == null)
            {
                Debug.LogWarning($"[PlaceableItem] Normal visual not assigned on {gameObject.name}");
                isValid = false;
            }
            
            if (placementVisual == null)
            {
                Debug.LogWarning($"[PlaceableItem] Placement visual not assigned on {gameObject.name}");
                isValid = false;
            }
            
            if (placementMaterials == null || placementMaterials.Length == 0)
            {
                Debug.LogWarning($"[PlaceableItem] Placement materials not assigned on {gameObject.name}");
                isValid = false;
            }
            
            return isValid;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-refresh renderer cache when values change in editor
            if (placementVisual != null)
            {
                placementRenderers = placementVisual.GetComponentsInChildren<Renderer>();
            }
        }
        
        private void Reset()
        {
            // Auto-find visuals when component is first added
            Transform[] children = GetComponentsInChildren<Transform>(true);
            
            foreach (var child in children)
            {
                if (child == transform) continue;
                
                string childName = child.name.ToLower();
                
                if (childName.Contains("visual") && !childName.Contains("placement"))
                {
                    normalVisual = child.gameObject;
                }
                else if (childName.Contains("placement"))
                {
                    placementVisual = child.gameObject;
                }
            }
        }
#endif
    }
    
    /// <summary>
    /// Visual mode for placeable items
    /// </summary>
    public enum VisualMode
    {
        Normal,      // Show normal visual
        Placement,   // Show placement preview visual
        Hidden       // Hide all visuals
    }
}


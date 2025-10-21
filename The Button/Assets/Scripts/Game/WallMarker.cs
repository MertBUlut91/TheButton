using UnityEngine;

namespace TheButton.Game
{
    /// <summary>
    /// Marks a wall cube as a potential spawn location for buttons or events.
    /// The marker itself is a visible wall cube that can be replaced during room generation.
    /// </summary>
    public class WallMarker : MonoBehaviour
    {
        [Header("Visual")]
        [Tooltip("Marker'ın mesh renderer'ı (button/event yerleşince devre dışı kalacak)")]
        public MeshRenderer markerRenderer;
        
        [Header("Info")]
        [Tooltip("Marker'ın unique ID'si (debug için)")]
        public int markerId;
        
        private void Reset()
        {
            // Auto-assign mesh renderer
            markerRenderer = GetComponent<MeshRenderer>();
        }
        
        private void OnValidate()
        {
            // Auto-assign mesh renderer if not set
            if (markerRenderer == null)
            {
                markerRenderer = GetComponent<MeshRenderer>();
            }
        }
        
        /// <summary>
        /// Marker'ı devre dışı bırak (button/event yerleştirildiğinde)
        /// </summary>
        public void DisableMarker()
        {
            if (markerRenderer != null)
            {
                markerRenderer.enabled = false;
            }
            
            // Collider'ı da devre dışı bırak (button/event'in kendi collider'ı olacak)
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
        
        /// <summary>
        /// Marker'ı tekrar aktif et (test/debug için)
        /// </summary>
        public void EnableMarker()
        {
            if (markerRenderer != null)
            {
                markerRenderer.enabled = true;
            }
            
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
        
        // Gizmos ile görselleştirme
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Sarı, yarı saydam
            Gizmos.DrawCube(transform.position, Vector3.one * 0.9f);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
            
            // ID'yi göster
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, $"Marker #{markerId}");
            #endif
        }
    }
}


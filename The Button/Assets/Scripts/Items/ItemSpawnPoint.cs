using UnityEngine;

namespace TheButton.Items
{
    /// <summary>
    /// Marks a location where items can be spawned
    /// Only used as a visual marker in the editor
    /// </summary>
    public class ItemSpawnPoint : MonoBehaviour
    {
        [Header("Spawn Point Info")]
        [Tooltip("Unique identifier for this spawn point")]
        public int spawnPointId;
        
        [Tooltip("Description of this spawn point")]
        public string description;
        
        [Header("Gizmo Settings")]
        [Tooltip("Color of the gizmo in editor")]
        public Color gizmoColor = Color.yellow;
        
        [Tooltip("Size of the gizmo sphere")]
        public float gizmoSize = 0.5f;
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoSize);
            
            // Draw arrow pointing up
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * gizmoSize * 2);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoSize);
            
            // Draw label
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (gizmoSize * 2 + 0.5f),
                $"Spawn Point {spawnPointId}\n{description}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = gizmoColor },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                }
            );
        }
#endif
    }
}


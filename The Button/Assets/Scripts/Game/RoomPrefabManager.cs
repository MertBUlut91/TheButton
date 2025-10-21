using System.Collections.Generic;
using UnityEngine;

namespace TheButton.Game
{
    /// <summary>
    /// Manages a room prefab and its wall markers.
    /// This component should be on the root of the room prefab.
    /// Wall markers are manually assigned in the Inspector.
    /// </summary>
    public class RoomPrefabManager : MonoBehaviour
    {
        [Header("Wall Markers")]
        [Tooltip("Manuel olarak atanan duvar marker'ları - button veya event yerleşebilir")]
        public List<WallMarker> wallMarkers = new List<WallMarker>();
        
        [Header("Info")]
        [Tooltip("Oda prefab'ının ismi")]
        public string roomName = "Default Room";
        
        [Tooltip("Oda açıklaması")]
        [TextArea(2, 4)]
        public string description = "A procedurally populated room";
        
        /// <summary>
        /// Get all wall markers
        /// </summary>
        public List<WallMarker> GetAllMarkers()
        {
            return wallMarkers;
        }
        
        /// <summary>
        /// Get marker count
        /// </summary>
        public int GetMarkerCount()
        {
            return wallMarkers != null ? wallMarkers.Count : 0;
        }
        
        /// <summary>
        /// Validate the room prefab setup
        /// </summary>
        public bool Validate()
        {
            if (wallMarkers == null || wallMarkers.Count == 0)
            {
                Debug.LogError($"[RoomPrefabManager] '{roomName}' has no wall markers assigned!");
                return false;
            }
            
            // Check for null markers
            int nullCount = 0;
            for (int i = 0; i < wallMarkers.Count; i++)
            {
                if (wallMarkers[i] == null)
                {
                    Debug.LogWarning($"[RoomPrefabManager] '{roomName}' has null marker at index {i}");
                    nullCount++;
                }
            }
            
            if (nullCount > 0)
            {
                Debug.LogWarning($"[RoomPrefabManager] '{roomName}' has {nullCount} null markers out of {wallMarkers.Count}");
            }
            
            int validMarkerCount = wallMarkers.Count - nullCount;
            if (validMarkerCount < 10)
            {
                Debug.LogWarning($"[RoomPrefabManager] '{roomName}' has only {validMarkerCount} valid markers. Consider adding more for better gameplay variety.");
            }
            
            return validMarkerCount > 0;
        }
        
        /// <summary>
        /// Auto-number markers for easier debugging
        /// </summary>
        [ContextMenu("Auto-Number Markers")]
        public void AutoNumberMarkers()
        {
            if (wallMarkers == null) return;
            
            for (int i = 0; i < wallMarkers.Count; i++)
            {
                if (wallMarkers[i] != null)
                {
                    wallMarkers[i].markerId = i;
                }
            }
            
            Debug.Log($"[RoomPrefabManager] Auto-numbered {wallMarkers.Count} markers");
        }
        
        /// <summary>
        /// Collect all WallMarker components in children (helper for setup)
        /// </summary>
        [ContextMenu("Collect All Markers From Children")]
        public void CollectMarkersFromChildren()
        {
            wallMarkers.Clear();
            wallMarkers.AddRange(GetComponentsInChildren<WallMarker>());
            AutoNumberMarkers();
            
            Debug.Log($"[RoomPrefabManager] Collected {wallMarkers.Count} markers from children");
        }
        
        private void OnValidate()
        {
            // Auto-number markers when list changes
            if (wallMarkers != null && wallMarkers.Count > 0)
            {
                AutoNumberMarkers();
            }
        }
        
        private void OnDrawGizmos()
        {
            // Draw room bounds
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw lines to all markers
            if (wallMarkers == null) return;
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            foreach (var marker in wallMarkers)
            {
                if (marker != null)
                {
                    Gizmos.DrawLine(transform.position, marker.transform.position);
                }
            }
            
            // Draw info
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f, 
                $"{roomName}\n{wallMarkers.Count} markers"
            );
            #endif
        }
    }
}


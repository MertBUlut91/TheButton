using System.Collections.Generic;
using UnityEngine;

namespace TheButton.Game
{
    /// <summary>
    /// Pool of events that can be spawned in the procedural room
    /// Contains required events (must spawn) and random events (optional)
    /// </summary>
    [CreateAssetMenu(fileName = "RoomEventPool", menuName = "TheButton/Room Event Pool")]
    public class RoomEventPool : ScriptableObject
    {
        [Header("Required Events")]
        [Tooltip("Events that MUST spawn in every room (e.g., exit door)")]
        public List<EventData> requiredEvents = new List<EventData>();
        
        [Header("Random Event Pool")]
        [Tooltip("Events that can randomly spawn in the room")]
        public List<EventData> randomEventPool = new List<EventData>();
        
        [Header("Random Event Settings")]
        [Tooltip("Minimum number of random events to spawn")]
        [Range(0, 10)]
        public int minRandomEvents = 0;
        
        [Tooltip("Maximum number of random events to spawn")]
        [Range(0, 10)]
        public int maxRandomEvents = 3;
        
        /// <summary>
        /// Get a random event from the pool based on spawn weights
        /// </summary>
        public EventData GetRandomEvent()
        {
            if (randomEventPool == null || randomEventPool.Count == 0)
            {
                Debug.LogWarning("[RoomEventPool] Random event pool is empty!");
                return null;
            }
            
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var eventData in randomEventPool)
            {
                if (eventData != null)
                {
                    totalWeight += eventData.spawnWeight;
                }
            }
            
            if (totalWeight <= 0f)
            {
                // No weights, use uniform random
                int randomIndex = Random.Range(0, randomEventPool.Count);
                return randomEventPool[randomIndex];
            }
            
            // Weighted random selection
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var eventData in randomEventPool)
            {
                if (eventData != null)
                {
                    currentWeight += eventData.spawnWeight;
                    if (randomValue <= currentWeight)
                    {
                        return eventData;
                    }
                }
            }
            
            // Fallback (shouldn't reach here)
            return randomEventPool[randomEventPool.Count - 1];
        }
        
        /// <summary>
        /// Validate that all events are properly configured
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;
            
            // Validate required events
            if (requiredEvents != null)
            {
                foreach (var eventData in requiredEvents)
                {
                    if (eventData == null)
                    {
                        Debug.LogError("[RoomEventPool] Required events list contains null entry!");
                        isValid = false;
                    }
                    else if (!eventData.Validate())
                    {
                        isValid = false;
                    }
                }
            }
            
            // Validate random events
            if (randomEventPool != null)
            {
                foreach (var eventData in randomEventPool)
                {
                    if (eventData == null)
                    {
                        Debug.LogError("[RoomEventPool] Random event pool contains null entry!");
                        isValid = false;
                    }
                    else if (!eventData.Validate())
                    {
                        isValid = false;
                    }
                }
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Get total number of events that will spawn
        /// </summary>
        public int GetTotalEventCount()
        {
            int count = requiredEvents != null ? requiredEvents.Count : 0;
            count += Random.Range(minRandomEvents, maxRandomEvents + 1);
            return count;
        }
    }
}


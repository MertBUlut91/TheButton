using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace TheButton.Network
{
    public class RelayManager : MonoBehaviour
    {
        public static RelayManager Instance { get; private set; }

        private const int MaxConnections = 8;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Create a Relay allocation and return the join code
        /// </summary>
        public async Task<string> CreateRelayAsync()
        {
            try
            {
                // Create Relay allocation
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
                
                // Get join code
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                // Configure Unity Transport
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );
                
                Debug.Log($"[Relay] Created relay with join code: {joinCode}");
                return joinCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Relay] Failed to create relay: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Join a Relay using a join code
        /// </summary>
        public async Task JoinRelayAsync(string joinCode)
        {
            try
            {
                // Join allocation
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                
                // Configure Unity Transport
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetClientRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    allocation.HostConnectionData
                );
                
                Debug.Log($"[Relay] Joined relay with code: {joinCode}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Relay] Failed to join relay: {e.Message}");
                throw;
            }
        }
    }
}


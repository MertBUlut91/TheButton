using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace TheButton.Network
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }

        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
            Failed
        }

        public ConnectionState CurrentState { get; private set; } = ConnectionState.Disconnected;
        
        public event Action<ConnectionState> OnConnectionStateChanged;
        public event Action<string> OnConnectionError;

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
        /// Create a lobby and start hosting
        /// </summary>
        public async Task<bool> CreateAndHostLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate)
        {
            SetConnectionState(ConnectionState.Connecting);

            try
            {
                // Wait for authentication if needed
                if (!AuthenticationManager.IsSignedIn)
                {
                    await WaitForAuthenticationAsync();
                }

                // Create lobby
                var lobby = await LobbyManager.Instance.CreateLobbyAsync(lobbyName, maxPlayers, isPrivate);
                
                // Start host
                bool success = NetworkManagerSetup.Instance.StartHost();
                
                if (success)
                {
                    SetConnectionState(ConnectionState.Connected);
                    Debug.Log($"[Connection] Successfully created and hosted lobby: {lobbyName}");
                    return true;
                }
                else
                {
                    SetConnectionState(ConnectionState.Failed);
                    OnConnectionError?.Invoke("Failed to start host");
                    await LobbyManager.Instance.DeleteLobbyAsync();
                    return false;
                }
            }
            catch (Exception e)
            {
                SetConnectionState(ConnectionState.Failed);
                OnConnectionError?.Invoke($"Failed to create lobby: {e.Message}");
                Debug.LogError($"[Connection] Error creating lobby: {e}");
                return false;
            }
        }

        /// <summary>
        /// Join a lobby by code and connect as client
        /// </summary>
        public async Task<bool> JoinLobbyByCodeAsync(string lobbyCode)
        {
            SetConnectionState(ConnectionState.Connecting);

            try
            {
                // Wait for authentication if needed
                if (!AuthenticationManager.IsSignedIn)
                {
                    await WaitForAuthenticationAsync();
                }

                // Join lobby
                var lobby = await LobbyManager.Instance.JoinLobbyByCodeAsync(lobbyCode);
                
                // Start client
                bool success = NetworkManagerSetup.Instance.StartClient();
                
                if (success)
                {
                    SetConnectionState(ConnectionState.Connected);
                    Debug.Log($"[Connection] Successfully joined lobby: {lobby.Name}");
                    return true;
                }
                else
                {
                    SetConnectionState(ConnectionState.Failed);
                    OnConnectionError?.Invoke("Failed to connect as client");
                    await LobbyManager.Instance.LeaveLobbyAsync();
                    return false;
                }
            }
            catch (Exception e)
            {
                SetConnectionState(ConnectionState.Failed);
                OnConnectionError?.Invoke($"Failed to join lobby: {e.Message}");
                Debug.LogError($"[Connection] Error joining lobby: {e}");
                return false;
            }
        }

        /// <summary>
        /// Join a lobby by ID (for public lobbies)
        /// </summary>
        public async Task<bool> JoinLobbyByIdAsync(string lobbyId)
        {
            SetConnectionState(ConnectionState.Connecting);

            try
            {
                // Wait for authentication if needed
                if (!AuthenticationManager.IsSignedIn)
                {
                    await WaitForAuthenticationAsync();
                }

                // Join lobby
                var lobby = await LobbyManager.Instance.JoinLobbyByIdAsync(lobbyId);
                
                // Start client
                bool success = NetworkManagerSetup.Instance.StartClient();
                
                if (success)
                {
                    SetConnectionState(ConnectionState.Connected);
                    Debug.Log($"[Connection] Successfully joined lobby: {lobby.Name}");
                    return true;
                }
                else
                {
                    SetConnectionState(ConnectionState.Failed);
                    OnConnectionError?.Invoke("Failed to connect as client");
                    await LobbyManager.Instance.LeaveLobbyAsync();
                    return false;
                }
            }
            catch (Exception e)
            {
                SetConnectionState(ConnectionState.Failed);
                OnConnectionError?.Invoke($"Failed to join lobby: {e.Message}");
                Debug.LogError($"[Connection] Error joining lobby: {e}");
                return false;
            }
        }

        /// <summary>
        /// Disconnect from lobby and network
        /// </summary>
        public async Task DisconnectAsync()
        {
            NetworkManagerSetup.Instance.Disconnect();

            if (LobbyManager.Instance.IsHost)
            {
                await LobbyManager.Instance.DeleteLobbyAsync();
            }
            else
            {
                await LobbyManager.Instance.LeaveLobbyAsync();
            }

            SetConnectionState(ConnectionState.Disconnected);
        }

        private void SetConnectionState(ConnectionState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                OnConnectionStateChanged?.Invoke(newState);
                Debug.Log($"[Connection] State changed to: {newState}");
            }
        }

        private async Task WaitForAuthenticationAsync()
        {
            int maxWaitTime = 10; // seconds
            float elapsed = 0;
            
            while (!AuthenticationManager.IsSignedIn && elapsed < maxWaitTime)
            {
                await Task.Delay(100);
                elapsed += 0.1f;
            }

            if (!AuthenticationManager.IsSignedIn)
            {
                throw new Exception("Authentication timeout");
            }
        }
    }
}


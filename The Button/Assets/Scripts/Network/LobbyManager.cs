using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace TheButton.Network
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        public Lobby CurrentLobby { get; private set; }
        public bool IsHost => CurrentLobby != null && CurrentLobby.HostId == AuthenticationManager.PlayerId;

        private const string KEY_RELAY_CODE = "RelayCode";
        private const float LOBBY_HEARTBEAT_INTERVAL = 15f;
        private const float LOBBY_POLL_INTERVAL = 2f;

        private float heartbeatTimer;
        private float pollTimer;

        public event Action<Lobby> OnLobbyUpdated;
        public event Action OnLobbyLeft;

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

        private void Update()
        {
            HandleLobbyHeartbeat();
            HandleLobbyPolling();
        }

        /// <summary>
        /// Create a new lobby
        /// </summary>
        public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate)
        {
            try
            {
                // Create lobby options
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Data = new Dictionary<string, DataObject>()
                };

                // Create lobby
                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                
                // Get lobby code (Unity generates this automatically)
                string lobbyCode = CurrentLobby.LobbyCode;
                Debug.Log($"[Lobby] Created lobby: {lobbyName} ({CurrentLobby.Id}) - Code: {lobbyCode}");

                // Create Relay
                string relayCode = await RelayManager.Instance.CreateRelayAsync();
                
                // Update lobby with relay code
                await UpdateLobbyRelayCodeAsync(relayCode);

                OnLobbyUpdated?.Invoke(CurrentLobby);
                return CurrentLobby;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Failed to create lobby: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Join a lobby by its code (uses Unity's built-in lobby code system)
        /// </summary>
        public async Task<Lobby> JoinLobbyByCodeAsync(string lobbyCode)
        {
            try
            {
                // Use Unity's built-in join by code
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
                
                Debug.Log($"[Lobby] Joined lobby: {CurrentLobby.Name} ({CurrentLobby.Id})");

                // Get relay code and join
                string relayCode = CurrentLobby.Data[KEY_RELAY_CODE].Value;
                await RelayManager.Instance.JoinRelayAsync(relayCode);

                OnLobbyUpdated?.Invoke(CurrentLobby);
                return CurrentLobby;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Failed to join lobby: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Join a lobby by its ID (for public lobbies)
        /// </summary>
        public async Task<Lobby> JoinLobbyByIdAsync(string lobbyId)
        {
            try
            {
                CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
                Debug.Log($"[Lobby] Joined lobby: {CurrentLobby.Name} ({CurrentLobby.Id})");

                // Get relay code and join
                string relayCode = CurrentLobby.Data[KEY_RELAY_CODE].Value;
                await RelayManager.Instance.JoinRelayAsync(relayCode);

                OnLobbyUpdated?.Invoke(CurrentLobby);
                return CurrentLobby;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Failed to join lobby by ID: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Query available public lobbies
        /// </summary>
        public async Task<List<Lobby>> QueryPublicLobbiesAsync()
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Count = 25,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    }
                };

                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
                Debug.Log($"[Lobby] Found {response.Results.Count} public lobbies");
                return response.Results;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Failed to query lobbies: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Leave the current lobby
        /// </summary>
        public async Task LeaveLobbyAsync()
        {
            if (CurrentLobby == null) return;

            try
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationManager.PlayerId);
                Debug.Log($"[Lobby] Left lobby: {CurrentLobby.Name}");
                CurrentLobby = null;
                OnLobbyLeft?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Failed to leave lobby: {e.Message}");
            }
        }

        /// <summary>
        /// Delete the current lobby (host only)
        /// </summary>
        public async Task DeleteLobbyAsync()
        {
            if (CurrentLobby == null || !IsHost) return;

            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
                Debug.Log($"[Lobby] Deleted lobby: {CurrentLobby.Name}");
                CurrentLobby = null;
                OnLobbyLeft?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Failed to delete lobby: {e.Message}");
            }
        }

        public string GetLobbyCode()
        {
            if (CurrentLobby == null) return null;
            return CurrentLobby.LobbyCode;
        }

        private async Task UpdateLobbyRelayCodeAsync(string relayCode)
        {
            try
            {
                UpdateLobbyOptions options = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                };

                CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);
                Debug.Log($"[Lobby] Updated lobby with relay code");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Failed to update lobby relay code: {e.Message}");
            }
        }

        private void HandleLobbyHeartbeat()
        {
            if (CurrentLobby == null || !IsHost) return;

            heartbeatTimer += Time.deltaTime;
            if (heartbeatTimer >= LOBBY_HEARTBEAT_INTERVAL)
            {
                heartbeatTimer = 0;
                _ = SendHeartbeatAsync();
            }
        }

        private async Task SendHeartbeatAsync()
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Heartbeat failed: {e.Message}");
            }
        }

        private void HandleLobbyPolling()
        {
            if (CurrentLobby == null) return;

            pollTimer += Time.deltaTime;
            if (pollTimer >= LOBBY_POLL_INTERVAL)
            {
                pollTimer = 0;
                _ = PollLobbyAsync();
            }
        }

        private async Task PollLobbyAsync()
        {
            try
            {
                CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
                OnLobbyUpdated?.Invoke(CurrentLobby);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Poll failed: {e.Message}");
            }
        }


        private void OnApplicationQuit()
        {
            if (CurrentLobby != null)
            {
                if (IsHost)
                    _ = DeleteLobbyAsync();
                else
                    _ = LeaveLobbyAsync();
            }
        }
    }
}


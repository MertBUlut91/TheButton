using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheButton.Network
{
    public class NetworkManagerSetup : MonoBehaviour
    {
        public static NetworkManagerSetup Instance { get; private set; }

        [SerializeField] private string gameSceneName = "GameRoom";

        public bool IsConnected => NetworkManager.Singleton != null && 
                                    (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);

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

        private void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                // Disable automatic player spawning
                NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                NetworkManager.Singleton.OnServerStarted += OnServerStarted;
                
                // Subscribe to scene events to prevent initial scene sync
                if (NetworkManager.Singleton.SceneManager != null)
                {
                    NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading += OnVerifySceneBeforeLoading;
                }
            }
        }

        private bool OnVerifySceneBeforeLoading(int sceneIndex, string sceneName, LoadSceneMode loadSceneMode)
        {
            // Allow scene loading only if it's the GameRoom scene
            // This prevents automatic MainMenu synchronization when client joins
            bool isGameRoom = sceneName == gameSceneName || sceneIndex == 1;
            
            if (!isGameRoom)
            {
                Debug.Log($"[Network] Blocked automatic scene sync for: {sceneName} (index {sceneIndex})");
                return false; // Block the scene load
            }
            
            Debug.Log($"[Network] Allowing scene load: {sceneName}");
            return true; // Allow the scene load
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // Approve connection but don't spawn player yet
            response.Approved = true;
            response.CreatePlayerObject = false; // Prevent automatic player spawning
            response.Pending = false;
        }

        public bool StartHost()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[Network] NetworkManager not found!");
                return false;
            }

            bool success = NetworkManager.Singleton.StartHost();
            if (success)
            {
                Debug.Log("[Network] Started as Host");
            }
            return success;
        }

        public bool StartClient()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[Network] NetworkManager not found!");
                return false;
            }

            bool success = NetworkManager.Singleton.StartClient();
            if (success)
            {
                Debug.Log("[Network] Started as Client");
            }
            return success;
        }

        public void Disconnect()
        {
            if (NetworkManager.Singleton == null) return;

            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown();
                Debug.Log("[Network] Host shutdown");
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
                Debug.Log("[Network] Client disconnected");
            }
        }

        public void LoadGameScene()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("[Network] Only the host can load scenes");
                return;
            }

            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnGameSceneLoaded;
            Debug.Log($"[Network] Loading game scene: {gameSceneName}");
        }

        private void OnGameSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (sceneName != gameSceneName) return;

            // Unsubscribe
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnGameSceneLoaded;

            // Spawn players for all connected clients
            if (NetworkManager.Singleton.IsServer)
            {
                foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    SpawnPlayer(clientId);
                }
                Debug.Log($"[Network] Spawned {NetworkManager.Singleton.ConnectedClientsIds.Count} players");
            }
        }

        private void SpawnPlayer(ulong clientId)
        {
            // Get spawn position
            Vector3 spawnPosition = GetSpawnPosition();
            
            // Spawn player
            var playerObject = Instantiate(NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[0].Prefab, spawnPosition, Quaternion.identity);
            var networkObject = playerObject.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId, true);
            
            Debug.Log($"[Network] Spawned player for client {clientId}");
        }

        private Vector3 GetSpawnPosition()
        {
            // Try to find spawn points
            var spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
            if (spawnPoints.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
                return spawnPoints[randomIndex].transform.position;
            }

            // Default spawn position
            return new Vector3(0, 1, 0);
        }

        private void OnServerStarted()
        {
            Debug.Log("[Network] Server started successfully");
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[Network] Client connected: {clientId}");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[Network] Client disconnected: {clientId}");
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                
                if (NetworkManager.Singleton.SceneManager != null)
                {
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnGameSceneLoaded;
                    NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading -= OnVerifySceneBeforeLoading;
                }
            }
        }
    }
}


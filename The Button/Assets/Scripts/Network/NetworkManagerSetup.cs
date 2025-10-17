using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TheButton.Game;
using Scene = UnityEngine.SceneManagement.Scene;

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

            // Server: Generate room first, then spawn players
            if (NetworkManager.Singleton.IsServer)
            {
                StartCoroutine(GenerateRoomAndSpawnPlayers());
            }
        }
        
        /// <summary>
        /// Generate procedural room, wait for completion, then spawn players
        /// </summary>
        private IEnumerator GenerateRoomAndSpawnPlayers()
        {
            Debug.Log("[Network] Starting room generation...");
            
            // Find or create ProceduralRoomGenerator
            ProceduralRoomGenerator roomGenerator = FindObjectOfType<ProceduralRoomGenerator>();
            
            if (roomGenerator == null)
            {
                Debug.LogWarning("[Network] ProceduralRoomGenerator not found in scene, spawning one...");
                GameObject generatorObj = new GameObject("ProceduralRoomGenerator");
                roomGenerator = generatorObj.AddComponent<ProceduralRoomGenerator>();
                
                // Get NetworkObject and spawn it
                NetworkObject netObj = generatorObj.AddComponent<NetworkObject>();
                netObj.Spawn(true);
            }
            
            // Wait for room generator to be ready
            yield return new WaitUntil(() => roomGenerator.IsSpawned);
            
            // Start room generation
            bool roomReady = false;
            roomGenerator.OnRoomGenerationComplete += () => roomReady = true;
            roomGenerator.GenerateRoom();
            
            // Wait for room generation to complete
            yield return new WaitUntil(() => roomReady);
            
            Debug.Log("[Network] Room generation complete, spawning players...");
            
            // Calculate base spawn position - room center at floor level
            Vector3 baseSpawnPos = roomGenerator.GetRoomCenter();
            Debug.Log($"[Network] Base spawn position (room center): {baseSpawnPos}");
            
            // Spawn players in a circle pattern to avoid collision
            int playerCount = 0;
            int totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
            float spawnRadius = 2f; // Radius of spawn circle (adjust based on player size)
            
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                // Calculate angle for this player (evenly distributed around circle)
                float angle = (360f / totalPlayers) * playerCount * Mathf.Deg2Rad;
                
                // Calculate offset from center
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * spawnRadius,
                    0f,
                    Mathf.Sin(angle) * spawnRadius
                );
                
                Vector3 playerSpawnPos = baseSpawnPos + offset;
                
                Debug.Log($"[Network] Spawning player {playerCount} (clientId: {clientId}) at {playerSpawnPos} (angle: {angle * Mathf.Rad2Deg}°, offset: {offset})");
                SpawnPlayer(clientId, playerSpawnPos);
                playerCount++;
            }
            
            Debug.Log($"[Network] Successfully spawned {playerCount} players in circle formation (radius: {spawnRadius})");
        }

        private void SpawnPlayer(ulong clientId, Vector3? customSpawnPosition = null)
        {
            // Get spawn position
            Vector3 spawnPosition = customSpawnPosition ?? GetSpawnPosition();
            
            Debug.Log($"[Network] SpawnPlayer: Final position for client {clientId}: {spawnPosition}");
            
            // Spawn player
            var playerObject = Instantiate(NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[0].Prefab, spawnPosition, Quaternion.identity);
            var networkObject = playerObject.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId, true);
            
            Debug.Log($"[Network] ✅ Successfully spawned player for client {clientId} at {spawnPosition}");
        }

        private Vector3 GetSpawnPosition()
        {
            // Try to use room generator spawn position with random offset
            ProceduralRoomGenerator roomGenerator = FindObjectOfType<ProceduralRoomGenerator>();
            if (roomGenerator != null && roomGenerator.IsRoomReady())
            {
                Vector3 baseSpawnPos = roomGenerator.GetRoomCenter();
                
                // Add random offset to avoid collision
                float spawnRadius = 2f;
                float randomAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float randomRadius = UnityEngine.Random.Range(0f, spawnRadius);
                
                Vector3 offset = new Vector3(
                    Mathf.Cos(randomAngle) * randomRadius,
                    0f,
                    Mathf.Sin(randomAngle) * randomRadius
                );
                
                return baseSpawnPos + offset;
            }
            
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
            
            // If server and we're in GameRoom scene, spawn player for the newly connected client
            if (NetworkManager.Singleton.IsServer)
            {
                Scene currentScene = SceneManager.GetActiveScene();
                if (currentScene.name == gameSceneName)
                {
                    // Room already generated, spawn player immediately
                    ProceduralRoomGenerator roomGenerator = FindObjectOfType<ProceduralRoomGenerator>();
                    if (roomGenerator != null && roomGenerator.IsRoomReady())
                    {
                        // Calculate spawn position - add random offset to avoid collision
                        Vector3 baseSpawnPos = roomGenerator.GetRoomCenter();
                        
                        // Random offset within spawn radius
                        float spawnRadius = 2f;
                        float randomAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                        float randomRadius = UnityEngine.Random.Range(0f, spawnRadius);
                        
                        Vector3 offset = new Vector3(
                            Mathf.Cos(randomAngle) * randomRadius,
                            0f,
                            Mathf.Sin(randomAngle) * randomRadius
                        );
                        
                        Vector3 spawnPos = baseSpawnPos + offset;
                        
                        Debug.Log($"[Network] Late join: Spawning player for client {clientId} at {spawnPos} (base: {baseSpawnPos}, offset: {offset})");
                        SpawnPlayer(clientId, spawnPos);
                    }
                    else
                    {
                        Debug.LogWarning($"[Network] Room not ready yet for client {clientId}");
                    }
                }
            }
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


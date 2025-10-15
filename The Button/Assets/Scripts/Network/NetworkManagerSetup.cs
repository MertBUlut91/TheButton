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
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            }
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
            Debug.Log($"[Network] Loading game scene: {gameSceneName}");
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
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            }
        }
    }
}


using Unity.Netcode;
using UnityEngine;

namespace TheButton.Game
{
    /// <summary>
    /// Manages game state, win/lose conditions, and game flow
    /// Server-authoritative
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game Settings")]
        [Tooltip("Time limit for the game in seconds (0 = no limit)")]
        [SerializeField] private float gameTimeLimit = 600f; // 10 minutes
        
        private NetworkVariable<GameState> currentGameState = new NetworkVariable<GameState>(
            GameState.Playing,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private NetworkVariable<ulong> winnerClientId = new NetworkVariable<ulong>(
            ulong.MaxValue,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private float gameStartTime;
        
        public GameState CurrentGameState => currentGameState.Value;
        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action<ulong> OnPlayerWon;
        public event System.Action<ulong> OnPlayerDied;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                gameStartTime = Time.time;
                currentGameState.Value = GameState.Playing;
                Debug.Log("[GameManager] Game started");
            }
            
            currentGameState.OnValueChanged += OnGameStateValueChanged;
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            currentGameState.OnValueChanged -= OnGameStateValueChanged;
        }
        
        private void Update()
        {
            if (!IsServer) return;
            if (currentGameState.Value != GameState.Playing) return;
            
            // Check time limit
            if (gameTimeLimit > 0)
            {
                float elapsedTime = Time.time - gameStartTime;
                if (elapsedTime >= gameTimeLimit)
                {
                    // Time's up - all players lose
                    Debug.Log("[GameManager] Time limit reached - Game Over");
                    currentGameState.Value = GameState.Lost;
                }
            }
        }
        
        /// <summary>
        /// Called when a player wins the game (Server only)
        /// </summary>
        public void PlayerWon(ulong clientId)
        {
            if (!IsServer) return;
            if (currentGameState.Value != GameState.Playing) return;
            
            Debug.Log($"[GameManager] Player {clientId} won the game!");
            
            winnerClientId.Value = clientId;
            currentGameState.Value = GameState.Won;
            
            OnPlayerWon?.Invoke(clientId);
        }
        
        /// <summary>
        /// Called when a player dies (Server only)
        /// </summary>
        public void PlayerDied(ulong clientId)
        {
            if (!IsServer) return;
            
            Debug.Log($"[GameManager] Player {clientId} died");
            
            OnPlayerDied?.Invoke(clientId);
            
            // Check if all players are dead
            CheckAllPlayersDead();
        }
        
        private void CheckAllPlayersDead()
        {
            // Count alive players
            var playerNetworks = FindObjectsOfType<Player.PlayerNetwork>();
            int alivePlayers = 0;
            
            foreach (var player in playerNetworks)
            {
                if (player.Health.Value > 0)
                {
                    alivePlayers++;
                }
            }
            
            if (alivePlayers == 0)
            {
                Debug.Log("[GameManager] All players died - Game Over");
                currentGameState.Value = GameState.Lost;
            }
        }
        
        /// <summary>
        /// Restart the game (Server only)
        /// Generates a new room and resets all players
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RestartGameServerRpc()
        {
            if (!IsServer) return;
            
            Debug.Log("[GameManager] Restarting game...");
            
            // Reset game state
            currentGameState.Value = GameState.Playing;
            winnerClientId.Value = ulong.MaxValue;
            gameStartTime = Time.time;
            
            // Clear old room
            ProceduralRoomGenerator roomGenerator = FindObjectOfType<ProceduralRoomGenerator>();
            if (roomGenerator != null)
            {
                roomGenerator.ClearRoom();
                
                // Generate new room
                Debug.Log("[GameManager] Generating new room for restart...");
                roomGenerator.OnRoomGenerationComplete += OnNewRoomGenerated;
                roomGenerator.GenerateRoom();
            }
            
            // Reset all players
            var playerNetworks = FindObjectsOfType<Player.PlayerNetwork>();
            foreach (var player in playerNetworks)
            {
                player.ResetStats();
            }
            
            // Clear all inventories
            var playerInventories = FindObjectsOfType<Player.PlayerInventory>();
            foreach (var inventory in playerInventories)
            {
                inventory.ClearInventoryServerRpc();
            }
            
            // Despawn all world items
            var worldItems = FindObjectsOfType<Items.WorldItem>();
            foreach (var item in worldItems)
            {
                if (item.GetComponent<NetworkObject>().IsSpawned)
                {
                    item.GetComponent<NetworkObject>().Despawn(true);
                }
            }
            
            Debug.Log("[GameManager] Game restarted");
        }
        
        private void OnNewRoomGenerated()
        {
            Debug.Log("[GameManager] New room generated, repositioning players...");
            
            ProceduralRoomGenerator roomGenerator = FindObjectOfType<ProceduralRoomGenerator>();
            if (roomGenerator != null)
            {
                roomGenerator.OnRoomGenerationComplete -= OnNewRoomGenerated;
                
                // Move all players to new room center
                Vector3 roomCenter = roomGenerator.GetRoomCenter();
                var playerNetworks = FindObjectsOfType<Player.PlayerNetwork>();
                foreach (var player in playerNetworks)
                {
                    player.transform.position = roomCenter;
                }
            }
        }
        
        /// <summary>
        /// Return to lobby (Server only)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ReturnToLobbyServerRpc()
        {
            if (!IsServer) return;
            
            Debug.Log("[GameManager] Returning to lobby...");
            
            // Load main menu scene
            var networkManager = NetworkManager.Singleton;
            if (networkManager != null)
            {
                networkManager.SceneManager.LoadScene("MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
        
        private void OnGameStateValueChanged(GameState oldState, GameState newState)
        {
            Debug.Log($"[GameManager] Game state changed: {oldState} -> {newState}");
            OnGameStateChanged?.Invoke(newState);
        }
        
        public float GetRemainingTime()
        {
            if (gameTimeLimit <= 0) return -1f;
            
            float elapsedTime = Time.time - gameStartTime;
            return Mathf.Max(0f, gameTimeLimit - elapsedTime);
        }
        
        public ulong GetWinnerClientId()
        {
            return winnerClientId.Value;
        }
    }
    
    /// <summary>
    /// Possible game states
    /// </summary>
    public enum GameState
    {
        Playing,
        Won,
        Lost
    }
}


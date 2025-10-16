using Unity.Netcode;
using UnityEngine;
using TMPro;
using TheButton.Network;

namespace TheButton.Player
{
    public class PlayerNetwork : NetworkBehaviour
    {
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private Transform nameTagTransform;

        [Header("Stats - Synchronized")]
        public NetworkVariable<float> Health = new NetworkVariable<float>(100f);
        public NetworkVariable<float> Hunger = new NetworkVariable<float>(100f);
        public NetworkVariable<float> Thirst = new NetworkVariable<float>(100f);
        public NetworkVariable<float> Stamina = new NetworkVariable<float>(100f);

        [Header("Stats Settings")]
        [SerializeField] private float hungerDecayRate = 1f; // per minute
        [SerializeField] private float thirstDecayRate = 1.5f; // per minute
        [SerializeField] private float staminaRegenRate = 20f; // per second

        private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>(
            new NetworkString(""), // Initialize with empty string to prevent null serialization
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                // Set player name (use player ID for now)
                SetPlayerNameServerRpc($"Player_{OwnerClientId}");
            }

            // Subscribe to player name changes
            playerName.OnValueChanged += OnPlayerNameChanged;
            UpdatePlayerNameDisplay();

            // Make nametag face camera
            if (nameTagTransform != null)
            {
                // This will be handled in LateUpdate
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            playerName.OnValueChanged -= OnPlayerNameChanged;
        }

        private void Update()
        {
            if (IsServer)
            {
                UpdateStatsServer();
            }
        }

        private void LateUpdate()
        {
            // Make nametag face the camera
            if (nameTagTransform != null && Camera.main != null)
            {
                nameTagTransform.LookAt(Camera.main.transform);
                nameTagTransform.Rotate(0, 180, 0); // Flip to face correctly
            }
        }

        private void UpdateStatsServer()
        {
            float deltaTime = Time.deltaTime;

            // Decay hunger and thirst over time
            Hunger.Value = Mathf.Max(0, Hunger.Value - (hungerDecayRate / 60f) * deltaTime);
            Thirst.Value = Mathf.Max(0, Thirst.Value - (thirstDecayRate / 60f) * deltaTime);

            // Regenerate stamina when not sprinting
            if (Stamina.Value < 100f)
            {
                Stamina.Value = Mathf.Min(100f, Stamina.Value + staminaRegenRate * deltaTime);
            }

            // Health damage from hunger/thirst
            if (Hunger.Value <= 0 || Thirst.Value <= 0)
            {
                Health.Value = Mathf.Max(0, Health.Value - 5f * deltaTime); // 5 damage per second
            }
            
            // Check if player died
            if (Health.Value <= 0)
            {
                OnPlayerDeath();
            }
        }
        
        private void OnPlayerDeath()
        {
            Debug.Log($"[PlayerNetwork] Player {OwnerClientId} died");
            
            // Notify game manager
            var gameManager = FindObjectOfType<Game.GameManager>();
            if (gameManager != null)
            {
                gameManager.PlayerDied(OwnerClientId);
            }
        }

        [ServerRpc]
        private void SetPlayerNameServerRpc(string name)
        {
            playerName.Value = name;
        }

        private void OnPlayerNameChanged(NetworkString oldValue, NetworkString newValue)
        {
            UpdatePlayerNameDisplay();
        }

        private void UpdatePlayerNameDisplay()
        {
            if (playerNameText != null)
            {
                playerNameText.text = playerName.Value.ToString();
            }
        }

        // Methods to modify stats (called from items/buttons later)
        [ServerRpc(RequireOwnership = false)]
        public void ModifyHealthServerRpc(float amount)
        {
            Health.Value = Mathf.Clamp(Health.Value + amount, 0, 100);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ModifyHungerServerRpc(float amount)
        {
            Hunger.Value = Mathf.Clamp(Hunger.Value + amount, 0, 100);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ModifyThirstServerRpc(float amount)
        {
            Thirst.Value = Mathf.Clamp(Thirst.Value + amount, 0, 100);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ModifyStaminaServerRpc(float amount)
        {
            Stamina.Value = Mathf.Clamp(Stamina.Value + amount, 0, 100);
        }
        
        /// <summary>
        /// Reset all stats to default values (Server only)
        /// </summary>
        public void ResetStats()
        {
            if (!IsServer) return;
            
            Health.Value = 100f;
            Hunger.Value = 100f;
            Thirst.Value = 100f;
            Stamina.Value = 100f;
            
            Debug.Log($"[PlayerNetwork] Reset stats for Player {OwnerClientId}");
        }
    }
}


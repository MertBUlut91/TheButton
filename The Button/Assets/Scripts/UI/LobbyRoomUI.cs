using System.Collections.Generic;
using System.Linq;
using TheButton.Network;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheButton.UI
{
    public class LobbyRoomUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI lobbyNameText;
        [SerializeField] private TextMeshProUGUI lobbyCodeText;
        [SerializeField] private Button copyCodeButton;
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerItemPrefab;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button leaveLobbyButton;

        [Header("References")]
        [SerializeField] private MainMenuUI mainMenuUI;

        private List<GameObject> playerItems = new List<GameObject>();

        private void Start()
        {
            copyCodeButton.onClick.AddListener(OnCopyCodeClick);
            startGameButton.onClick.AddListener(OnStartGameClick);
            leaveLobbyButton.onClick.AddListener(OnLeaveLobbyClick);

            // Subscribe to lobby updates
            LobbyManager.Instance.OnLobbyUpdated += OnLobbyUpdated;
            LobbyManager.Instance.OnLobbyLeft += OnLobbyLeft;
        }

        private void OnEnable()
        {
            UpdateLobbyInfo();
        }

        private void OnDestroy()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnLobbyUpdated -= OnLobbyUpdated;
                LobbyManager.Instance.OnLobbyLeft -= OnLobbyLeft;
            }
        }

        private void UpdateLobbyInfo()
        {
            var lobby = LobbyManager.Instance.CurrentLobby;
            if (lobby == null) return;

            // Update lobby name
            if (lobbyNameText != null)
                lobbyNameText.text = lobby.Name;

            // Update lobby code
            string code = LobbyManager.Instance.GetLobbyCode();
            if (lobbyCodeText != null)
                lobbyCodeText.text = $"Code: {code}";

            // Update player list
            UpdatePlayerList(lobby.Players);

            // Update start button visibility (only host can start)
            if (startGameButton != null)
                startGameButton.gameObject.SetActive(LobbyManager.Instance.IsHost);
        }

        private void UpdatePlayerList(List<Unity.Services.Lobbies.Models.Player> players)
        {
            // Clear existing items
            foreach (var item in playerItems)
            {
                Destroy(item);
            }
            playerItems.Clear();

            // Create new items
            foreach (var player in players)
            {
                GameObject item = Instantiate(playerItemPrefab, playerListContainer);
                playerItems.Add(item);

                var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    string playerName = $"Player {player.Id.Substring(0, 8)}";
                    bool isHost = player.Id == LobbyManager.Instance.CurrentLobby.HostId;
                    nameText.text = isHost ? $"{playerName} (Host)" : playerName;
                }
            }
        }

        private void OnLobbyUpdated(Lobby lobby)
        {
            UpdateLobbyInfo();
        }

        private void OnLobbyLeft()
        {
            mainMenuUI.ShowMainPanel();
        }

        private void OnCopyCodeClick()
        {
            string code = LobbyManager.Instance.GetLobbyCode();
            if (!string.IsNullOrEmpty(code))
            {
                GUIUtility.systemCopyBuffer = code;
                Debug.Log($"[LobbyRoom] Copied code to clipboard: {code}");
            }
        }

        private void OnStartGameClick()
        {
            if (!LobbyManager.Instance.IsHost)
            {
                Debug.LogWarning("[LobbyRoom] Only the host can start the game");
                return;
            }

            Debug.Log("[LobbyRoom] Starting game...");
            NetworkManagerSetup.Instance.LoadGameScene();
        }

        private async void OnLeaveLobbyClick()
        {
            await ConnectionManager.Instance.DisconnectAsync();
            mainMenuUI.ShowMainPanel();
        }
    }
}


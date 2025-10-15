using System.Collections.Generic;
using TheButton.Network;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheButton.UI
{
    public class LobbyBrowserUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform lobbyListContainer;
        [SerializeField] private GameObject lobbyItemPrefab;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("References")]
        [SerializeField] private MainMenuUI mainMenuUI;

        private List<GameObject> lobbyItems = new List<GameObject>();

        private void Start()
        {
            refreshButton.onClick.AddListener(RefreshLobbies);
            backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void OnEnable()
        {
            RefreshLobbies();
        }

        public async void RefreshLobbies()
        {
            refreshButton.interactable = false;
            ClearLobbyList();
            UpdateStatus("Loading lobbies...");

            try
            {
                List<Lobby> lobbies = await LobbyManager.Instance.QueryPublicLobbiesAsync();

                if (lobbies.Count == 0)
                {
                    UpdateStatus("No public lobbies found. Create one!");
                }
                else
                {
                    UpdateStatus($"Found {lobbies.Count} lobbies");
                    DisplayLobbies(lobbies);
                }
            }
            catch (System.Exception e)
            {
                UpdateStatus($"Error loading lobbies: {e.Message}");
                Debug.LogError($"[LobbyBrowser] Failed to load lobbies: {e}");
            }
            finally
            {
                refreshButton.interactable = true;
            }
        }

        private void DisplayLobbies(List<Lobby> lobbies)
        {
            foreach (var lobby in lobbies)
            {
                GameObject item = Instantiate(lobbyItemPrefab, lobbyListContainer);
                lobbyItems.Add(item);

                // Setup lobby item
                var nameText = item.transform.Find("LobbyName")?.GetComponent<TextMeshProUGUI>();
                var playersText = item.transform.Find("PlayersCount")?.GetComponent<TextMeshProUGUI>();
                var joinButton = item.transform.Find("JoinButton")?.GetComponent<Button>();

                if (nameText != null)
                    nameText.text = lobby.Name;

                if (playersText != null)
                    playersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

                if (joinButton != null)
                {
                    string lobbyId = lobby.Id;
                    joinButton.onClick.AddListener(() => OnJoinLobbyClick(lobbyId));
                }
            }
        }

        private async void OnJoinLobbyClick(string lobbyId)
        {
            mainMenuUI.ShowLoadingPanel("Joining lobby...");

            bool success = await ConnectionManager.Instance.JoinLobbyByIdAsync(lobbyId);

            mainMenuUI.HideLoadingPanel();

            if (success)
            {
                mainMenuUI.ShowLobbyRoomPanel();
            }
            else
            {
                UpdateStatus("Failed to join lobby");
            }
        }

        private void ClearLobbyList()
        {
            foreach (var item in lobbyItems)
            {
                Destroy(item);
            }
            lobbyItems.Clear();
        }

        private void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void OnBackButtonClick()
        {
            mainMenuUI.ShowMainPanel();
        }
    }
}


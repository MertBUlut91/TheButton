using TheButton.Network;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheButton.UI
{
    public class LobbyCreationUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_InputField lobbyNameInput;
        [SerializeField] private Slider maxPlayersSlider;
        [SerializeField] private TextMeshProUGUI maxPlayersText;
        [SerializeField] private Toggle privateToggle;
        [SerializeField] private Button createButton;
        [SerializeField] private Button backButton;

        [Header("References")]
        [SerializeField] private MainMenuUI mainMenuUI;

        private void Start()
        {
            // Setup default values
            lobbyNameInput.text = $"Lobby_{Random.Range(1000, 9999)}";
            maxPlayersSlider.value = 4;
            UpdateMaxPlayersText(4);

            // Setup listeners
            maxPlayersSlider.onValueChanged.AddListener(UpdateMaxPlayersText);
            createButton.onClick.AddListener(OnCreateButtonClick);
            backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void UpdateMaxPlayersText(float value)
        {
            maxPlayersText.text = $"Max Players: {(int)value}";
        }

        private async void OnCreateButtonClick()
        {
            string lobbyName = lobbyNameInput.text.Trim();
            if (string.IsNullOrEmpty(lobbyName))
            {
                Debug.LogWarning("[LobbyCreation] Lobby name is empty");
                return;
            }

            int maxPlayers = (int)maxPlayersSlider.value;
            bool isPrivate = privateToggle.isOn;

            createButton.interactable = false;
            mainMenuUI.ShowLoadingPanel("Creating lobby...");

            bool success = await ConnectionManager.Instance.CreateAndHostLobbyAsync(lobbyName, maxPlayers, isPrivate);

            mainMenuUI.HideLoadingPanel();

            if (success)
            {
                mainMenuUI.ShowLobbyRoomPanel();
            }
            else
            {
                createButton.interactable = true;
                Debug.LogError("[LobbyCreation] Failed to create lobby");
            }
        }

        private void OnBackButtonClick()
        {
            mainMenuUI.ShowMainPanel();
        }
    }
}


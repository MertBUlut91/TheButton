using TheButton.Network;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheButton.UI
{
    public class JoinByCodeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("References")]
        [SerializeField] private MainMenuUI mainMenuUI;

        private void Start()
        {
            joinButton.onClick.AddListener(OnJoinButtonClick);
            backButton.onClick.AddListener(OnBackButtonClick);
            
            if (errorText != null)
                errorText.gameObject.SetActive(false);

            // Auto-uppercase the code input
            codeInput.onValueChanged.AddListener((text) => 
            {
                codeInput.text = text.ToUpper();
            });
        }

        private async void OnJoinButtonClick()
        {
            string code = codeInput.text.Trim().ToUpper();
            
            if (string.IsNullOrEmpty(code) || code.Length != 6)
            {
                ShowError("Please enter a valid 6-character code");
                return;
            }

            joinButton.interactable = false;
            HideError();
            mainMenuUI.ShowLoadingPanel("Joining lobby...");

            bool success = await ConnectionManager.Instance.JoinLobbyByCodeAsync(code);

            mainMenuUI.HideLoadingPanel();

            if (success)
            {
                mainMenuUI.ShowLobbyRoomPanel();
            }
            else
            {
                joinButton.interactable = true;
                ShowError("Failed to join lobby. Check the code and try again.");
            }
        }

        private void OnBackButtonClick()
        {
            mainMenuUI.ShowMainPanel();
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
        }

        private void HideError()
        {
            if (errorText != null)
                errorText.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            codeInput.text = "";
            HideError();
            joinButton.interactable = true;
        }
    }
}


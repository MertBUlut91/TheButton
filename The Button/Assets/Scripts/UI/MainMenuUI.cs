using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheButton.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject createLobbyPanel;
        [SerializeField] private GameObject joinByCodePanel;
        [SerializeField] private GameObject lobbyBrowserPanel;
        [SerializeField] private GameObject lobbyRoomPanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button joinByCodeButton;
        [SerializeField] private Button browseLobbyButton;
        [SerializeField] private Button quitButton;

        [Header("Loading")]
        [SerializeField] private TextMeshProUGUI loadingText;

        private void Start()
        {
            // Setup button listeners
            createLobbyButton.onClick.AddListener(OnCreateLobbyClick);
            joinByCodeButton.onClick.AddListener(OnJoinByCodeClick);
            browseLobbyButton.onClick.AddListener(OnBrowseLobbyClick);
            quitButton.onClick.AddListener(OnQuitClick);

            // Show main panel
            ShowMainPanel();
        }

        public void ShowMainPanel()
        {
            HideAllPanels();
            mainPanel.SetActive(true);
        }

        public void ShowCreateLobbyPanel()
        {
            HideAllPanels();
            createLobbyPanel.SetActive(true);
        }

        public void ShowJoinByCodePanel()
        {
            HideAllPanels();
            joinByCodePanel.SetActive(true);
        }

        public void ShowLobbyBrowserPanel()
        {
            HideAllPanels();
            lobbyBrowserPanel.SetActive(true);
        }

        public void ShowLobbyRoomPanel()
        {
            HideAllPanels();
            lobbyRoomPanel.SetActive(true);
        }

        public void ShowLoadingPanel(string message)
        {
            loadingPanel.SetActive(true);
            if (loadingText != null)
                loadingText.text = message;
        }

        public void HideLoadingPanel()
        {
            loadingPanel.SetActive(false);
        }

        private void HideAllPanels()
        {
            mainPanel.SetActive(false);
            createLobbyPanel.SetActive(false);
            joinByCodePanel.SetActive(false);
            lobbyBrowserPanel.SetActive(false);
            lobbyRoomPanel.SetActive(false);
            loadingPanel.SetActive(false);
        }

        private void OnCreateLobbyClick()
        {
            ShowCreateLobbyPanel();
        }

        private void OnJoinByCodeClick()
        {
            ShowJoinByCodePanel();
        }

        private void OnBrowseLobbyClick()
        {
            ShowLobbyBrowserPanel();
            // Trigger lobby browser to refresh
            var browser = lobbyBrowserPanel.GetComponent<LobbyBrowserUI>();
            if (browser != null)
                browser.RefreshLobbies();
        }

        private void OnQuitClick()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}


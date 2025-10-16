using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using TheButton.Game;

namespace TheButton.UI
{
    /// <summary>
    /// Displays game state UI (win/lose screens)
    /// Shows appropriate UI based on game state
    /// </summary>
    public class GameStateUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [Tooltip("Panel to show when player wins")]
        [SerializeField] private GameObject winPanel;
        
        [Tooltip("Panel to show when player loses")]
        [SerializeField] private GameObject losePanel;
        
        [Tooltip("Panel for in-game HUD")]
        [SerializeField] private GameObject hudPanel;
        
        [Header("Win Panel Elements")]
        [SerializeField] private TextMeshProUGUI winMessageText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button returnToLobbyButton;
        
        [Header("Lose Panel Elements")]
        [SerializeField] private TextMeshProUGUI loseMessageText;
        [SerializeField] private Button loseRestartButton;
        [SerializeField] private Button loseReturnToLobbyButton;
        
        [Header("Game Timer (Optional)")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private bool showTimer = true;
        
        private GameManager gameManager;
        
        private void Start()
        {
            // Find game manager
            gameManager = FindObjectOfType<GameManager>();
            
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
                UpdateUI(gameManager.CurrentGameState);
            }
            
            // Setup button listeners
            SetupButtons();
            
            // Hide win/lose panels initially
            ShowPlaying();
        }
        
        private void Update()
        {
            // Update timer if enabled
            if (showTimer && timerText != null && gameManager != null)
            {
                float remainingTime = gameManager.GetRemainingTime();
                if (remainingTime >= 0)
                {
                    int minutes = Mathf.FloorToInt(remainingTime / 60f);
                    int seconds = Mathf.FloorToInt(remainingTime % 60f);
                    timerText.text = $"Time: {minutes:00}:{seconds:00}";
                }
                else
                {
                    timerText.text = "Time: --:--";
                }
            }
        }
        
        private void SetupButtons()
        {
            // Win panel buttons
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            
            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.onClick.AddListener(OnReturnToLobbyClicked);
            }
            
            // Lose panel buttons
            if (loseRestartButton != null)
            {
                loseRestartButton.onClick.AddListener(OnRestartClicked);
            }
            
            if (loseReturnToLobbyButton != null)
            {
                loseReturnToLobbyButton.onClick.AddListener(OnReturnToLobbyClicked);
            }
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            UpdateUI(newState);
        }
        
        private void UpdateUI(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    ShowPlaying();
                    break;
                    
                case GameState.Won:
                    ShowWin();
                    break;
                    
                case GameState.Lost:
                    ShowLose();
                    break;
            }
        }
        
        private void ShowPlaying()
        {
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(true);
            
            // Unlock cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void ShowWin()
        {
            if (winPanel != null) winPanel.SetActive(true);
            if (losePanel != null) losePanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            
            // Update win message
            if (winMessageText != null && gameManager != null)
            {
                ulong winnerClientId = gameManager.GetWinnerClientId();
                ulong localClientId = NetworkManager.Singleton?.LocalClientId ?? 0;
                
                if (winnerClientId == localClientId)
                {
                    winMessageText.text = "You Won!\nYou successfully escaped!";
                }
                else
                {
                    winMessageText.text = $"Player {winnerClientId} Won!\nBetter luck next time!";
                }
            }
            
            // Show cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Only host can see restart button
            bool isHost = NetworkManager.Singleton?.IsHost ?? false;
            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(isHost);
            }
        }
        
        private void ShowLose()
        {
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(true);
            if (hudPanel != null) hudPanel.SetActive(false);
            
            // Update lose message
            if (loseMessageText != null)
            {
                loseMessageText.text = "Game Over!\nYou failed to escape...";
            }
            
            // Show cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Only host can see restart button
            bool isHost = NetworkManager.Singleton?.IsHost ?? false;
            if (loseRestartButton != null)
            {
                loseRestartButton.gameObject.SetActive(isHost);
            }
        }
        
        private void OnRestartClicked()
        {
            if (gameManager != null)
            {
                gameManager.RestartGameServerRpc();
            }
        }
        
        private void OnReturnToLobbyClicked()
        {
            if (gameManager != null)
            {
                gameManager.ReturnToLobbyServerRpc();
            }
        }
        
        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= OnGameStateChanged;
            }
            
            // Cleanup button listeners
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }
            
            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.onClick.RemoveListener(OnReturnToLobbyClicked);
            }
            
            if (loseRestartButton != null)
            {
                loseRestartButton.onClick.RemoveListener(OnRestartClicked);
            }
            
            if (loseReturnToLobbyButton != null)
            {
                loseReturnToLobbyButton.onClick.RemoveListener(OnReturnToLobbyClicked);
            }
        }
    }
}


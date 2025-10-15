using TheButton.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheButton.UI
{
    public class PlayerStatsUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider hungerBar;
        [SerializeField] private Slider thirstBar;
        [SerializeField] private Slider staminaBar;
        
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI hungerText;
        [SerializeField] private TextMeshProUGUI thirstText;
        [SerializeField] private TextMeshProUGUI staminaText;

        private PlayerNetwork localPlayer;

        private void Start()
        {
            // Find local player
            FindLocalPlayer();
        }

        private void Update()
        {
            if (localPlayer == null)
            {
                FindLocalPlayer();
                return;
            }

            UpdateStatsDisplay();
        }

        private void FindLocalPlayer()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
                return;

            var players = FindObjectsOfType<PlayerNetwork>();
            foreach (var player in players)
            {
                if (player.IsOwner)
                {
                    localPlayer = player;
                    break;
                }
            }
        }

        private void UpdateStatsDisplay()
        {
            float health = localPlayer.Health.Value;
            float hunger = localPlayer.Hunger.Value;
            float thirst = localPlayer.Thirst.Value;
            float stamina = localPlayer.Stamina.Value;

            // Update sliders
            if (healthBar != null) healthBar.value = health / 100f;
            if (hungerBar != null) hungerBar.value = hunger / 100f;
            if (thirstBar != null) thirstBar.value = thirst / 100f;
            if (staminaBar != null) staminaBar.value = stamina / 100f;

            // Update texts
            if (healthText != null) healthText.text = $"HP: {health:F0}";
            if (hungerText != null) hungerText.text = $"Hunger: {hunger:F0}";
            if (thirstText != null) thirstText.text = $"Thirst: {thirst:F0}";
            if (staminaText != null) staminaText.text = $"Stamina: {stamina:F0}";
        }
    }
}


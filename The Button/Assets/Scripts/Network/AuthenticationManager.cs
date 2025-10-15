using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

namespace TheButton.Network
{
    public class AuthenticationManager : MonoBehaviour
    {
        public static AuthenticationManager Instance { get; private set; }
        
        public static string PlayerId => AuthenticationService.Instance.PlayerId;
        public static bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;

        public event Action OnSignedIn;

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

        private async void Start()
        {
            await InitializeUnityServicesAsync();
        }

        private async Task InitializeUnityServicesAsync()
        {
            try
            {
                // Initialize Unity Services
                await UnityServices.InitializeAsync();
                Debug.Log("[Auth] Unity Services initialized");

                // Sign in anonymously (no authentication required for now)
                await SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Auth] Failed to initialize Unity Services: {e.Message}");
            }
        }

        public async Task SignInAnonymouslyAsync()
        {
            try
            {
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log($"[Auth] Already signed in as {PlayerId}");
                    OnSignedIn?.Invoke();
                    return;
                }

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[Auth] Signed in anonymously as {PlayerId}");
                OnSignedIn?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Auth] Failed to sign in: {e.Message}");
                throw;
            }
        }
    }
}


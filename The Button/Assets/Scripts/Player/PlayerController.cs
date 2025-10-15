using Unity.Netcode;
using UnityEngine;

namespace TheButton.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float mouseSensitivity = 2f;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float minCameraAngle = -80f;
        [SerializeField] private float maxCameraAngle = 80f;

        private CharacterController characterController;
        private Vector3 velocity;
        private float cameraRotationX = 0f;
        private bool isGrounded;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Only enable controls and camera for the local player
            if (!IsOwner)
            {
                if (cameraTransform != null)
                    cameraTransform.gameObject.SetActive(false);
                enabled = false;
                return;
            }

            // Lock and hide cursor for local player
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!IsOwner) return;

            HandleMovement();
            HandleMouseLook();
            HandleJump();
        }

        private void HandleMovement()
        {
            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Calculate movement direction
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            
            // Apply movement
            characterController.Move(move * moveSpeed * Time.deltaTime);
        }

        private void HandleMouseLook()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate player horizontally
            transform.Rotate(Vector3.up * mouseX);

            // Rotate camera vertically
            cameraRotationX -= mouseY;
            cameraRotationX = Mathf.Clamp(cameraRotationX, minCameraAngle, maxCameraAngle);
            
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(cameraRotationX, 0, 0);
        }

        private void HandleJump()
        {
            // Check if grounded
            isGrounded = characterController.isGrounded;

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small negative value to keep grounded
            }

            // Jump
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = jumpForce;
            }

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (IsOwner && hasFocus)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // Called when Escape key is pressed (for unlocking cursor in editor/testing)
        private void LateUpdate()
        {
            if (IsOwner && Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }
}


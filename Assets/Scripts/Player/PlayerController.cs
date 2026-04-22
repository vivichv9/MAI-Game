using MAIGame.Core;
using MAIGame.Data;
using UnityEngine;

namespace MAIGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(GroundChecker))]
    public sealed class PlayerController : MonoBehaviour, IResettableLevelObject
    {
        [Header("Config")]
        [SerializeField] private PlayerConfig playerConfig;

        [Header("Camera")]
        [SerializeField] private Camera followCamera;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Vector3 cameraOffset = new(0f, 4f, -6f);
        [SerializeField, Min(0.01f)] private float mouseSensitivity = 0.08f;
        [SerializeField] private Vector2 pitchLimits = new(-35f, 60f);
        [SerializeField] private bool lockCursor = true;

        [Header("Fallback Values")]
        [SerializeField, Min(0f)] private float fallbackMoveSpeed = 5f;
        [SerializeField, Min(0f)] private float fallbackRotationSpeed = 540f;
        [SerializeField, Min(0f)] private float fallbackJumpHeight = 1.25f;
        [SerializeField, Min(0f)] private float fallbackGravity = 24f;
        [SerializeField, Min(0f)] private float fallbackGroundedRadius = 0.25f;
        [SerializeField] private LayerMask fallbackGroundMask = ~0;

        private CharacterController characterController;
        private PlayerInputHandler inputHandler;
        private GroundChecker groundChecker;

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float cameraYaw;
        private float cameraPitch = 20f;

        private float MoveSpeed => playerConfig != null ? playerConfig.MoveSpeed : fallbackMoveSpeed;
        private float RotationSpeed => playerConfig != null ? playerConfig.RotationSpeed : fallbackRotationSpeed;
        private bool AllowAirControl => playerConfig != null && playerConfig.AllowAirControl;
        private bool JumpEnabled => playerConfig == null || playerConfig.JumpEnabled;
        private float JumpHeight => playerConfig != null ? playerConfig.JumpHeight : fallbackJumpHeight;
        private float Gravity => playerConfig != null ? playerConfig.Gravity : fallbackGravity;
        private float GroundedRadius => playerConfig != null ? playerConfig.GroundedRadius : fallbackGroundedRadius;
        private LayerMask GroundMask => playerConfig != null ? playerConfig.GroundMask : fallbackGroundMask;
        public bool IsGrounded => groundChecker != null && groundChecker.IsGrounded;
        public float VerticalVelocity => verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputHandler = GetComponent<PlayerInputHandler>();
            groundChecker = GetComponent<GroundChecker>();

            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            cameraYaw = transform.eulerAngles.y;
        }

        private void OnEnable()
        {
            inputHandler.RestartRequested += HandleRestartRequested;
            LevelManager.Instance?.RegisterResettable(this);
        }

        private void OnDisable()
        {
            inputHandler.RestartRequested -= HandleRestartRequested;
            LevelManager.Instance?.UnregisterResettable(this);
        }

        private void Start()
        {
            if (followCamera == null)
            {
                followCamera = Camera.main;
            }

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            UpdateCameraPosition();
        }

        private void Update()
        {
            if (!CanAcceptGameplayInput())
            {
                UpdateCameraPosition();
                return;
            }

            UpdateCameraAngles();
            MovePlayer(Time.deltaTime);
            UpdateCameraPosition();
        }

        public void ResetLevelObject()
        {
            WarpTo(spawnPosition, spawnRotation);
        }

        public void WarpTo(Vector3 position, Quaternion rotation)
        {
            characterController.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            characterController.enabled = true;

            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            cameraYaw = rotation.eulerAngles.y;
            cameraPitch = 20f;
            UpdateCameraPosition();
        }

        private void HandleRestartRequested()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevelState();
                return;
            }

            ResetLevelObject();
        }

        private bool CanAcceptGameplayInput()
        {
            GameManager gameManager = GameManager.Instance;
            return gameManager == null || gameManager.CurrentState == GameState.Playing;
        }

        private void UpdateCameraAngles()
        {
            Vector2 lookInput = inputHandler.LookInput * mouseSensitivity;
            cameraYaw += lookInput.x;
            cameraPitch = Mathf.Clamp(cameraPitch - lookInput.y, pitchLimits.x, pitchLimits.y);
        }

        private void MovePlayer(float deltaTime)
        {
            bool isGrounded = groundChecker.CheckGround(GroundMask, GroundedRadius);

            if (isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (isGrounded || AllowAirControl)
            {
                horizontalVelocity = CalculateDesiredHorizontalVelocity();
            }

            if (isGrounded && JumpEnabled && inputHandler.JumpPressedThisFrame)
            {
                verticalVelocity = Mathf.Sqrt(2f * Gravity * JumpHeight);
            }

            verticalVelocity -= Gravity * deltaTime;

            Vector3 velocity = horizontalVelocity;
            velocity.y = verticalVelocity;
            characterController.Move(velocity * deltaTime);

            RotateTowardsMovement(deltaTime);
        }

        private Vector3 CalculateDesiredHorizontalVelocity()
        {
            Vector2 movementInput = inputHandler.MovementInput;
            if (movementInput.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            Quaternion yawRotation = Quaternion.Euler(0f, cameraYaw, 0f);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;
            Vector3 movement = forward * movementInput.y + right * movementInput.x;

            return movement.normalized * MoveSpeed;
        }

        private void RotateTowardsMovement(float deltaTime)
        {
            Vector3 flatVelocity = new(horizontalVelocity.x, 0f, horizontalVelocity.z);
            if (flatVelocity.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(flatVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                RotationSpeed * deltaTime);
        }

        private void UpdateCameraPosition()
        {
            if (followCamera == null)
            {
                return;
            }

            Transform target = cameraTarget != null ? cameraTarget : transform;
            Quaternion cameraRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            followCamera.transform.SetPositionAndRotation(
                target.position + cameraRotation * cameraOffset,
                cameraRotation);
        }
    }
}

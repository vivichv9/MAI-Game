using System;
using MAIGame.Core;
using MAIGame.Data;
using MAIGame.Player;
using UnityEngine;
using UnityEngine.Events;

namespace MAIGame.Echo
{
    public sealed class EchoSwapController : MonoBehaviour
    {
        public event Action<EchoInstance> OnSwapStarted;
        public event Action<EchoInstance> OnSwapCompleted;
        public event Action<EchoSwapFailureReason> OnSwapFailed;

        [Header("Config")]
        [SerializeField] private EchoConfig echoConfig;

        [Header("References")]
        [SerializeField] private EchoSelectionController selectionController;
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private CharacterController playerCharacterController;
        [SerializeField] private GroundChecker groundChecker;

        [Header("Validation")]
        [SerializeField] private LayerMask destinationBlockers = ~0;
        [SerializeField, Min(0f)] private float destinationCheckPadding = 0.02f;

        [Header("Feedback Hooks")]
        [SerializeField] private UnityEvent<EchoInstance> swapStartedFeedback;
        [SerializeField] private UnityEvent<EchoInstance> swapCompletedFeedback;
        [SerializeField] private UnityEvent<EchoSwapFailureReason> swapFailedFeedback;

        private readonly Collider[] overlapResults = new Collider[16];
        private float lastSuccessfulSwapTime = float.NegativeInfinity;

        public float CooldownRemaining
        {
            get
            {
                float cooldown = echoConfig != null ? echoConfig.SwapCooldownSeconds : 0f;
                return Mathf.Max(0f, lastSuccessfulSwapTime + cooldown - Time.time);
            }
        }

        public bool IsCooldownReady => CooldownRemaining <= 0f;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            CacheReferences();

            if (inputHandler != null)
            {
                inputHandler.EchoSwapRequested += HandleSwapRequested;
            }
        }

        private void OnDisable()
        {
            if (inputHandler != null)
            {
                inputHandler.EchoSwapRequested -= HandleSwapRequested;
            }
        }

        public bool TrySwap()
        {
            if (!CanSwap(out EchoInstance activeEcho, out EchoSwapFailureReason failureReason))
            {
                FailSwap(failureReason);
                return false;
            }

            Vector3 playerPosition = playerTransform.position;
            Quaternion playerRotation = playerTransform.rotation;
            Vector3 echoPosition = activeEcho.CapturedPosition;
            Quaternion echoRotation = activeEcho.CapturedRotation;

            OnSwapStarted?.Invoke(activeEcho);
            swapStartedFeedback?.Invoke(activeEcho);

            if (playerController != null)
            {
                playerController.WarpTo(echoPosition, echoRotation);
            }
            else
            {
                WarpPlayerTransform(echoPosition, echoRotation);
            }

            activeEcho.MoveTo(playerPosition, playerRotation);
            lastSuccessfulSwapTime = Time.time;

            OnSwapCompleted?.Invoke(activeEcho);
            swapCompletedFeedback?.Invoke(activeEcho);
            return true;
        }

        private void HandleSwapRequested()
        {
            TrySwap();
        }

        private bool CanSwap(out EchoInstance activeEcho, out EchoSwapFailureReason failureReason)
        {
            CacheReferences();

            activeEcho = selectionController != null ? selectionController.ActiveEcho : null;
            failureReason = EchoSwapFailureReason.NoActiveEcho;

            if (playerTransform == null)
            {
                failureReason = EchoSwapFailureReason.NoPlayerReference;
                return false;
            }

            if (!IsGamePlaying())
            {
                failureReason = EchoSwapFailureReason.GameNotPlaying;
                return false;
            }

            if (activeEcho == null || !activeEcho.IsActive)
            {
                failureReason = EchoSwapFailureReason.NoActiveEcho;
                return false;
            }

            if (!IsCooldownReady)
            {
                failureReason = EchoSwapFailureReason.CooldownActive;
                return false;
            }

            if (echoConfig != null && !echoConfig.AllowSwapInAir && groundChecker != null && !groundChecker.IsGrounded)
            {
                failureReason = EchoSwapFailureReason.PlayerNotGrounded;
                return false;
            }

            if (echoConfig != null && !echoConfig.AllowSwapWhileFalling && playerController != null && !playerController.IsGrounded && playerController.VerticalVelocity < -0.01f)
            {
                failureReason = EchoSwapFailureReason.PlayerFalling;
                return false;
            }

            if (!IsPlayerDestinationValid(activeEcho.CapturedPosition, playerTransform, activeEcho.transform))
            {
                failureReason = EchoSwapFailureReason.InvalidPlayerDestination;
                return false;
            }

            if (!IsPlayerDestinationValid(playerTransform.position, playerTransform, activeEcho.transform))
            {
                failureReason = EchoSwapFailureReason.InvalidEchoDestination;
                return false;
            }

            return true;
        }

        private bool IsGamePlaying()
        {
            GameManager gameManager = GameManager.Instance;
            return gameManager == null || gameManager.CurrentState == GameState.Playing;
        }

        private void WarpPlayerTransform(Vector3 position, Quaternion rotation)
        {
            if (playerCharacterController != null)
            {
                playerCharacterController.enabled = false;
            }

            playerTransform.SetPositionAndRotation(position, rotation);

            if (playerCharacterController != null)
            {
                playerCharacterController.enabled = true;
            }
        }

        private bool IsPlayerDestinationValid(Vector3 destination, Transform ignoredRootA, Transform ignoredRootB)
        {
            if (playerCharacterController == null)
            {
                return true;
            }

            float radius = Mathf.Max(0.01f, playerCharacterController.radius - destinationCheckPadding);
            float height = Mathf.Max(playerCharacterController.height, radius * 2f);
            float halfSegmentHeight = Mathf.Max(0f, (height * 0.5f) - radius);
            Vector3 center = destination + playerCharacterController.center;
            Vector3 bottom = center + Vector3.down * halfSegmentHeight;
            Vector3 top = center + Vector3.up * halfSegmentHeight;
            LayerMask blockers = destinationBlockers.value != 0 ? destinationBlockers : ~0;

            int hitCount = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                radius,
                overlapResults,
                blockers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = overlapResults[i];
                if (hit == null || IsIgnored(hit.transform, ignoredRootA) || IsIgnored(hit.transform, ignoredRootB))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static bool IsIgnored(Transform hitTransform, Transform ignoredRoot)
        {
            return ignoredRoot != null && hitTransform != null && hitTransform.IsChildOf(ignoredRoot);
        }

        private void FailSwap(EchoSwapFailureReason reason)
        {
            OnSwapFailed?.Invoke(reason);
            swapFailedFeedback?.Invoke(reason);
        }

        private void CacheReferences()
        {
            if (selectionController == null)
            {
                selectionController = GetComponent<EchoSelectionController>();
            }

            if (inputHandler == null)
            {
                inputHandler = FindObjectOfType<PlayerInputHandler>();
            }

            if (playerController == null && inputHandler != null)
            {
                playerController = inputHandler.GetComponent<PlayerController>();
            }

            if (playerTransform == null)
            {
                playerTransform = playerController != null ? playerController.transform : inputHandler != null ? inputHandler.transform : null;
            }

            if (playerCharacterController == null && playerTransform != null)
            {
                playerCharacterController = playerTransform.GetComponent<CharacterController>();
            }

            if (groundChecker == null && playerTransform != null)
            {
                groundChecker = playerTransform.GetComponent<GroundChecker>();
            }
        }
    }
}

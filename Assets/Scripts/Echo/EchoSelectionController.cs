using System;
using MAIGame.Player;
using UnityEngine;

namespace MAIGame.Echo
{
    [RequireComponent(typeof(EchoManager))]
    public sealed class EchoSelectionController : MonoBehaviour
    {
        public event Action<EchoInstance> OnActiveEchoChanged;

        [SerializeField] private EchoManager echoManager;
        [SerializeField] private PlayerInputHandler inputHandler;

        public EchoInstance ActiveEcho { get; private set; }
        public int ActiveEchoIndex => ActiveEcho != null ? ActiveEcho.Index : -1;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            CacheReferences();

            if (echoManager != null)
            {
                echoManager.OnEchoCreated += HandleEchoCreated;
                echoManager.OnEchoRemoved += HandleEchoRemoved;
            }

            if (inputHandler != null)
            {
                inputHandler.EchoSelectNextRequested += SelectNextEcho;
                inputHandler.EchoSelectPreviousRequested += SelectPreviousEcho;
            }
        }

        private void OnDisable()
        {
            if (echoManager != null)
            {
                echoManager.OnEchoCreated -= HandleEchoCreated;
                echoManager.OnEchoRemoved -= HandleEchoRemoved;
            }

            if (inputHandler != null)
            {
                inputHandler.EchoSelectNextRequested -= SelectNextEcho;
                inputHandler.EchoSelectPreviousRequested -= SelectPreviousEcho;
            }
        }

        public void SelectNextEcho()
        {
            SelectRelative(1);
        }

        public void SelectPreviousEcho()
        {
            SelectRelative(-1);
        }

        public void SetActiveEcho(EchoInstance nextEcho)
        {
            if (ActiveEcho == nextEcho)
            {
                return;
            }

            SetEchoHighlight(ActiveEcho, false);
            ActiveEcho = nextEcho;
            SetEchoHighlight(ActiveEcho, true);
            OnActiveEchoChanged?.Invoke(ActiveEcho);
        }

        public void ClearSelection()
        {
            SetActiveEcho(null);
        }

        private void SelectRelative(int direction)
        {
            if (echoManager == null || echoManager.EchoCount == 0)
            {
                ClearSelection();
                return;
            }

            int currentIndex = ActiveEcho != null ? ActiveEcho.Index : -1;
            int nextIndex = currentIndex < 0
                ? 0
                : (currentIndex + direction + echoManager.EchoCount) % echoManager.EchoCount;

            SetActiveEcho(echoManager.Echoes[nextIndex]);
        }

        private void HandleEchoCreated(EchoInstance echo)
        {
            if (ActiveEcho == null)
            {
                SetActiveEcho(echo);
            }
        }

        private void HandleEchoRemoved(EchoInstance removedEcho)
        {
            if (ActiveEcho != removedEcho)
            {
                return;
            }

            SetEchoHighlight(removedEcho, false);
            ActiveEcho = null;

            if (echoManager != null && echoManager.EchoCount > 0)
            {
                int nextIndex = Mathf.Clamp(removedEcho.Index, 0, echoManager.EchoCount - 1);
                SetActiveEcho(echoManager.Echoes[nextIndex]);
                return;
            }

            OnActiveEchoChanged?.Invoke(null);
        }

        private void CacheReferences()
        {
            if (echoManager == null)
            {
                echoManager = GetComponent<EchoManager>();
            }

            if (inputHandler == null)
            {
                inputHandler = FindObjectOfType<PlayerInputHandler>();
            }
        }

        private static void SetEchoHighlight(EchoInstance echo, bool highlighted)
        {
            if (echo == null)
            {
                return;
            }

            EchoVisual visual = echo.GetComponent<EchoVisual>();
            if (visual != null)
            {
                visual.SetHighlighted(highlighted);
            }
        }
    }
}


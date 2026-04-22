using System;
using UnityEngine;

namespace MAIGame.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState> OnGameStateChanged;
        public event Action<bool> OnPauseChanged;

        [SerializeField] private GameState initialState = GameState.Bootstrapping;

        public GameState CurrentState { get; private set; }
        public bool IsPaused => CurrentState == GameState.Paused;

        private GameState stateBeforePause = GameState.Playing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CurrentState = initialState;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetState(GameState nextState)
        {
            if (CurrentState == nextState)
            {
                return;
            }

            CurrentState = nextState;
            OnGameStateChanged?.Invoke(CurrentState);
        }

        public void SetPaused(bool paused)
        {
            if (paused)
            {
                if (CurrentState == GameState.Paused)
                {
                    return;
                }

                stateBeforePause = CurrentState;
                SetState(GameState.Paused);
                Time.timeScale = 0f;
                OnPauseChanged?.Invoke(true);
                return;
            }

            if (CurrentState != GameState.Paused)
            {
                return;
            }

            Time.timeScale = 1f;
            SetState(stateBeforePause);
            OnPauseChanged?.Invoke(false);
        }

        public void QuitApplication()
        {
            Application.Quit();
        }
    }
}


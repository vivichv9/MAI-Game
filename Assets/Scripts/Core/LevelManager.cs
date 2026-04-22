using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MAIGame.Core
{
    public sealed class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public event Action OnLevelStarted;
        public event Action OnLevelRestartRequested;
        public event Action OnLevelCompleted;
        public event Action OnPlayerDeath;

        private readonly List<IResettableLevelObject> resettableObjects = new();

        public bool IsLevelCompleted { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            RefreshResettableObjects();
            OnLevelStarted?.Invoke();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterResettable(IResettableLevelObject resettable)
        {
            if (resettable == null || resettableObjects.Contains(resettable))
            {
                return;
            }

            resettableObjects.Add(resettable);
        }

        public void UnregisterResettable(IResettableLevelObject resettable)
        {
            if (resettable == null)
            {
                return;
            }

            resettableObjects.Remove(resettable);
        }

        public void CompleteLevel()
        {
            if (IsLevelCompleted)
            {
                return;
            }

            IsLevelCompleted = true;
            GameManager.Instance?.SetState(GameState.LevelCompleted);
            OnLevelCompleted?.Invoke();
        }

        public void HandlePlayerDeath()
        {
            OnPlayerDeath?.Invoke();
            RestartLevelState();
        }

        public void RestartLevelState()
        {
            GameManager.Instance?.SetState(GameState.Restarting);
            OnLevelRestartRequested?.Invoke();

            RefreshResettableObjects();
            foreach (IResettableLevelObject resettable in resettableObjects.ToArray())
            {
                resettable.ResetLevelObject();
            }

            IsLevelCompleted = false;
            GameManager.Instance?.SetState(GameState.Playing);
            OnLevelStarted?.Invoke();
        }

        public void ReloadActiveScene()
        {
            GameManager.Instance?.SetState(GameState.Restarting);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void RefreshResettableObjects()
        {
            resettableObjects.Clear();

            var behaviours = FindObjectsOfType<MonoBehaviour>(true);
            resettableObjects.AddRange(behaviours.OfType<IResettableLevelObject>());
        }
    }
}


using System;
using System.Collections.Generic;
using MAIGame.Core;
using MAIGame.Data;
using MAIGame.Player;
using UnityEngine;

namespace MAIGame.Echo
{
    public sealed class EchoManager : MonoBehaviour, IResettableLevelObject
    {
        public event Action<EchoInstance> OnEchoCreated;
        public event Action<EchoInstance> OnEchoRemoved;
        public event Action<EchoCreateFailureReason> OnEchoCreateFailed;

        [SerializeField] private EchoConfig echoConfig;
        [SerializeField] private GameObject echoPrefab;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private GroundChecker groundChecker;

        private readonly List<EchoInstance> echoes = new();
        private int nextEchoId = 1;

        public IReadOnlyList<EchoInstance> Echoes => echoes;
        public int EchoCount => echoes.Count;
        public int MaxEchoCount => echoConfig != null ? echoConfig.MaxEchoCount : 3;

        private void Awake()
        {
            CacheSceneReferences();
        }

        private void OnEnable()
        {
            CacheSceneReferences();

            if (inputHandler != null)
            {
                inputHandler.EchoCreateRequested += HandleEchoCreateRequested;
            }

            LevelManager.Instance?.RegisterResettable(this);
        }

        private void OnDisable()
        {
            if (inputHandler != null)
            {
                inputHandler.EchoCreateRequested -= HandleEchoCreateRequested;
            }

            LevelManager.Instance?.UnregisterResettable(this);
        }

        public void ResetLevelObject()
        {
            ClearEchoes();
            nextEchoId = 1;
        }

        public bool TryCreateEcho(out EchoInstance echo)
        {
            echo = null;

            if (playerTransform == null)
            {
                OnEchoCreateFailed?.Invoke(EchoCreateFailureReason.NoPlayerReference);
                return false;
            }

            if (echoPrefab == null)
            {
                OnEchoCreateFailed?.Invoke(EchoCreateFailureReason.NoEchoPrefab);
                return false;
            }

            if (echoes.Count >= MaxEchoCount)
            {
                OnEchoCreateFailed?.Invoke(EchoCreateFailureReason.LimitReached);
                return false;
            }

            if (echoConfig != null && echoConfig.EchoCreateAllowedOnlyWhenGrounded && groundChecker != null && !groundChecker.IsGrounded)
            {
                OnEchoCreateFailed?.Invoke(EchoCreateFailureReason.PlayerNotGrounded);
                return false;
            }

            Vector3 spawnPosition = playerTransform.position + (echoConfig != null ? echoConfig.EchoHeightOffset : Vector3.zero);
            Quaternion spawnRotation = playerTransform.rotation;

            GameObject echoObject = Instantiate(echoPrefab, spawnPosition, spawnRotation, transform);
            echoObject.name = $"Echo_{nextEchoId:00}";

            echo = echoObject.GetComponent<EchoInstance>();
            if (echo == null)
            {
                echo = echoObject.AddComponent<EchoInstance>();
            }

            echo.Initialize(nextEchoId, echoes.Count, spawnPosition, spawnRotation, Time.time);
            echoes.Add(echo);
            nextEchoId++;

            OnEchoCreated?.Invoke(echo);
            return true;
        }

        public void RemoveEcho(EchoInstance echo)
        {
            if (echo == null || !echoes.Remove(echo))
            {
                return;
            }

            echo.Deactivate();
            OnEchoRemoved?.Invoke(echo);
            Destroy(echo.gameObject);
            ReindexEchoes();
        }

        public void ClearEchoes()
        {
            for (int i = echoes.Count - 1; i >= 0; i--)
            {
                EchoInstance echo = echoes[i];
                if (echo == null)
                {
                    continue;
                }

                echo.Deactivate();
                OnEchoRemoved?.Invoke(echo);
                Destroy(echo.gameObject);
            }

            echoes.Clear();
        }

        private void HandleEchoCreateRequested()
        {
            TryCreateEcho(out _);
        }

        private void CacheSceneReferences()
        {
            if (inputHandler == null)
            {
                inputHandler = FindObjectOfType<PlayerInputHandler>();
            }

            if (playerTransform == null && inputHandler != null)
            {
                playerTransform = inputHandler.transform;
            }

            if (groundChecker == null && playerTransform != null)
            {
                groundChecker = playerTransform.GetComponent<GroundChecker>();
            }
        }

        private void ReindexEchoes()
        {
            for (int i = 0; i < echoes.Count; i++)
            {
                echoes[i].SetIndex(i);
            }
        }
    }
}


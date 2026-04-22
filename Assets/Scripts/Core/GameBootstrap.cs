using UnityEngine;

namespace MAIGame.Core
{
    /// <summary>
    /// Ensures core gameplay services exist before scene-specific systems start using them.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private bool persistAcrossScenes = true;

        private void Awake()
        {
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            EnsureService<GameManager>("GameManager");
            EnsureService<LevelManager>("LevelManager");

            GameManager.Instance?.SetState(GameState.Playing);
        }

        private void EnsureService<T>(string serviceName) where T : Component
        {
            if (FindObjectOfType<T>() != null)
            {
                return;
            }

            var serviceObject = new GameObject(serviceName);
            serviceObject.transform.SetParent(transform);
            serviceObject.AddComponent<T>();
        }
    }
}


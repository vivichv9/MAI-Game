using UnityEngine;

namespace MAIGame.Data
{
    [CreateAssetMenu(fileName = "LevelGoalConfig", menuName = "MAI Game/Configs/Level Goal Config")]
    public sealed class LevelGoalConfig : ScriptableObject
    {
        [Header("Completion")]
        [SerializeField] private bool requireAllConditionsBeforeExit = true;
        [SerializeField, Min(0)] private int requiredConditionCount = 1;

        [Header("Restart")]
        [SerializeField] private bool restartOnPlayerDeath = true;
        [SerializeField] private bool clearEchoesOnRestart = true;

        public bool RequireAllConditionsBeforeExit => requireAllConditionsBeforeExit;
        public int RequiredConditionCount => requiredConditionCount;
        public bool RestartOnPlayerDeath => restartOnPlayerDeath;
        public bool ClearEchoesOnRestart => clearEchoesOnRestart;
    }
}


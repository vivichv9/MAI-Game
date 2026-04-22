using UnityEngine;

namespace MAIGame.Data
{
    [CreateAssetMenu(fileName = "EchoConfig", menuName = "MAI Game/Configs/Echo Config")]
    public sealed class EchoConfig : ScriptableObject
    {
        [Header("Creation")]
        [SerializeField, Min(1)] private int maxEchoCount = 3;
        [SerializeField] private bool echoCreateAllowedOnlyWhenGrounded = true;
        [SerializeField] private Vector3 echoHeightOffset = Vector3.zero;
        [SerializeField] private LayerMask echoLayerMask;

        [Header("Lifetime")]
        [SerializeField] private bool useFiniteEchoLifetime;
        [SerializeField, Min(0f)] private float echoLifetimeSeconds;

        [Header("Swap")]
        [SerializeField, Min(0f)] private float swapCooldownSeconds = 1.5f;
        [SerializeField] private bool allowSwapInAir;
        [SerializeField] private bool allowSwapWhileFalling;
        [SerializeField] private bool allowSwapDuringInteraction;
        [SerializeField, Min(0f)] private float failFeedbackDuration = 1.25f;

        public int MaxEchoCount => maxEchoCount;
        public bool EchoCreateAllowedOnlyWhenGrounded => echoCreateAllowedOnlyWhenGrounded;
        public Vector3 EchoHeightOffset => echoHeightOffset;
        public LayerMask EchoLayerMask => echoLayerMask;
        public bool UseFiniteEchoLifetime => useFiniteEchoLifetime;
        public float EchoLifetimeSeconds => echoLifetimeSeconds;
        public float SwapCooldownSeconds => swapCooldownSeconds;
        public bool AllowSwapInAir => allowSwapInAir;
        public bool AllowSwapWhileFalling => allowSwapWhileFalling;
        public bool AllowSwapDuringInteraction => allowSwapDuringInteraction;
        public float FailFeedbackDuration => failFeedbackDuration;
    }
}


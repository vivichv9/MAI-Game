using UnityEngine;

namespace MAIGame.Data
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "MAI Game/Configs/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField, Min(0f)] private float rotationSpeed = 540f;
        [SerializeField] private bool allowAirControl;

        [Header("Jump")]
        [SerializeField] private bool jumpEnabled = true;
        [SerializeField, Min(0f)] private float jumpHeight = 1.25f;
        [SerializeField, Min(0f)] private float gravity = 24f;

        [Header("Ground Check")]
        [SerializeField, Min(0f)] private float groundedRadius = 0.25f;
        [SerializeField] private LayerMask groundMask = ~0;

        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public bool AllowAirControl => allowAirControl;
        public bool JumpEnabled => jumpEnabled;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float GroundedRadius => groundedRadius;
        public LayerMask GroundMask => groundMask;
    }
}


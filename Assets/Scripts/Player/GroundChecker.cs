using UnityEngine;

namespace MAIGame.Player
{
    public sealed class GroundChecker : MonoBehaviour
    {
        [SerializeField] private Transform checkOrigin;
        [SerializeField, Min(0f)] private float fallbackRadius = 0.25f;
        [SerializeField] private LayerMask fallbackGroundMask = ~0;

        public bool IsGrounded { get; private set; }

        private void Reset()
        {
            checkOrigin = transform;
        }

        public bool CheckGround(LayerMask groundMask, float radius)
        {
            Transform origin = checkOrigin != null ? checkOrigin : transform;
            float checkRadius = radius > 0f ? radius : fallbackRadius;
            LayerMask mask = groundMask.value != 0 ? groundMask : fallbackGroundMask;

            IsGrounded = Physics.CheckSphere(
                origin.position,
                checkRadius,
                mask,
                QueryTriggerInteraction.Ignore);

            return IsGrounded;
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = checkOrigin != null ? checkOrigin : transform;
            Gizmos.color = IsGrounded ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(origin.position, fallbackRadius);
        }
    }
}


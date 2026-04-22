using UnityEngine;

namespace MAIGame.Echo
{
    public sealed class EchoInstance : MonoBehaviour
    {
        public int Id { get; private set; }
        public int Index { get; private set; }
        public bool IsActive { get; private set; }
        public float CreatedAt { get; private set; }
        public Vector3 CapturedPosition { get; private set; }
        public Quaternion CapturedRotation { get; private set; }

        public void Initialize(int id, int index, Vector3 position, Quaternion rotation, float createdAt)
        {
            Id = id;
            Index = index;
            IsActive = true;
            CreatedAt = createdAt;
            CapturedPosition = position;
            CapturedRotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
        }

        public void SetIndex(int index)
        {
            Index = index;
        }

        public void MoveTo(Vector3 position, Quaternion rotation)
        {
            CapturedPosition = position;
            CapturedRotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}

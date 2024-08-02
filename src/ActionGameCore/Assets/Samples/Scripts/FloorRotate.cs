using UnityEngine;

namespace ActionGameCoreSamples
{
    public class FloorRotate : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        public Vector3 Axis = Vector3.up;

        [SerializeField]
        public float AngleSpeed = 360f;

        private void Reset()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            var angle = AngleSpeed * Time.fixedDeltaTime;
            var rotation = Quaternion.AngleAxis(angle, Axis);
            _rigidbody.MoveRotation(_rigidbody.rotation * rotation);
        }
    }
}

using UnityEngine;

namespace CharacterControls.Movements
{
    public class CharacterMoveController : MonoBehaviour, IMoveController
    {
        [SerializeField]
        public Rigidbody Rigidbody = null;

        [SerializeField]
        public Transform CameraTransform = null;

        [SerializeField]
        public float StepHeightMax = 0.25f;

        [SerializeField]
        private bool _autoResizeCapsuleCollider = true;

        [SerializeField]
        private CapsuleCollider _capsuleCollider = null;

        [SerializeField]
        public float LegStrength = 40.0f;

        [SerializeField]
        public float LegSuspenion = 20.0f;

        [SerializeField]
        public float WalkSpeed = 5.0f;

        private Vector2 _moveInput;

        private void Reset()
        {
            Rigidbody = GetComponent<Rigidbody>();
            CameraTransform = Camera.main?.transform;
            _capsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void Start()
        {
            if (_autoResizeCapsuleCollider && _capsuleCollider != null)
            {
                var height = _capsuleCollider.height;
                var center = _capsuleCollider.center;
                var stepHeight = StepHeightMax;
                var newHeight = height - stepHeight;
                var newCenter = center + new Vector3(0, stepHeight / 2, 0);
                _capsuleCollider.height = newHeight;
                _capsuleCollider.center = newCenter;
            }
        }

        public void SetMoveInput(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude > 1)
            {
                moveInput.Normalize();
            }

            _moveInput = moveInput;
        }

        private void FixedUpdate()
        {
            if (Rigidbody == null)
            {
                return;
            }

            const float DistanceMergin = 0.1f;
            var legRay = new Ray(transform.position + transform.up * (StepHeightMax + DistanceMergin), -transform.up);
            if (Physics.Raycast(legRay, out var hitInfo, StepHeightMax + DistanceMergin))
            {
                // Leg spring
                var hitDistance = hitInfo.distance - DistanceMergin;
                var springRatio = Mathf.Clamp01(hitDistance / StepHeightMax);
                var springPushForce = (1 - springRatio) * LegStrength;
                Rigidbody.AddForce(springPushForce * transform.up, ForceMode.Acceleration);

                // Leg suspension
                var currentVelocity = Rigidbody.velocity;
                if (hitInfo.rigidbody != null)
                {
                    currentVelocity -= hitInfo.rigidbody.velocity;
                }
                var suspensionForce = Vector3.Project(currentVelocity, transform.up) * -LegSuspenion;
                Rigidbody.AddForce(suspensionForce, ForceMode.Acceleration);

                // Move horizontal
                var forward = GetForwardOfMovementSpace();
                var right = Vector3.Cross(transform.up, forward);
                var targetVelocity = (forward * _moveInput.y + right * _moveInput.x) * WalkSpeed;
                var currentVerticalVelocity = Vector3.Project(Rigidbody.velocity, transform.up);
                Rigidbody.velocity = targetVelocity + currentVerticalVelocity;
            }
        }

        private Vector3 GetForwardOfMovementSpace()
        {
            if (CameraTransform == null)
            {
                return transform.forward;
            }

            var forward = CameraTransform.forward;
            if (forward == transform.up)
            {
                forward = -CameraTransform.up;
            }
            else if (forward == -transform.up)
            {
                forward = CameraTransform.up;
            }

            return Vector3.ProjectOnPlane(forward, transform.up).normalized;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * StepHeightMax);
        }
    }
}

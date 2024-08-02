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

        [SerializeField]
        public float FrictionStrength = 20;

        [SerializeField]
        public float StaticFriction = 0.1f;

        [SerializeField]
        public float DynamicFriction = 1.0f;

        [SerializeField]
        public float AirWalkSpeed = 2.0f;

        [SerializeField]
        public float JumpSpeed = 5.0f;

        private Vector2 _moveInput;
        private float _jumpInput;
        private FrictionCalculator _frictionCalculator = new FrictionCalculator();
        private float _jumpElapsedTime;

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

        public void SetJumpInput(float jumpInput)
        {
            _jumpInput = jumpInput;
        }

        private void FixedUpdate()
        {
            if (Rigidbody == null)
            {
                return;
            }

            const float GroundCheckSkipDurationAfterJump = 0.1f;
            _jumpElapsedTime += Time.fixedDeltaTime;
            var skipGroundCheck = _jumpElapsedTime < GroundCheckSkipDurationAfterJump;

            const float DistanceMergin = 0.1f;
            const float Radius = 0.1f;
            var legRay = new Ray(transform.position + transform.up * (StepHeightMax + DistanceMergin + Radius), -transform.up);
            if (!skipGroundCheck && Physics.SphereCast(legRay, Radius, out var hitInfo, StepHeightMax + DistanceMergin))
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
                var relativeVelocity = Rigidbody.velocity;
                if (hitInfo.rigidbody != null)
                {
                    relativeVelocity -= hitInfo.rigidbody.velocity;
                }
                var diffVelocity = targetVelocity - relativeVelocity;
                diffVelocity.y = 0;
                _frictionCalculator.StaticFriction = StaticFriction;
                _frictionCalculator.DynamicFriction = DynamicFriction;
                _frictionCalculator.Strength = FrictionStrength;
                _frictionCalculator.Calculate(-diffVelocity);
                var addVelocity = _frictionCalculator.FrictionForce;
                Rigidbody.AddForce(addVelocity, ForceMode.Acceleration);

                // Jump
                if (_jumpInput > 0)
                {
                    var verticalSpeed = Vector3.Dot(Rigidbody.velocity, transform.up);
                    verticalSpeed = Mathf.Min(verticalSpeed, 0);
                    Rigidbody.AddForce(transform.up * (JumpSpeed - verticalSpeed), ForceMode.VelocityChange);
                    _jumpElapsedTime = 0;
                }
            }
            else
            {
                var forward = GetForwardOfMovementSpace();
                var right = Vector3.Cross(transform.up, forward);
                var targetVelocity = (forward * _moveInput.y + right * _moveInput.x) * AirWalkSpeed;
                var addVelocity = targetVelocity - (Rigidbody.velocity - Vector3.Project(Rigidbody.velocity, transform.up));
                Rigidbody.AddForce(addVelocity, ForceMode.Acceleration);
            }

            _jumpInput = 0;
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

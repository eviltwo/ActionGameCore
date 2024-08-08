using System;
using UnityEngine;

namespace CharacterControls.Movements.Modules
{
    public class CharacterWalkModule : CharacterModuleBase, IInputReceiver<Vector2>, IInputReceiver<float>
    {
        [SerializeField]
        public float WalkSpeed = 5.0f;

        [SerializeField]
        public bool EnableDash = true;

        [SerializeField]
        public float DashSpeed = 10.0f;

        [SerializeField]
        public float SpeedReductionBySlope = 1.0f;

        [SerializeField]
        public float StaticFriction = 0.6f;

        [SerializeField]
        public float DynamicFriction = 0.5f;

        [SerializeField]
        public float AirWalkAcceleration = 2.0f;

        [SerializeField]
        public float AirWalkSpeedMax = 5.0f;

        private Vector2 _moveInput;
        private bool _dashInput;
        private FrictionCalculator _frictionCalculator = new FrictionCalculator();
        private ModuleRequestManager _stopMoveRequestManager = new ModuleRequestManager();

        public Vector3 TargetVelocity { get; private set; }

        public Vector3 RelativeVelocityToGround { get; private set; }

        public void OnReceiveInput(string key, Vector2 value)
        {
            if (key == "Move")
            {
                if (value.sqrMagnitude > 1)
                {
                    value.Normalize();
                }

                _moveInput = value;
            }
        }

        public void OnReceiveInput(string key, float value)
        {
            if (key == "Dash")
            {
                _dashInput = value > 0;
            }
        }

        public override void FixedUpdateModule(in CharacterMoveModulePayload payload)
        {
            var input = _stopMoveRequestManager.HasRequest() ? Vector2.zero : _moveInput;
            if (payload.Controller.IsGrounded)
            {
                Walk(payload, input);
            }
            else
            {
                WalkAir(payload, input);
            }
        }

        public void Walk(in CharacterMoveModulePayload payload, Vector2 input)
        {
            var root = payload.Root;
            var rb = payload.Rigidbody;
            var hit = payload.Controller.LastGroundHit;

            var forward = CharacterMoveUtility.GetForwardMovementDirectionFromCamera(root, payload.Controller.CameraTransform);
            var right = Vector3.Cross(root.up, forward);
            var speed = _dashInput && EnableDash ? DashSpeed : WalkSpeed;
            var inputBaseVelocity = (forward * input.y + right * input.x) * speed;
            TargetVelocity = Quaternion.FromToRotation(root.up, hit.normal) * inputBaseVelocity;
            if (TargetVelocity.y > 0)
            {
                var targetAngle = Vector3.Angle(inputBaseVelocity, TargetVelocity);
                var speedReductionRatio = Mathf.Sin(targetAngle * Mathf.Deg2Rad);
                TargetVelocity -= TargetVelocity * (speedReductionRatio * SpeedReductionBySlope);
            }
            RelativeVelocityToGround = rb.velocity;
            if (hit.rigidbody != null)
            {
                RelativeVelocityToGround -= hit.rigidbody.GetPointVelocity(hit.point);
            }
            var diffVelocity = TargetVelocity - RelativeVelocityToGround;
            diffVelocity -= Vector3.Project(diffVelocity, hit.normal); // Remove velocity on normal
            _frictionCalculator.StaticFriction = StaticFriction;
            _frictionCalculator.DynamicFriction = DynamicFriction;
            _frictionCalculator.Calculate(-diffVelocity);
            var addVelocity = _frictionCalculator.FrictionForce / Time.fixedDeltaTime;
            rb.AddForce(addVelocity, ForceMode.Acceleration);
        }

        private void WalkAir(in CharacterMoveModulePayload payload, Vector2 input)
        {
            var root = payload.Root;
            var rb = payload.Rigidbody;
            var hit = payload.Controller.LastGroundHit;

            var forward = CharacterMoveUtility.GetForwardMovementDirectionFromCamera(root, payload.Controller.CameraTransform);
            var right = Vector3.Cross(root.up, forward);
            TargetVelocity = (forward * input.y + right * input.x) * AirWalkSpeedMax;
            RelativeVelocityToGround = rb.velocity;
            var diffVelocity = TargetVelocity - RelativeVelocityToGround;
            diffVelocity.y = 0;
            var addVelocity = diffVelocity * AirWalkAcceleration;

            // If the maximum input is made in the same direction as the movement direction, don't brake the velocity.
            var targetDirectionAddSpeed = Vector3.Dot(addVelocity, TargetVelocity.normalized);
            const float fullInputThreshold = 0.75f;
            if (input.sqrMagnitude > fullInputThreshold * fullInputThreshold && targetDirectionAddSpeed < 0)
            {
                var moveDirectionMatchRatio = 1 - Mathf.Clamp01(Vector3.Angle(TargetVelocity, RelativeVelocityToGround) / 180);
                addVelocity -= TargetVelocity.normalized * (targetDirectionAddSpeed * moveDirectionMatchRatio);
            }

            rb.AddForce(addVelocity, ForceMode.Acceleration);
        }

        public IDisposable RequestStopMove()
        {
            return _stopMoveRequestManager.GetRequest();
        }
    }
}

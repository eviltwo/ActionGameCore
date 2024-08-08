using System;
using UnityEngine;

namespace CharacterControls.Movements.Modules
{
    public class CharacterWalkModule : CharacterModuleBase, IInputReceiver<Vector2>
    {
        [SerializeField]
        public float WalkSpeed = 5.0f;

        [SerializeField]
        public float FrictionStrength = 30;

        [SerializeField]
        public float StaticFriction = 0.1f;

        [SerializeField]
        public float DynamicFriction = 1.0f;

        [SerializeField]
        public float AirWalkAcceleration = 2.0f;

        [SerializeField]
        public float AirWalkSpeedMax = 5.0f;

        private Vector2 _moveInput;
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
            var inputBaseVelocity = (forward * input.y + right * input.x) * WalkSpeed;
            TargetVelocity = Quaternion.FromToRotation(root.up, hit.normal) * inputBaseVelocity;
            RelativeVelocityToGround = rb.velocity;
            if (hit.rigidbody != null)
            {
                RelativeVelocityToGround -= hit.rigidbody.GetPointVelocity(hit.point);
            }
            var diffVelocity = TargetVelocity - RelativeVelocityToGround;
            diffVelocity -= Vector3.Project(diffVelocity, root.up); // Remove velocity on normal
            _frictionCalculator.StaticFriction = StaticFriction;
            _frictionCalculator.DynamicFriction = DynamicFriction;
            _frictionCalculator.Strength = FrictionStrength;
            _frictionCalculator.Calculate(-diffVelocity);
            var addVelocity = _frictionCalculator.FrictionForce;
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

            // Speed limit
            if (addVelocity.sqrMagnitude > AirWalkSpeedMax * AirWalkSpeedMax)
            {
                addVelocity = addVelocity.normalized * AirWalkSpeedMax;
            }

            rb.AddForce(addVelocity, ForceMode.Acceleration);
        }

        public IDisposable RequestStopMove()
        {
            return _stopMoveRequestManager.GetRequest();
        }
    }
}

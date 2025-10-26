using System;
using CharacterControls.Inputs;
using UnityEngine;
using UnityEngine.Events;

namespace CharacterControls.Movements.Modules
{
    public class CharacterJumpModule : CharacterModuleBase
    {
        public string InputActionName = "Jump";

        public float BufferedInputDuration = 0.1f;

        public float CoyoteDuration = 0.1f;

        public float JumpSpeed = 5.0f;

        public float SkipGroundCheckTime = 0.1f;

        public UnityEvent OnJump = default;

        private bool _jumpInput;
        private float _jumpInputTime;
        private float _jumpElapsedTime;
        private float _groundElapsedTime;
        private IDisposable _skipGroundCheckRequest;
        private ModuleRequestManager _stopJumpRequestManager = new ModuleRequestManager();

        public bool IsJumping { get; private set; }
        public bool IsCoyoteTime => !IsJumping && _groundElapsedTime < CoyoteDuration;

        protected override void OnDisable()
        {
            base.OnDisable();
            _skipGroundCheckRequest?.Dispose();
            _skipGroundCheckRequest = null;
        }

        public override void OnReceiveInput(InputContext context)
        {
            if (context.actionName == InputActionName)
            {
                _jumpInput = context.ReadValue<float>() > 0;
                if (_jumpInput)
                {
                    _jumpInputTime = Time.time;
                }
            }
        }

        public override void FixedUpdateModule(in CharacterMoveModulePayload payload)
        {
            _groundElapsedTime += Time.fixedDeltaTime;
            if (payload.Controller.IsGrounded)
            {
                _groundElapsedTime = 0;
                IsJumping = false;
            }

            _jumpElapsedTime += Time.fixedDeltaTime;
            if (_skipGroundCheckRequest != null && _jumpElapsedTime > SkipGroundCheckTime)
            {
                _skipGroundCheckRequest.Dispose();
                _skipGroundCheckRequest = null;
            }

            if (_stopJumpRequestManager.HasRequest())
            {
                return;
            }

            var virtualGrounded = _groundElapsedTime < CoyoteDuration;
            if (virtualGrounded && _jumpInput && Time.time - _jumpInputTime < BufferedInputDuration)
            {
                var rb = payload.Controller.Rigidbody;
                // Fit ground
                if (rb.position.y < payload.Controller.LastGroundHit.point.y)
                {
                    rb.MovePosition(new Vector3(rb.position.x, payload.Controller.LastGroundHit.point.y, rb.position.z));
                }

                // Get current velocity to cancel it out.
                var accVelocity = rb.GetAccumulatedForce() / rb.mass * Time.fixedDeltaTime;
                var verticalSpeed = Vector3.Dot(rb.velocity + accVelocity, payload.Root.up);
                // Weaken jumping force by height.
                var currentHeight = Vector3.Dot(rb.position - payload.Controller.LastGroundHit.point, payload.Root.up);
                var jumpHeight = GetHeightByJumpSpeed(JumpSpeed, -Physics.gravity.y);
                var modifiedJumpSpeed = GetJumpSpeedByHeight(jumpHeight - currentHeight, -Physics.gravity.y);
                modifiedJumpSpeed = Mathf.Min(modifiedJumpSpeed, JumpSpeed);
                // Apply jump force
                rb.AddForce(payload.Root.up * (modifiedJumpSpeed - verticalSpeed), ForceMode.VelocityChange);
                _jumpElapsedTime = 0;
                IsJumping = true;
                _groundElapsedTime = float.MaxValue;
                _skipGroundCheckRequest = payload.Controller.RequestSkipGroundCheck();
                OnJump?.Invoke();
            }
        }

        private static float GetHeightByJumpSpeed(float jumpSpeed, float gravity)
        {
            return jumpSpeed * jumpSpeed / (2 * gravity);
        }

        private static float GetJumpSpeedByHeight(float height, float gravity)
        {
            return Mathf.Sqrt(2 * gravity * height);
        }

        public IDisposable RequestStopJump()
        {
            return _stopJumpRequestManager.GetRequest();
        }
    }
}

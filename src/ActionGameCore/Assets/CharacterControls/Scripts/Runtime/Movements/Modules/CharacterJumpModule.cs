using System;
using UnityEngine;
using UnityEngine.Events;

namespace CharacterControls.Movements.Modules
{
    public class CharacterJumpModule : MonoBehaviour, ICharacterMoveModule, IInputReceiver<float>
    {
        [SerializeField]
        public float BufferedInputDuration = 0.1f;

        [SerializeField]
        public float CoyoteDuration = 0.1f;

        [SerializeField]
        public float JumpSpeed = 5.0f;

        [SerializeField]
        public float SkipGroundCheckTime = 0.1f;

        [SerializeField]
        public UnityEvent OnJump = default;

        private bool _jumpInput;
        private float _jumpInputTime;
        private float _jumpElapsedTime;
        private float _groundElapsedTime;
        private IDisposable _skipGroundCheckRequest;

        private void OnDisable()
        {
            _skipGroundCheckRequest?.Dispose();
            _skipGroundCheckRequest = null;
        }

        public void OnReceiveInput(string key, float value)
        {
            if (key == "Jump")
            {
                _jumpInput = value > 0;
                if (_jumpInput)
                {
                    _jumpInputTime = Time.time;
                }
            }
        }

        public void FixedUpdateModule(in CharacterMoveModulePayload payload)
        {
            _groundElapsedTime += Time.fixedDeltaTime;
            if (payload.Controller.IsGrounded)
            {
                _groundElapsedTime = 0;
            }

            var virtualGrounded = _groundElapsedTime < CoyoteDuration;
            if (virtualGrounded && _jumpInput && Time.time - _jumpInputTime < BufferedInputDuration)
            {
                var rig = payload.Controller.Rigidbody;
                var accVelocity = rig.GetAccumulatedForce() / rig.mass * Time.fixedDeltaTime;
                var verticalSpeed = Vector3.Dot(rig.velocity + accVelocity, transform.up);
                verticalSpeed = Mathf.Min(verticalSpeed, 0);
                rig.AddForce(transform.up * (JumpSpeed - verticalSpeed), ForceMode.VelocityChange);
                _jumpElapsedTime = 0;
                _groundElapsedTime = float.MaxValue;
                _skipGroundCheckRequest = payload.Controller.RequestSkipGroundCheck();
                OnJump?.Invoke();
            }

            _jumpElapsedTime += Time.fixedDeltaTime;
            if (_skipGroundCheckRequest != null && _jumpElapsedTime > SkipGroundCheckTime)
            {
                _skipGroundCheckRequest.Dispose();
                _skipGroundCheckRequest = null;
            }
        }
    }
}
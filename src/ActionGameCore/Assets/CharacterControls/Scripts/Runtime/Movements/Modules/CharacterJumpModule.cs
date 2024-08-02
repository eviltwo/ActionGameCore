using System;
using UnityEngine;

namespace CharacterControls.Movements.Modules
{
    public class CharacterJumpModule : MonoBehaviour, ICharacterMoveModule, IInputReceiver<float>
    {
        [SerializeField]
        public float JumpSpeed = 5.0f;

        [SerializeField]
        public float SkipGroundCheckTime = 0.1f;

        private float _jumpInput;
        private float _jumpElapsedTime;
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
                _jumpInput = value;
            }
        }

        public void FixedUpdateModule(in CharacterMoveModulePayload payload)
        {
            if (payload.Controller.IsGrounded && _jumpInput > 0)
            {
                var rig = payload.Controller.Rigidbody;
                var verticalSpeed = Vector3.Dot(rig.velocity, transform.up);
                verticalSpeed = Mathf.Min(verticalSpeed, 0);
                rig.AddForce(transform.up * (JumpSpeed - verticalSpeed), ForceMode.VelocityChange);
                _jumpElapsedTime = 0;
                _skipGroundCheckRequest = payload.Controller.RequestSkipGroundCheck();
            }
            _jumpInput = 0;

            _jumpElapsedTime += Time.fixedDeltaTime;
            if (_skipGroundCheckRequest != null && _jumpElapsedTime > SkipGroundCheckTime)
            {
                _skipGroundCheckRequest.Dispose();
                _skipGroundCheckRequest = null;
            }
        }
    }
}

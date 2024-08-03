using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CharacterControls.Movements.Modules
{
    public class CharacterPullUpModule : MonoBehaviour, ICharacterMoveModule, IInputReceiver<float>
    {
        [SerializeField]
        public float[] CheckDistances = new float[] { 0.25f, 0.3f, 0.4f };

        [SerializeField]
        public float CheckRadius = 0.1f;

        [SerializeField]
        public float MaxHeight = 1.5f;

        [SerializeField]
        public float MinHeight = 0.5f;

        [SerializeField]
        public float SlopeLimit = 45.0f;

        [SerializeField]
        public float StopMoveDuration = 0.5f;

        [SerializeField]
        public float SafetyCapsuleStart = 0.5f;

        [SerializeField]
        public float SafetyCapsuleEnd = 1.5f;

        [SerializeField]
        public float SafetyCapsuleRadius = 0.2f;

        [SerializeField]
        public UnityEvent OnPullUp = default;

        private bool _jumpInput;
        private List<IDisposable> _stopRequests = new List<IDisposable>();
        private float _stopElapsedTime;
        private List<CharacterJumpModule> _jumpModuleBuffer = new List<CharacterJumpModule>();

        public Vector3 MoveDirection { get; private set; }

        private void Start()
        {
            var characterMoveController = GetComponentInParent<CharacterMoveController>();
            characterMoveController?.RegisterModule(this);
        }

        private void OnDestroy()
        {
            var characterMoveController = GetComponentInParent<CharacterMoveController>();
            characterMoveController?.UnregisterModule(this);

            foreach (var request in _stopRequests)
            {
                request?.Dispose();
            }
            _stopRequests.Clear();
        }

        public void OnReceiveInput(string key, float value)
        {
            if (key == "Jump")
            {
                _jumpInput = value > 0f;
            }
        }

        public void FixedUpdateModule(in CharacterMoveModulePayload payload)
        {
            _stopElapsedTime += Time.deltaTime;
            if (_stopRequests.Count > 0 && _stopElapsedTime > StopMoveDuration)
            {
                foreach (var request in _stopRequests)
                {
                    request?.Dispose();
                }
                _stopRequests.Clear();
            }

            if (!_jumpInput || payload.Controller.IsGrounded || _stopRequests.Count > 0)
            {
                return;
            }

            var velocity = payload.Controller.TargetVelocity;
            var verticalVelocity = Vector3.Project(velocity, transform.up);
            var direction = (velocity - verticalVelocity).normalized;
            if (direction.sqrMagnitude > 0f)
            {
                MoveDirection = direction;
            }

            if (MoveDirection.sqrMagnitude == 0f)
            {
                return;
            }

            var checkCount = CheckDistances.Length;
            for (var i = 0; i < checkCount; i++)
            {
                var distance = CheckDistances[i];
                var ray = new Ray(transform.position + MoveDirection * distance + transform.up * (MaxHeight + CheckRadius), -transform.up);
                if (Physics.SphereCast(ray, CheckRadius, out var hitInfo, MaxHeight - MinHeight, payload.Controller.GroundLayer)
                    && Vector3.Angle(hitInfo.normal, Vector3.up) < SlopeLimit
                    && !Physics.CheckCapsule(hitInfo.point + transform.up * SafetyCapsuleStart, hitInfo.point + transform.up * SafetyCapsuleEnd, SafetyCapsuleRadius, payload.Controller.GroundLayer))
                {
                    var rig = payload.Controller.Rigidbody;
                    rig.MovePosition(hitInfo.point);
                    var accVelocity = payload.Controller.Rigidbody.GetAccumulatedForce() / rig.mass * Time.fixedDeltaTime;
                    rig.AddForce(-rig.velocity - accVelocity, ForceMode.VelocityChange);
                    _stopRequests.Add(payload.Controller.RequestStopMove());
                    payload.Controller.GetModules(_jumpModuleBuffer);
                    for (int j = 0; j < _jumpModuleBuffer.Count; j++)
                    {
                        _stopRequests.Add(_jumpModuleBuffer[j].RequestStopJump());
                    }
                    _stopElapsedTime = 0f;
                    OnPullUp?.Invoke();
                    break;
                }
            }
        }
    }
}

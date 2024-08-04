using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CharacterControls.Movements.Modules
{
    public class CharacterPullUpModule : MonoBehaviour, ICharacterMoveModule
    {
        [SerializeField]
        public float CheckDistance = 0.3f;

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

            if (payload.Controller.IsGrounded || _stopRequests.Count > 0)
            {
                return;
            }

            var velocity = payload.Controller.TargetVelocity;
            var verticalVelocity = Vector3.Project(velocity, payload.Root.up);
            var direction = (velocity - verticalVelocity).normalized;
            if (direction.sqrMagnitude > 0f)
            {
                MoveDirection = direction;
            }

            if (MoveDirection.sqrMagnitude == 0f)
            {
                return;
            }

            var ray = new Ray(payload.Root.position + MoveDirection * CheckDistance + payload.Root.up * MaxHeight, -payload.Root.up);
            var verticalDistance = MaxHeight - MinHeight;
            if (CharacterMoveUtility.CheckGroundSafety(ray, CheckRadius, 8, verticalDistance, SlopeLimit, out var hit, payload.Controller.GroundLayer)
                && Vector3.Angle(hit.normal, Vector3.up) < SlopeLimit
                && !Physics.CheckCapsule(hit.point + payload.Root.up * SafetyCapsuleStart, hit.point + payload.Root.up * SafetyCapsuleEnd, SafetyCapsuleRadius, payload.Controller.GroundLayer))
            {
                var rig = payload.Controller.Rigidbody;
                rig.MovePosition(hit.point);
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
            }
        }
    }
}

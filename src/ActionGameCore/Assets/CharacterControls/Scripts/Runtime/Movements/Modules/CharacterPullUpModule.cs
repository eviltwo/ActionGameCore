using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CharacterControls.Movements.Modules
{
    public class CharacterPullUpModule : MonoBehaviour, ICharacterMoveModule
    {
        [SerializeField]
        public float CheckDistanceStart = 0.3f;

        [SerializeField]
        public float CheckDistanceEnd = 0.5f;

        [SerializeField]
        public int CheckCount = 4;

        [SerializeField]
        public float MaxHeight = 1.5f;

        [SerializeField]
        public float MinHeight = 0.5f;

        [SerializeField]
        public float SlopeLimit = 45.0f;

        [SerializeField]
        public float StopMoveDurationMin = 0.1f;

        [SerializeField]
        public float StopMoveDurationMax = 0.5f;

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
        public float StopDuration { get; private set; }
        private List<CharacterJumpModule> _jumpModuleBuffer = new List<CharacterJumpModule>();

        public Vector3 LastMoveDirection { get; private set; }

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
            if (_stopRequests.Count > 0 && _stopElapsedTime > StopDuration)
            {
                foreach (var request in _stopRequests)
                {
                    request?.Dispose();
                }
                _stopRequests.Clear();
            }

            if (payload.Controller.IsGrounded
                || Vector3.Dot(payload.Rigidbody.velocity, payload.Root.up) > 0f
                || _stopRequests.Count > 0)
            {
                return;
            }

            var velocity = payload.Controller.TargetVelocity;
            var verticalVelocity = Vector3.Project(velocity, payload.Root.up);
            var moveDirection = (velocity - verticalVelocity).normalized;
            if (moveDirection.sqrMagnitude == 0f)
            {
                return;
            }

            var ray = new Ray(payload.Root.position + moveDirection * CheckDistanceStart + payload.Root.up * MaxHeight, -payload.Root.up);
            var lineLength = CheckDistanceEnd - CheckDistanceStart;
            var verticalDistance = MaxHeight - MinHeight;
            if (CharacterMoveUtility.CheckLineGroundSafety(ray, verticalDistance, moveDirection, lineLength, CheckCount, SlopeLimit, out var hit, payload.Controller.GroundLayer)
                && !Physics.CheckCapsule(hit.point + payload.Root.up * SafetyCapsuleStart, hit.point + payload.Root.up * SafetyCapsuleEnd, SafetyCapsuleRadius, payload.Controller.GroundLayer))
            {
                var rig = payload.Controller.Rigidbody;
                var grabHeight = hit.point.y - rig.position.y;
                var edge = GetEdge(rig.position, hit.point, payload.Controller.GroundLayer);
                rig.MovePosition(edge);
                var accVelocity = payload.Controller.Rigidbody.GetAccumulatedForce() / rig.mass * Time.fixedDeltaTime;
                rig.AddForce(-rig.velocity - accVelocity, ForceMode.VelocityChange);
                if (hit.rigidbody != null)
                {
                    rig.AddForce(hit.rigidbody.velocity, ForceMode.VelocityChange);
                }
                _stopRequests.Add(payload.Controller.RequestStopMove());
                payload.Controller.GetModules(_jumpModuleBuffer);
                for (int j = 0; j < _jumpModuleBuffer.Count; j++)
                {
                    _stopRequests.Add(_jumpModuleBuffer[j].RequestStopJump());
                }
                LastMoveDirection = moveDirection;
                var heightRatio = Mathf.InverseLerp(MinHeight, MaxHeight, grabHeight);
                StopDuration = Mathf.Lerp(StopMoveDurationMin, StopMoveDurationMax, heightRatio);
                _stopElapsedTime = 0f;
                OnPullUp?.Invoke();
            }
        }

        private Vector3 GetEdge(Vector3 currentPosition, Vector3 targetGround, LayerMask layerMask)
        {
            const float heightOffset = 0.01f;
            var toTarget = targetGround - currentPosition;
            toTarget.y = 0;
            var ray = new Ray(new Vector3(currentPosition.x, targetGround.y - heightOffset, currentPosition.z), toTarget.normalized);
            var distance = toTarget.magnitude;
            if (Physics.Raycast(ray, out var hit, distance, layerMask))
            {
                const float safetyOffset = 0.01f;
                return new Vector3(hit.point.x, targetGround.y, hit.point.z) + toTarget.normalized * safetyOffset;
            }

            return targetGround;
        }
    }
}

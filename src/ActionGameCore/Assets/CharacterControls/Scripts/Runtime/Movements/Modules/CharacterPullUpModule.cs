using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CharacterControls.Movements.Modules
{
    public class CharacterPullUpModule : CharacterModuleBase
    {
        [SerializeField]
        public float CheckDistanceStart = 0.3f;

        [SerializeField]
        public float CheckDistanceEnd = 0.5f;

        [SerializeField]
        public int CheckCount = 4;

        [SerializeField]
        public float HeightMin = 0.5f;

        [SerializeField]
        public float HeightMax = 1.5f;

        [SerializeField]
        public float SlopeLimit = 45.0f;

        [SerializeField]
        public bool RequireAirborneState = true;

        [SerializeField]
        public bool RequireFallingState = true;

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

        public Vector3 LastMoveDirection { get; private set; }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var request in _stopRequests)
            {
                request?.Dispose();
            }
            _stopRequests.Clear();
        }

        public override void FixedUpdateModule(in CharacterMoveModulePayload payload)
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

            // Check stop requests
            if (_stopRequests.Count > 0)
            {
                return;
            }

            // Check airborne state
            if (RequireAirborneState && payload.Controller.IsGrounded)
            {
                return;
            }

            // Check falling state
            if (RequireFallingState && Vector3.Dot(payload.Rigidbody.velocity, payload.Root.up) > 0f)
            {
                return;
            }

            if (!payload.Controller.TryGetModule<CharacterWalkModule>(out var walkModule))
            {
                return;
            }

            var velocity = walkModule.TargetVelocity;
            var verticalVelocity = Vector3.Project(velocity, payload.Root.up);
            var moveDirection = (velocity - verticalVelocity).normalized;
            if (moveDirection.sqrMagnitude == 0f)
            {
                return;
            }

            var ray = new Ray(payload.Root.position + moveDirection * CheckDistanceStart + payload.Root.up * HeightMax, -payload.Root.up);
            var lineLength = CheckDistanceEnd - CheckDistanceStart;
            var verticalDistance = HeightMax - HeightMin;
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
                _stopRequests.Add(walkModule.RequestStopMove());
                if (payload.Controller.TryGetModule<CharacterJumpModule>(out var jumpModule))
                {
                    _stopRequests.Add(jumpModule.RequestStopJump());
                }
                LastMoveDirection = moveDirection;
                var heightRatio = Mathf.InverseLerp(HeightMin, HeightMax, grabHeight);
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

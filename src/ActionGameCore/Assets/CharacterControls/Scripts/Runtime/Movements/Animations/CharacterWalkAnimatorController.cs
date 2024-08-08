using System.Collections.Generic;
using CharacterControls.Movements.Modules;
using UnityEngine;

namespace CharacterControls.Movements.Animations
{
    public class CharacterWalkAnimatorController : MonoBehaviour
    {
        [SerializeField]
        public Transform ModelRoot = null;

        [SerializeField]
        public Animator Animator = null;

        [SerializeField]
        public CharacterMoveController MoveController = null;

        [SerializeField]
        public CharacterWalkModule Module;

        [SerializeField]
        public float RotateSpeedOnGround = 360.0f;

        [SerializeField]
        public float RotateSpeedInAir = 45.0f;

        [SerializeField]
        public string SpeedAnimatorParameter = "Speed";

        [SerializeField]
        public float SpeedMultiplier = 1.0f;

        [SerializeField]
        public float SpeedSmoothing = 0.1f;

        [SerializeField]
        public string GroundedAnimatorParameter = "IsGrounded";

        private float _smoothedSpeed;

        [SerializeField]
        public List<Collider> IgnoreCollidersForIK = default;

        private Vector3 _leftDiffPos;
        private Quaternion _leftDiffRot;
        private Vector3 _rightDiffPos;
        private Quaternion _rightDiffRot;

        private void Reset()
        {
            Animator = GetComponentInChildren<Animator>();
            ModelRoot = Animator == null ? transform : Animator.transform;
            MoveController = GetComponent<CharacterMoveController>();
            Module = GetComponent<CharacterWalkModule>();
        }

        private void Update()
        {
            if (MoveController == null || Module == null)
            {
                return;
            }

            UpdateRotation();
            UpdateWalkAnimation();
            UpdateAirAnimation();
        }

        private void UpdateRotation()
        {
            var velocity = Module.TargetVelocity;
            var verticalVelocity = Vector3.Project(velocity, ModelRoot.up);
            var horizontalVelocity = velocity - verticalVelocity;
            if (horizontalVelocity.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(horizontalVelocity, ModelRoot.up);
                var rotateSpeed = MoveController.IsGrounded ? RotateSpeedOnGround : RotateSpeedInAir;
                var targetSpeed = MoveController.IsGrounded ? Module.WalkSpeed : Module.AirWalkSpeedMax;
                var speedRatio = Mathf.InverseLerp(0, targetSpeed, horizontalVelocity.magnitude);
                ModelRoot.rotation = Quaternion.RotateTowards(ModelRoot.rotation, targetRotation, rotateSpeed * speedRatio * Time.deltaTime);
            }
        }

        private void UpdateWalkAnimation()
        {
            var velocity = Module.RelativeVelocityToGround;
            var verticalVelocity = Vector3.Project(velocity, ModelRoot.up);
            var horizontalVelocity = velocity - verticalVelocity;
            _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, horizontalVelocity.magnitude, SpeedSmoothing);
            Animator.SetFloat(SpeedAnimatorParameter, _smoothedSpeed * SpeedMultiplier);
        }

        private void UpdateAirAnimation()
        {
            Animator.SetBool(GroundedAnimatorParameter, MoveController.IsGrounded);
        }

        private void OnAnimatorIK()
        {
            UpdateFootIK(AvatarIKGoal.LeftFoot, Animator.leftFeetBottomHeight, ref _leftDiffPos, ref _leftDiffRot);
            UpdateFootIK(AvatarIKGoal.RightFoot, Animator.rightFeetBottomHeight, ref _rightDiffPos, ref _rightDiffRot);
        }

        private void UpdateFootIK(AvatarIKGoal ikGoal, float bottomHeight, ref Vector3 diffPos, ref Quaternion diffRot)
        {
            const float raycastDistance = 0.4f;
            const float footAngleLimit = 180.0f;
            const float toesLength = 0.3f;
            const float moveSpeed = 0.5f;
            const float angleSpeed = 180f;

            var footPos = Animator.GetIKPosition(ikGoal);
            var footRot = Animator.GetIKRotation(ikGoal);
            var toesPos = footPos + footRot * Vector3.forward * toesLength;

            var footRay = new Ray(footPos + footRot * Vector3.up * raycastDistance, footRot * Vector3.down);
            var toesRay = new Ray(footPos, footRot * Vector3.forward);
            var footIKPos = footPos;
            var footIKRot = footRot;
            if (Physics.Raycast(footRay, out var footHitInfo, raycastDistance + bottomHeight + 0.01f) && !IgnoreCollidersForIK.Contains(footHitInfo.collider))
            {
                footIKPos = footHitInfo.point + footHitInfo.normal * bottomHeight;
                var toesHeight = Vector3.Dot(toesPos - footIKPos, footHitInfo.normal);
                var toesIKPos = toesHeight > 0 ? toesPos : toesPos + footHitInfo.normal * -toesHeight;
                footIKRot = Quaternion.FromToRotation(footRot * Vector3.forward, (toesIKPos - footIKPos).normalized) * footRot;
            }
            else if (Physics.Raycast(toesRay, out var toesHitInfo, toesLength) && !IgnoreCollidersForIK.Contains(toesHitInfo.collider))
            {
                var toesHeight = Vector3.Dot(toesPos - toesHitInfo.point, toesHitInfo.normal);
                var toesIKPos = toesHeight > 0 ? toesPos : toesPos + toesHitInfo.normal * -toesHeight;
                footIKRot = Quaternion.FromToRotation(footRot * Vector3.forward, (toesIKPos - footPos).normalized) * footRot;
            }

            diffPos = Vector3.MoveTowards(diffPos, footIKPos - footPos, moveSpeed * Time.deltaTime);
            Animator.SetIKPositionWeight(ikGoal, 1);
            Animator.SetIKPosition(ikGoal, footPos + diffPos);
            var footIKDiffRot = footIKRot * Quaternion.Inverse(footRot);
            diffRot = Quaternion.RotateTowards(diffRot, footIKDiffRot, angleSpeed * Time.deltaTime);
            var rotW = 1 - Quaternion.Angle(Quaternion.identity, diffRot) / footAngleLimit;
            Animator.SetIKRotationWeight(ikGoal, rotW);
            Animator.SetIKRotation(ikGoal, diffRot * footRot);
        }
    }
}

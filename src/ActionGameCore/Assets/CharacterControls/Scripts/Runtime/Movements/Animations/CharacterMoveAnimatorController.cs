using System.Collections.Generic;
using UnityEngine;

namespace CharacterControls.Movements.Animations
{
    public class CharacterMoveAnimatorController : MonoBehaviour
    {
        [SerializeField]
        public Transform ModelRoot = null;

        [SerializeField]
        public Animator Animator = null;

        [SerializeField]
        public CharacterMoveController CharacterMoveController = null;

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

        private void Reset()
        {
            Animator = GetComponentInChildren<Animator>();
            ModelRoot = Animator == null ? transform : Animator.transform;
            CharacterMoveController = GetComponentInParent<CharacterMoveController>();
        }

        private void Update()
        {
            if (CharacterMoveController == null || Animator == null)
            {
                return;
            }

            UpdateRotation();
            UpdateWalkAnimation();
            UpdateAirAnimation();
        }

        private void UpdateRotation()
        {
            var velocity = CharacterMoveController.TargetVelocity;
            var verticalVelocity = Vector3.Project(velocity, ModelRoot.up);
            var horizontalVelocity = velocity - verticalVelocity;
            if (horizontalVelocity.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(horizontalVelocity, ModelRoot.up);
                var rotateSpeed = CharacterMoveController.IsGrounded ? RotateSpeedOnGround : RotateSpeedInAir;
                var targetSpeed = CharacterMoveController.IsGrounded ? CharacterMoveController.WalkSpeed : CharacterMoveController.AirWalkSpeed;
                var speedRatio = Mathf.InverseLerp(0, targetSpeed, horizontalVelocity.magnitude);
                ModelRoot.rotation = Quaternion.RotateTowards(ModelRoot.rotation, targetRotation, rotateSpeed * speedRatio * Time.deltaTime);
            }
        }

        private void UpdateWalkAnimation()
        {
            var velocity = CharacterMoveController.RelativeVelocityToGround;
            var verticalVelocity = Vector3.Project(velocity, ModelRoot.up);
            var horizontalVelocity = velocity - verticalVelocity;
            _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, horizontalVelocity.magnitude, SpeedSmoothing);
            Animator.SetFloat(SpeedAnimatorParameter, _smoothedSpeed * SpeedMultiplier);
        }

        private void UpdateAirAnimation()
        {
            Animator.SetBool(GroundedAnimatorParameter, CharacterMoveController.IsGrounded);
        }

        private void OnAnimatorIK()
        {
            UpdateFootIK(HumanBodyBones.LeftFoot, AvatarIKGoal.LeftFoot, Animator.leftFeetBottomHeight);
            UpdateFootIK(HumanBodyBones.RightFoot, AvatarIKGoal.RightFoot, Animator.rightFeetBottomHeight);
        }

        private void UpdateFootIK(HumanBodyBones bone, AvatarIKGoal ikGoal, float bottomHeight)
        {
            const float raycastBeforeDistance = 0.4f;
            const float footAngleLimit = 180.0f;
            var foot = Animator.GetBoneTransform(bone);
            var footWorldRot = Animator.GetIKRotation(ikGoal);
            var ray = new Ray(foot.position + footWorldRot * Vector3.up * raycastBeforeDistance, footWorldRot * Vector3.down);
            if (Physics.Raycast(ray, out var hitInfo, raycastBeforeDistance + bottomHeight)
                && !IgnoreCollidersForIK.Contains(hitInfo.collider))
            {
                var diffRot = Quaternion.FromToRotation(-foot.up, hitInfo.normal);
                var angleDiff = Quaternion.Angle(footWorldRot, diffRot * footWorldRot);
                var rotW = 1 - Mathf.InverseLerp(0, footAngleLimit, angleDiff);
                Animator.SetIKRotationWeight(ikGoal, rotW);
                Animator.SetIKRotation(ikGoal, diffRot * footWorldRot);

                Animator.SetIKPositionWeight(ikGoal, 1);
                Animator.SetIKPosition(ikGoal, hitInfo.point + hitInfo.normal * bottomHeight);
            }
            else
            {
                Animator.SetIKPositionWeight(ikGoal, 0);
                Animator.SetIKRotationWeight(ikGoal, 0);
            }
        }
    }
}

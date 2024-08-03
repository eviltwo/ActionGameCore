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
    }
}

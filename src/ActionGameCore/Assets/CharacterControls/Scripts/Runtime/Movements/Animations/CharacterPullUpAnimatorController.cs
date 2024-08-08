using CharacterControls.Movements.Modules;
using UnityEngine;

namespace CharacterControls.Movements.Animations
{
    public class CharacterPullUpAnimatorController : MonoBehaviour
    {
        [SerializeField]
        public Transform ModelRoot = null;

        [SerializeField]
        public Animator Animator = null;

        [SerializeField]
        public CharacterPullUpModule Module = null;

        [SerializeField]
        public float PullUpStartTimeMax = 0.9f;

        [SerializeField]
        public string PullUpState = "PullUp";

        private void Reset()
        {
            Animator = GetComponentInChildren<Animator>();
            ModelRoot = Animator == null ? transform : Animator.transform;
            Module = GetComponentInParent<CharacterPullUpModule>();
        }

        private void Start()
        {
            if (Module != null)
            {
                Module.OnPullUp.AddListener(OnPullUp);
            }
        }

        private void OnDestroy()
        {
            if (Module != null)
            {
                Module.OnPullUp.RemoveListener(OnPullUp);
            }
        }

        private void OnPullUp()
        {
            ModelRoot.rotation = Quaternion.LookRotation(Module.LastMoveDirection, ModelRoot.up);
            var durationRatio = Mathf.InverseLerp(Module.StopMoveDurationMin, Module.StopMoveDurationMax, Module.StopDuration);
            var motionTime = Mathf.Lerp(0f, PullUpStartTimeMax, durationRatio);
            Animator.Play(PullUpState, 0, motionTime);
        }
    }
}

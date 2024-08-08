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
        public string PullUpAnimatorParameterTrigger = "OnPullUp";

        [SerializeField]
        public int AnimatorRefreshCount = 3;

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
            Animator.SetTrigger(PullUpAnimatorParameterTrigger);
            for (int i = 0; i < AnimatorRefreshCount; i++)
            {
                Animator.Update(0);
            }
        }
    }
}

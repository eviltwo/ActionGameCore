using CharacterControls.Movements.Modules;
using UnityEngine;

namespace CharacterControls.Movements.Animations
{
    public class CharacterJumpAnimatorController : MonoBehaviour
    {
        [SerializeField]
        public Animator Animator = null;

        [SerializeField]
        public CharacterJumpModule JumpModule = null;

        [SerializeField]
        public string JumpAnimatorParameterTrigger = "OnJump";

        private void Reset()
        {
            Animator = GetComponentInChildren<Animator>();
            JumpModule = GetComponentInParent<CharacterJumpModule>();
        }

        private void Start()
        {
            if (!JumpModule)
            {
                JumpModule.OnJump.AddListener(OnJump);
            }
        }

        private void OnDestroy()
        {
            if (!JumpModule)
            {
                JumpModule.OnJump.RemoveListener(OnJump);
            }
        }

        private void OnJump()
        {
            if (!Animator)
            {
                return;
            }

            Animator.SetTrigger(JumpAnimatorParameterTrigger);
        }
    }
}

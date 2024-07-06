#if ENABLE_INPUT_SYSTEM
using CharacterControls.Movements;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterControls.Inputs
{
    public class CharacterMoveInput : MonoBehaviour
    {
        [SerializeField]
        public PlayerInput PlayerInput = null;

        [SerializeField]
        private InputActionReference _moveActionReference = null;

        [SerializeField]
        private InputActionReference _jumpActionReference = null;

        public IMoveController MoveController { get; set; }

        private void Start()
        {
            MoveController = GetComponent<IMoveController>();
            PlayerInput.onActionTriggered += OnActionTriggerd;
        }

        private void OnDestroy()
        {
            PlayerInput.onActionTriggered -= OnActionTriggerd;
        }

        private void OnActionTriggerd(InputAction.CallbackContext context)
        {
            if (_moveActionReference != null && context.action.name == _moveActionReference.action.name)
            {
                OnMove(context);
            }

            if (_jumpActionReference != null && context.action.name == _jumpActionReference.action.name)
            {
                OnJump(context);
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<Vector2>();
            MoveController.SetMoveInput(value);
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && context.startTime > Time.realtimeSinceStartup - 0.5f)
            {
                MoveController.SetJumpInput(1);
            }
        }
    }
}
#endif

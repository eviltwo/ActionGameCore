#if SUPPORT_INPUTSYSTEM
using CharacterControls.Movements;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterControls.Inputs
{
    public class CharacterMoveInput : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset _inputActionAsset;

        public IMoveController MoveController { get; set; }

        private InputAction _moveAction;

        private InputAction _jumpAction;

        private void Start()
        {
            _inputActionAsset.Enable();
            var map = _inputActionAsset.FindActionMap("Player");
            _moveAction = map.FindAction("Move");
            _jumpAction = map.FindAction("Jump");

            MoveController = GetComponent<IMoveController>();
        }

        private void Update()
        {
            if (_inputActionAsset == null || MoveController == null)
            {
                return;
            }

            MoveController.SetMoveInput(_moveAction.ReadValue<Vector2>());

            if (_jumpAction.WasPressedThisFrame())
            {
                MoveController.SetJumpInput(1.0f);
            }
        }
    }
}
#endif

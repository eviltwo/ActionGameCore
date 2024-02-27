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

        private void Start()
        {
            _inputActionAsset.Enable();
            var map = _inputActionAsset.FindActionMap("Player");
            _moveAction = map.FindAction("Move");

            MoveController = GetComponent<IMoveController>();
        }

        private void Update()
        {
            if (_inputActionAsset == null || MoveController == null)
            {
                return;
            }

            var value = _moveAction.ReadValue<Vector2>();
            MoveController.SetMoveInput(value);
        }
    }
}
#endif

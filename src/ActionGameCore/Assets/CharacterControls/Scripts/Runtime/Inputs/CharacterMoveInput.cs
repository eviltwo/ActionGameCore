using CharacterControls.Movements;
using UnityEngine;

#if SUPPORT_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif

namespace CharacterControls.Inputs
{
    public class CharacterMoveInput : MonoBehaviour
    {
#if SUPPORT_INPUTSYSTEM
        [Header("InputSystem")]
        [SerializeField]
        private InputActionReference _moveActionReference = null;

        [SerializeField]
        private InputActionReference _jumpActionReference = null;
#endif

        public IMoveController MoveController { get; set; }

        private void Start()
        {
#if SUPPORT_INPUTSYSTEM
            _moveActionReference?.action.Enable();
            _jumpActionReference?.action.Enable();
            MoveController = GetComponent<IMoveController>();
#endif
        }

        private void Update()
        {
            if (MoveController == null)
            {
                return;
            }

            UpdateMoveInput();
            UpdateJumpInput();
        }

        private void UpdateMoveInput()
        {
            var value = Vector2.zero;

#if SUPPORT_INPUTSYSTEM
            if (_moveActionReference != null)
            {
                var v = _moveActionReference.action.ReadValue<Vector2>();
                value = v.sqrMagnitude > value.sqrMagnitude ? v : value;
            }
#endif

            MoveController.SetMoveInput(value);
        }

        private void UpdateJumpInput()
        {
            var value = false;

#if SUPPORT_INPUTSYSTEM
            value |= _jumpActionReference != null && _jumpActionReference.action.WasPressedThisFrame();
#endif

            if (value)
            {
                MoveController.SetJumpInput(1);
            }
        }
    }
}

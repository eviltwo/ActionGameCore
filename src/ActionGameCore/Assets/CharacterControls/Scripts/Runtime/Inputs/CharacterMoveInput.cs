#if SUPPORT_INPUTSYSTEM
using CharacterControls.Movements;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterControls.Inputs
{
    public class CharacterMoveInput : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference _moveActionReference = null;

        [SerializeField]
        private InputActionReference _jumpActionReference = null;

        public IMoveController MoveController { get; set; }

        private void Start()
        {
            MoveController = GetComponent<IMoveController>();
        }

        private void Update()
        {
            if (MoveController == null)
            {
                return;
            }

            if (_moveActionReference != null)
            {
                MoveController.SetMoveInput(_moveActionReference.action.ReadValue<Vector2>());
            }

            if (_jumpActionReference != null && _jumpActionReference.action.WasPressedThisFrame())
            {
                MoveController.SetJumpInput(1.0f);
            }
        }
    }
}
#endif

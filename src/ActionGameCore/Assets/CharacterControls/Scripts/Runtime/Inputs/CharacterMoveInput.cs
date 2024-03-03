#if SUPPORT_INPUTSYSTEM
using CharacterControls.Movements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace CharacterControls.Inputs
{
    public class CharacterMoveInput : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset _inputActionAsset;

        [SerializeField]
        private string _actionMapName = "Player";

        [SerializeField]
        private string _moveActionName = "Move";

        [SerializeField]
        private string _jumpActionName = "Jump";

        public IMoveController MoveController { get; set; }

        private InputAction _moveAction;

        private InputAction _jumpAction;

        private void Start()
        {
            Assert.IsNotNull(_inputActionAsset, "InputActionAsset is not set");
            if (_inputActionAsset != null)
            {
                _inputActionAsset.Enable();
                var map = _inputActionAsset.FindActionMap(_actionMapName);
                Assert.IsNotNull(map, $"Action map {_actionMapName} not found");
                if (map != null)
                {
                    _moveAction = map.FindAction(_moveActionName);
                    _jumpAction = map.FindAction(_jumpActionName);
                    Assert.IsNotNull(_moveAction, $"Action {_moveActionName} not found in {_actionMapName}");
                    Assert.IsNotNull(_jumpAction, $"Action {_jumpActionName} not found in {_actionMapName}");
                }
            }

            MoveController = GetComponent<IMoveController>();
        }

        private void Update()
        {
            if (MoveController == null)
            {
                return;
            }

            if (_moveAction != null)
            {
                MoveController.SetMoveInput(_moveAction.ReadValue<Vector2>());
            }

            if (_jumpAction != null && _jumpAction.WasPressedThisFrame())
            {
                MoveController.SetJumpInput(1.0f);
            }
        }
    }
}
#endif

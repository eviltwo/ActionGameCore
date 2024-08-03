#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
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

        public List<IInputReceiver<Vector2>> _vector2Receivers = new List<IInputReceiver<Vector2>>();
        public List<IInputReceiver<float>> _floatReceivers = new List<IInputReceiver<float>>();

        private void Start()
        {
            PlayerInput.onActionTriggered += OnActionTriggerd;
        }

        private void OnDestroy()
        {
            PlayerInput.onActionTriggered -= OnActionTriggerd;
        }

        private void Update()
        {
            _vector2Receivers.Clear();
            GetComponentsInChildren(_vector2Receivers);
            _floatReceivers.Clear();
            GetComponentsInChildren(_floatReceivers);
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
            var count = _vector2Receivers.Count;
            for (var i = 0; i < count; i++)
            {
                _vector2Receivers[i].OnReceiveInput("Move", value);
            }
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            var count = _floatReceivers.Count;
            for (var i = 0; i < count; i++)
            {
                _floatReceivers[i].OnReceiveInput("Jump", context.performed ? 1f : 0f);
            }
        }
    }
}
#endif

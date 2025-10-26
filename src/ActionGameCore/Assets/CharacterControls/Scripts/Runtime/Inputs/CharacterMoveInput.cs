#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterControls.Inputs
{
    public class CharacterMoveInput : MonoBehaviour
    {
        [SerializeField]
        public PlayerInput PlayerInput = null;

        private readonly List<IInputReceiver> _receivers = new List<IInputReceiver>();

        private void OnEnable()
        {
            if (PlayerInput != null)
            {
                PlayerInput.onActionTriggered += OnActionTriggerd;
            }
        }

        private void OnDisable()
        {
            if (PlayerInput != null)
            {
                PlayerInput.onActionTriggered -= OnActionTriggerd;
            }
        }

        public void RegisterReceiver(IInputReceiver receiver)
        {
            _receivers.Add(receiver);
        }

        public void UnregisterReceiver(IInputReceiver receiver)
        {
            _receivers.Remove(receiver);
        }

        private void OnActionTriggerd(InputAction.CallbackContext context)
        {
            foreach (var receiver in _receivers)
            {
                receiver.OnReceiveInput(new InputContext(context));
            }
        }
    }
}
#endif

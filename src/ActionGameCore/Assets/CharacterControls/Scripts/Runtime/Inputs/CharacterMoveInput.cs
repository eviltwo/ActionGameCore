#if ENABLE_INPUT_SYSTEM
using System;
using System.Collections.Generic;
using CharacterControls.Movements;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterControls.Inputs
{
    public class CharacterMoveInput : MonoBehaviour
    {
        [Serializable]
        private class InputActionKeyMap
        {
            public string CharacterActionKey = string.Empty;
            public InputActionReference InputActionReference = null;
        }

        [SerializeField]
        public PlayerInput PlayerInput = null;

        [SerializeField]
        private InputActionKeyMap[] _floatActionKeyMaps = new InputActionKeyMap[]
        {
            new InputActionKeyMap
            {
                CharacterActionKey = "Jump",
                InputActionReference = null,
            }
        };

        [SerializeField]
        private InputActionKeyMap[] _vector2ActionKeyMaps = new InputActionKeyMap[]
        {
            new InputActionKeyMap
            {
                CharacterActionKey = "Move",
                InputActionReference = null,
            }
        };

        public List<IInputReceiver<float>> _floatReceivers = new List<IInputReceiver<float>>();
        public List<IInputReceiver<Vector2>> _vector2Receivers = new List<IInputReceiver<Vector2>>();

        private void Start()
        {
            CollectReceivers();
            if (PlayerInput != null)
            {
                PlayerInput.onActionTriggered += OnActionTriggerd;
            }
        }

        private void OnDestroy()
        {
            if (PlayerInput != null)
            {
                PlayerInput.onActionTriggered -= OnActionTriggerd;
            }
        }

        public void CollectReceivers()
        {
            _vector2Receivers.Clear();
            GetComponentsInChildren(_vector2Receivers);
            _floatReceivers.Clear();
            GetComponentsInChildren(_floatReceivers);
        }

        private void OnActionTriggerd(InputAction.CallbackContext context)
        {
            SendValue(context, _floatActionKeyMaps, _floatReceivers);
            SendValue(context, _vector2ActionKeyMaps, _vector2Receivers);
        }

        private void SendValue<T>(InputAction.CallbackContext context, InputActionKeyMap[] maps, List<IInputReceiver<T>> receivers)
            where T : struct
        {
            var mapCount = maps.Length;
            var receiverCount = receivers.Count;
            for (int i = 0; i < mapCount; i++)
            {
                var map = maps[i];
                if (map.InputActionReference != null && context.action.name == map.InputActionReference.action.name)
                {
                    for (int j = 0; j < receiverCount; j++)
                    {
                        receivers[j].OnReceiveInput(map.CharacterActionKey, context.ReadValue<T>());
                    }
                }
            }
        }
    }
}
#endif

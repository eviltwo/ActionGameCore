#if SUPPORT_INPUTSYSTEM
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Interactions
{
    public class InteractSystem : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference[] _interactActionReferences = null;

        [Serializable]
        public class ButtonInteractEvent : UnityEvent<InteractEventData> { }

        [SerializeField]
        public ButtonInteractEvent InteractEvent;

        private List<GameObject> _findBuffer = new List<GameObject>();

        private List<IInteractHandler> _pointerOverInteractables = new List<IInteractHandler>();

        public IReadOnlyList<IInteractHandler> PointerOverInteractables => _pointerOverInteractables;

        private void Update()
        {
            _findBuffer.Clear();
            var finders = InteractObjectFinderManager.GetObjectFinders();
            var finderCount = finders.Count;
            for (int i = 0; i < finderCount; i++)
            {
                var finder = finders[i];
                finder.Find(_findBuffer);
            }

            _pointerOverInteractables.Clear();
            if (_findBuffer.Count > 0)
            {
                var result = _findBuffer[0];
                result.gameObject.GetComponents(_pointerOverInteractables);
            }

            var actionCount = _interactActionReferences.Length;
            for (int i = 0; i < actionCount; i++)
            {
                var action = _interactActionReferences[i]?.action;
                if (action != null && action.WasPressedThisFrame())
                {
                    var handlerCount = _pointerOverInteractables.Count;
                    for (int j = 0; j < handlerCount; j++)
                    {
                        var interactable = _pointerOverInteractables[j];
                        var filter = interactable.InputActionNameFilter;
                        if (string.IsNullOrEmpty(filter) || filter == action.name)
                        {
                            var eventData = new InteractEventData
                            {
                                InputActionName = action.name,
                                InteractedObject = interactable,
                            };
                            interactable.OnInteract(eventData);
                            InteractEvent?.Invoke(eventData);
                        }
                    }
                }
            }
        }
    }
}
#endif

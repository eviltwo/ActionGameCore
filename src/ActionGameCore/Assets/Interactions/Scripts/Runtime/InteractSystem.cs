#if SUPPORT_INPUTSYSTEM
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Interactions
{
    public class InteractSystem : MonoBehaviour
    {
        [SerializeField]
        public BaseRaycaster Raycaster = null;

        [SerializeField]
        private InputActionReference[] _interactActionReferences = null;

        [Serializable]
        public class ButtonInteractEvent : UnityEvent<InteractEventData> { }

        [SerializeField]
        public ButtonInteractEvent InteractEvent;

        private List<RaycastResult> _resultBuffer = new List<RaycastResult>();

        private List<IInteractHandler> _pointerOverInteractables = new List<IInteractHandler>();

        public IReadOnlyList<IInteractHandler> PointerOverInteractables => _pointerOverInteractables;

        private static PointerEventData DummyPointerEventData => new PointerEventData(null);

        private void Reset()
        {
            Raycaster = FindObjectOfType<BaseRaycaster>();
        }

        private void Start()
        {
            Assert.IsNotNull(Raycaster, "Raycaster is not set.");
        }

        private void Update()
        {
            if (Raycaster == null)
            {
                return;
            }

            _pointerOverInteractables.Clear();
            _resultBuffer.Clear();
            Raycaster.Raycast(DummyPointerEventData, _resultBuffer);
            if (_resultBuffer.Count > 0)
            {
                var result = _resultBuffer[0];
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

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

        [SerializeField]
        public UnityEvent<ISelectHandler> SelectEvent = null;

        [SerializeField]
        public UnityEvent<ISelectHandler> DeselectEvent = null;

        [Serializable]
        public class ButtonInteractEvent : UnityEvent<InteractEventData> { }

        [SerializeField]
        public ButtonInteractEvent InteractEvent;

        private List<GameObject> _findBuffer = new List<GameObject>();

        private List<ISelectHandler> _selectedObjects = new List<ISelectHandler>();
        private List<ISelectHandler> _selectedObjectsTemp = new List<ISelectHandler>();

        public IReadOnlyList<ISelectHandler> SelectedObjects => _selectedObjects;

        private void Start()
        {
            foreach (var actionReference in _interactActionReferences)
            {
                actionReference?.action.Enable();
            }
        }

        private void Update()
        {
            // Keep last selected objects
            _selectedObjectsTemp.Clear();
            _selectedObjectsTemp.AddRange(_selectedObjects);

            // Collect selectable objects
            _findBuffer.Clear();
            var finders = InteractObjectFinderManager.GetObjectFinders();
            var finderCount = finders.Count;
            for (int i = 0; i < finderCount; i++)
            {
                var finder = finders[i];
                finder.Find(_findBuffer);
            }

            _selectedObjects.Clear();
            if (_findBuffer.Count > 0)
            {
                var result = _findBuffer[0];
                result.gameObject.GetComponents(_selectedObjects);
            }

            // Execute OnDeselect
            var lastCount = _selectedObjectsTemp.Count;
            for (int i = 0; i < lastCount; i++)
            {
                var lastSelected = _selectedObjectsTemp[i];
                if (!_selectedObjects.Contains(lastSelected))
                {
                    lastSelected.OnDeselect();
                    DeselectEvent?.Invoke(lastSelected);
                }
            }

            // Execute OnSelect
            var selectedCount = _selectedObjects.Count;
            for (int i = 0; i < selectedCount; i++)
            {
                var selected = _selectedObjects[i];
                if (!_selectedObjectsTemp.Contains(selected))
                {
                    selected.OnSelect();
                    SelectEvent?.Invoke(selected);
                }
            }

            // Execute OnInteract
            var actionCount = _interactActionReferences.Length;
            for (int i = 0; i < actionCount; i++)
            {
                var action = _interactActionReferences[i]?.action;
                if (action != null && action.WasPressedThisFrame())
                {
                    for (int j = 0; j < selectedCount; j++)
                    {
                        if (_selectedObjects[j] is IInteractHandler interactable)
                        {
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
}
#endif

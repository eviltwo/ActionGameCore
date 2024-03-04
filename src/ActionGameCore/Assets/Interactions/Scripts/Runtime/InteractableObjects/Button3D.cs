using System;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions
{
    public class Button3D : Selectable3D, IInteractHandler
    {
        [SerializeField]
        public string InputActionName = "Interact";

        public string InputActionNameFilter => InputActionName;

        [Serializable]
        public class ButtonInteractEvent : UnityEvent<InteractEventData> { }

        [SerializeField]
        public ButtonInteractEvent InteractEvent;

        public void OnInteract(InteractEventData eventData)
        {
            InteractEvent?.Invoke(eventData);
        }
    }
}

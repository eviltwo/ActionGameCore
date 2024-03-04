using System;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions
{
    public class Button3D : Selectable3D, IInteractHandler
    {
        [SerializeField]
        public string InteractActionNameFilter = "Interact";

        public string InputActionNameFilter => InteractActionNameFilter;

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

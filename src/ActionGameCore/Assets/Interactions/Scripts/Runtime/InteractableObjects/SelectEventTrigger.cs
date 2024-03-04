using UnityEngine;
using UnityEngine.Events;

namespace Interactions
{
    public class SelectEventTrigger : MonoBehaviour, ISelectHandler
    {
        [SerializeField]
        private UnityEvent SelectEvent = null;

        [SerializeField]
        private UnityEvent DeselectEvent = null;

        public void OnSelect()
        {
            SelectEvent?.Invoke();
        }

        public void OnDeselect()
        {
            DeselectEvent?.Invoke();
        }
    }
}

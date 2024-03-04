using UnityEngine;
using UnityEngine.EventSystems;

namespace Interactions
{
    public class Selectable3D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _hasSelection;

        public bool IsHighlighted()
        {
            return _hasSelection;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _hasSelection = true;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            _hasSelection = false;
        }
    }
}

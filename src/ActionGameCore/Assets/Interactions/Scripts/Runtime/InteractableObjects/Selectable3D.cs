using UnityEngine;

namespace Interactions
{
    public abstract class Selectable3D : MonoBehaviour, ISelectHandler
    {
        public virtual void OnSelect() { }

        public virtual void OnDeselect() { }
    }
}

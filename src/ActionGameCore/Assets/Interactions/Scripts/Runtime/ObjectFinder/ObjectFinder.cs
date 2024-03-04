using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    public abstract class ObjectFinder : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            ObjectFinderManager.Add(this);
        }

        protected virtual void OnDisable()
        {
            ObjectFinderManager.Remove(this);
        }

        public abstract void Find(List<GameObject> resultAppendList);
    }
}

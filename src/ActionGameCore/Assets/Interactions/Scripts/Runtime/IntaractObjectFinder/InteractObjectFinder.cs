using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    public abstract class InteractObjectFinder : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            InteractObjectFinderManager.Add(this);
        }

        protected virtual void OnDisable()
        {
            InteractObjectFinderManager.Remove(this);
        }

        public abstract void Find(List<GameObject> resultAppendList);
    }
}

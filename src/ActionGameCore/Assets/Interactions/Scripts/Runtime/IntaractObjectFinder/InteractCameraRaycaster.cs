using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Interact object finder for FPS game.
    /// </summary>
    public class InteractCameraRaycaster : InteractObjectFinder
    {
        [SerializeField]
        public Camera Camera = null;

        [SerializeField]
        public float MaxDistance = 10.0f;

        [SerializeField]
        public LayerMask LayerMask = -1;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[50];

        private class DistanceComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }

        private readonly DistanceComparer _distanceComparer = new DistanceComparer();

        private void Reset()
        {
            Camera = Camera.main;
        }

        public override void Find(List<GameObject> resultAppendList)
        {
            if (Camera == null)
            {
                return;
            }

            var ray = new Ray(Camera.transform.position, Camera.transform.forward);
            var hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, MaxDistance, LayerMask);

            if (hitCount > 1)
            {
                Array.Sort(_hitBuffer, 0, hitCount, _distanceComparer);
            }

            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitBuffer[i];
                resultAppendList.Add(hit.collider.gameObject);
            }
        }
    }
}

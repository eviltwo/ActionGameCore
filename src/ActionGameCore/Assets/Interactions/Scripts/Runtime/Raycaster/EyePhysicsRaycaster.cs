using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EyePhysicsRaycaster : PhysicsRaycaster
{
    private readonly RaycastHit[] _hitBuffer = new RaycastHit[50];

    private class DistanceComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }

    private readonly DistanceComparer _distanceComparer = new DistanceComparer();

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (eventCamera == null)
        {
            return;
        }

        var ray = new Ray(eventCamera.transform.position, eventCamera.transform.forward);
        var dist = eventCamera.farClipPlane;
        var hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, dist, finalEventMask);

        if (hitCount > 1)
        {
            Array.Sort(_hitBuffer, 0, hitCount, _distanceComparer);
        }

        for (int i = 0; i < hitCount; i++)
        {
            var hit = _hitBuffer[i];
            var result = new RaycastResult
            {
                gameObject = hit.collider.gameObject,
                module = this,
                distance = hit.distance,
                worldPosition = hit.point,
                worldNormal = hit.normal,
                screenPosition = eventData.position,
                index = resultAppendList.Count,
                sortingLayer = 0,
                sortingOrder = 0
            };
            resultAppendList.Add(result);
        }
    }
}

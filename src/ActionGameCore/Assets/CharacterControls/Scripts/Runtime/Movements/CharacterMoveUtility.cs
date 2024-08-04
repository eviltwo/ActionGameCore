using System.Collections.Generic;
using UnityEngine;

namespace CharacterControls.Movements
{
    public static class CharacterMoveUtility
    {
        private static List<Vector3> _offsetBuffer = new List<Vector3>();
        private static List<Vector3> _pointBuffer = new List<Vector3>();

        public static bool DrawDebug { get; set; } = false;

        public static bool CheckLineGroundSafety(Ray ray, float rayDistance, Vector3 lineDirection, float lineDistance, int resolution, float slopeLimit, out RaycastHit hit, LayerMask layerMask)
        {
            _offsetBuffer.Clear();
            for (int i = 0; i < resolution; i++)
            {
                var t = (float)i / (resolution - 1);
                _offsetBuffer.Add(Vector3.Lerp(Vector3.zero, lineDirection * lineDistance, t));
            }

            for (int i = 0; i < _offsetBuffer.Count; i++)
            {
                var offset = _offsetBuffer[i];
                var lineRay = new Ray(ray.origin + offset, ray.direction);
                if (Physics.Raycast(lineRay, out var lineHit, rayDistance, layerMask))
                {
                    var validSlope = Vector3.Angle(lineHit.normal, Vector3.up) < slopeLimit;

                    if (DrawDebug)
                    {
                        Debug.DrawLine(lineRay.origin, lineHit.point, validSlope ? Color.green : Color.red);
                    }

                    if (validSlope)
                    {
                        hit = lineHit;
                        return true;
                    }
                }
                else if (DrawDebug)
                {
                    Debug.DrawLine(lineRay.origin, lineRay.GetPoint(rayDistance), Color.yellow);
                }
            }

            hit = default;
            return false;
        }

        public static bool CheckCircleGroundSafety(Ray ray, float rayDistance, float radius, int resolution, float slopeLimit, out RaycastHit hit, LayerMask layerMask)
        {
            _offsetBuffer.Clear();
            _offsetBuffer.Add(Vector3.zero);
            for (int i = 0; i < resolution; i++)
            {
                var rad = 360f / resolution * i * Mathf.Deg2Rad;
                _offsetBuffer.Add(new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius);
            }

            var isHit = false;
            var isValidGround = false;
            RaycastHit closestHit = default;
            _pointBuffer.Clear();
            var totalNormal = Vector3.zero;
            var normalCount = 0;
            for (int i = 0; i < _offsetBuffer.Count; i++)
            {
                var offset = _offsetBuffer[i];
                var circleRay = new Ray(ray.origin + offset, ray.direction);
                if (Physics.Raycast(circleRay, out var circleHit, rayDistance, layerMask))
                {
                    var validSlope = Vector3.Angle(circleHit.normal, Vector3.up) < slopeLimit;

                    // Calculate surface normal
                    if (validSlope)
                    {
                        _pointBuffer.Add(circleHit.point);
                        if (_pointBuffer.Count >= 3)
                        {
                            for (int j = 0; j < _pointBuffer.Count - 2; j++)
                            {
                                var plane = new Plane(_pointBuffer[j], _pointBuffer[j + 1], _pointBuffer[_pointBuffer.Count - 1]);
                                var normal = plane.normal;
                                totalNormal += normal.y > 0 ? normal : -normal;
                                normalCount++;
                            }
                        }
                    }

                    // Check if the hit is the closest
                    if (!isHit || (validSlope && circleHit.distance < closestHit.distance))
                    {
                        isHit = true;
                        isValidGround = validSlope;
                        closestHit = circleHit;
                    }
                    if (DrawDebug)
                    {
                        Debug.DrawLine(circleRay.origin, circleHit.point, validSlope ? Color.green : Color.red);
                    }
                }
                else if (DrawDebug)
                {
                    Debug.DrawLine(circleRay.origin, circleRay.GetPoint(rayDistance), Color.yellow);
                }
            }

            hit = closestHit;
            if (normalCount > 0)
            {
                hit.normal = totalNormal / normalCount;
            }

            if (DrawDebug)
            {
                Debug.DrawRay(hit.point, hit.normal, Color.blue);
            }

            return isValidGround;
        }

        public static Vector3 GetForwardMovementDirectionFromCamera(Transform self, Transform cam)
        {
            if (cam == null)
            {
                return self.forward;
            }

            var forward = cam.forward;
            if (forward == self.up)
            {
                forward = -cam.up;
            }
            else if (forward == -self.up)
            {
                forward = cam.up;
            }

            return Vector3.ProjectOnPlane(forward, self.up).normalized;
        }
    }
}


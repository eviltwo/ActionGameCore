#if SUPPORT_INPUTSYSTEM
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSCameraControls
{
    public class FPSCameraController : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference _deltaActionReference = null;

        [SerializeField]
        private InputActionReference _continuousActionreference = null;

        [SerializeField]
        public Transform Target = null;

        [SerializeField]
        public float OffsetY = 1.5f;

        [SerializeField]
        public float Sensitivity = 1.0f;

        [SerializeField]
        public float AngleMin = -90.0f;

        [SerializeField]
        public float AngleMax = 90.0f;

        [SerializeField]
        public int SmoothingFrameCount = 2;

        [SerializeField]
        private bool _lockAndHideCursor = true;

        private Vector3 _lookAngles;
        private List<Vector2> _deltaPositions = new List<Vector2>();
        private List<float> _deltaTimes = new List<float>();

        private void Start()
        {
            _deltaActionReference?.action.Enable();
            _continuousActionreference?.action.Enable();
            if (_lockAndHideCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            var position = Target.position + new Vector3(0, OffsetY, 0);

            var deltaAngle = Vector2.zero;
            if (_deltaActionReference != null)
            {
                _deltaPositions.Add(_deltaActionReference.action.ReadValue<Vector2>());
                _deltaTimes.Add(Time.deltaTime);
                SmoothingFrameCount = Mathf.Max(1, SmoothingFrameCount);
                if (_deltaPositions.Count > SmoothingFrameCount)
                {
                    _deltaPositions.RemoveRange(0, _deltaPositions.Count - SmoothingFrameCount);
                    _deltaTimes.RemoveRange(0, _deltaTimes.Count - SmoothingFrameCount);
                }
                var totalPosition = Vector2.zero;
                var totalTime = 0f;
                for (int i = 0; i < _deltaPositions.Count; i++)
                {
                    totalPosition += _deltaPositions[i];
                    totalTime += _deltaTimes[i];
                }
                var deltaPositionAverage = totalPosition / totalTime;
                var smoothDeltaPosition = deltaPositionAverage * Time.deltaTime;

                const float DpiAverage = 96;
                var dpi = Screen.dpi == 0 ? DpiAverage : Screen.dpi;
                const float InchForTurn = 13;
                deltaAngle += smoothDeltaPosition / dpi / InchForTurn * 180;
            }

            if (_continuousActionreference != null)
            {
                const float SecondsForTurn = 1.0f;
                deltaAngle += _continuousActionreference.action.ReadValue<Vector2>() * Time.deltaTime / SecondsForTurn * 180;
            }

            _lookAngles.x = Mathf.Clamp(_lookAngles.x - deltaAngle.y * Sensitivity, -AngleMax, -AngleMin); // Look up and down
            _lookAngles.y = (_lookAngles.y + deltaAngle.x * Sensitivity) % 360; // Look left and right
            var rotation = Quaternion.Euler(_lookAngles);
            transform.SetPositionAndRotation(position, rotation);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < _deltaPositions.Count; i++)
            {
                var d = _deltaPositions[i].x;
                var basePos = new Vector3(i * 0.1f, 1, 0);
                Gizmos.DrawLine(basePos, basePos + Vector3.up * d);
            }
        }
    }
}
#endif

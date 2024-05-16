#if SUPPORT_INPUTSYSTEM
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
        private bool _lockAndHideCursor = true;

        private Vector3 _lookAngles;

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

            var rotation = Target.rotation;
            if (_deltaActionReference != null)
            {
                var deltaAngle = Vector2.zero;
                if (_continuousActionreference != null && _deltaActionReference.action.IsInProgress())
                {
                    const float DpiAverage = 96;
                    var dpi = Screen.dpi == 0 ? DpiAverage : Screen.dpi;
                    const float InchForTurn = 13;
                    deltaAngle = _deltaActionReference.action.ReadValue<Vector2>() / dpi / InchForTurn * 180;
                }
                else if (_continuousActionreference != null && _continuousActionreference.action.IsInProgress())
                {
                    const float SecondsForTurn = 1.0f;
                    deltaAngle = _continuousActionreference.action.ReadValue<Vector2>() * Time.deltaTime / SecondsForTurn * 180;
                }

                _lookAngles.x = Mathf.Clamp(_lookAngles.x - deltaAngle.y * Sensitivity, -AngleMax, -AngleMin); // Look up and down
                _lookAngles.y = (_lookAngles.y + deltaAngle.x * Sensitivity) % 360; // Look left and right
                rotation = Quaternion.Euler(_lookAngles);
            }

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
#endif

using System.Collections.Generic;
using CameraControls.Controllers;
using UnityEngine;

#if SUPPORT_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif


namespace CameraControls.Inputs
{
    public class CameraInput : MonoBehaviour
    {
#if SUPPORT_INPUTSYSTEM
        [Header("InputSystem")]
        [SerializeField]
        private InputActionReference _deltaActionReference = null;

        [SerializeField]
        private InputActionReference _continuousActionreference = null;
#endif 

        private ICameraController _cameraController;
        private List<ICameraController> _cameraControllerBuffer = new List<ICameraController>();

        private void Start()
        {
            _cameraController = GetComponent<ICameraController>();

#if SUPPORT_INPUTSYSTEM
            _deltaActionReference?.action.Enable();
            _continuousActionreference?.action.Enable();
#endif
        }

        private void Update()
        {
            if (_cameraController == null || !_cameraController.Enabled)
            {
                GetComponents(_cameraControllerBuffer);
                foreach (var controller in _cameraControllerBuffer)
                {
                    if (controller.Enabled)
                    {
                        _cameraController = controller;
                        break;
                    }
                }
                return;
            }

            var deltaAngle = Vector2.zero;

            // Digital input
            {
                var deltaPixels = GetDeltaPixels();
                const float DpiAverage = 96;
                var dpi = Screen.dpi == 0 ? DpiAverage : Screen.dpi;
                const float InchForTurn = 13;
                var v = deltaPixels / dpi / InchForTurn * 180;
                deltaAngle = v.sqrMagnitude > deltaAngle.sqrMagnitude ? v : deltaAngle;
            }

            // Analog input
            {
                var analogValues = GetAnalogValues();
                const float SecondsForTurn = 1.0f;
                var v = analogValues * Time.deltaTime / SecondsForTurn * 180;
                deltaAngle = v.sqrMagnitude > deltaAngle.sqrMagnitude ? v : deltaAngle;
            }

            _cameraController.SetDeltaAngles(deltaAngle);
        }

        private Vector2 GetDeltaPixels()
        {
            var value = Vector2.zero;

#if SUPPORT_INPUTSYSTEM
            if (_deltaActionReference != null)
            {
                var v = _deltaActionReference.action.ReadValue<Vector2>();
                value = v.sqrMagnitude > value.sqrMagnitude ? v : value;
            }
#endif

            return value;
        }

        private Vector2 GetAnalogValues()
        {
            var value = Vector2.zero;

#if SUPPORT_INPUTSYSTEM
            if (_continuousActionreference != null)
            {
                var v = _continuousActionreference.action.ReadValue<Vector2>();
                value = v.sqrMagnitude > value.sqrMagnitude ? v : value;
            }
#endif

            return value;
        }
    }
}

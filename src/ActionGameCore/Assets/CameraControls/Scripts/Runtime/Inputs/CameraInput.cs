#if ENABLE_INPUT_SYSTEM
using CameraControls.Controllers;
using UnityEngine;


using UnityEngine.InputSystem;


namespace CameraControls.Inputs
{
    public class CameraInput : MonoBehaviour
    {
        [SerializeField]
        private PlayerInput _playerInput = null;

        [SerializeField]
        private InputActionReference _lookActionReference = null;

        private ICameraController _cameraController;
        private Vector2 _deltaPixels;
        private Vector2 _analogValues;

        private void Awake()
        {
            _cameraController = GetComponent<ICameraController>();
            _playerInput.onActionTriggered += OnActionTriggerd;
        }

        private void OnDestroy()
        {
            _playerInput.onActionTriggered -= OnActionTriggerd;
        }

        private void OnActionTriggerd(InputAction.CallbackContext context)
        {
            if (_lookActionReference != null && context.action.name == _lookActionReference.action.name)
            {
                OnLook(context);
            }
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            if (context.action.activeControl.device is Pointer)
            {
                _deltaPixels = context.ReadValue<Vector2>();
            }
            else
            {
                _analogValues = context.ReadValue<Vector2>();
            }
        }

        private void Update()
        {
            if (_cameraController == null || !_cameraController.Enabled)
            {
                var cameraControllers = GetComponents<ICameraController>();
                foreach (var controller in cameraControllers)
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
            deltaAngle += PixelToAngle(_deltaPixels);
            deltaAngle += AnalogToAngle(_analogValues, Time.deltaTime);
            _cameraController.SetDeltaAngles(deltaAngle);
        }

        private static Vector2 PixelToAngle(Vector2 pixels)
        {
            const float DpiAverage = 96;
            var dpi = Screen.dpi == 0 ? DpiAverage : Screen.dpi;
            const float InchForTurn = 13;
            return pixels / dpi / InchForTurn * 180;
        }

        private static Vector2 AnalogToAngle(Vector2 analog, float deltaTime)
        {
            const float SecondsForTurn = 1.0f;
            return analog * Time.deltaTime / SecondsForTurn * 180;
        }
    }
}
#endif

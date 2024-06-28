using CameraControls.Controllers;
using UnityEngine;

#if SUPPORT_INPUTSYSTEM
using UnityEngine.InputSystem;
using System.Collections.Generic;

#endif

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
using Steamworks;
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

#if SUPPORT_STEAMWORKS
        [Header("SteamInput")]
        [SerializeField]
        private string _steamCameraActionName = "Camera";
#endif

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
        private bool _steamInitialized;
        private InputAnalogActionHandle_t _steamCameraActionHandle;
        private InputHandle_t[] _connectedControllerInputHandles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
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

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
            try
            {
                if (!string.IsNullOrEmpty(_steamCameraActionName))
                {
                    _steamCameraActionHandle = SteamInput.GetAnalogActionHandle(_steamCameraActionName);
                }
                _steamInitialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Steam Input: {e.Message}");
            }
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

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
            if (_steamInitialized && _steamCameraActionHandle != null)
            {
                var controllerCount = SteamInput.GetConnectedControllers(_connectedControllerInputHandles);
                for (int i = 0; i < controllerCount; i++)
                {
                    var inputHandle = _connectedControllerInputHandles[i];
                    var data = SteamInput.GetAnalogActionData(inputHandle, _steamCameraActionHandle);
                    var v = new Vector2(data.x, -data.y);
                    value = v.sqrMagnitude > value.sqrMagnitude ? v : value;
                }
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
                const float SecondsForTurn = 1.0f;
                var v = _continuousActionreference.action.ReadValue<Vector2>();
                value = v.sqrMagnitude > value.sqrMagnitude ? v : value;
            }
#endif

            return value;
        }
    }
}

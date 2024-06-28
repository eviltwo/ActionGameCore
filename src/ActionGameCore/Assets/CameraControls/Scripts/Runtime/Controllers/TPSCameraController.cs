using System.Collections.Generic;
using Steamworks;
using UnityEngine;

#if SUPPORT_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif 

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
#endif

namespace FPSCameraControls.Controllers
{
    public class TPSCameraController : MonoBehaviour
    {
        public enum OffsetRotationType
        {
            FullRotation = 0,
            HorizontalRotation = 1,
        }

        [SerializeField]
        public Transform Target = null;

        [SerializeField]
        public Vector3 Offset = Vector3.zero;

        [SerializeField]
        public OffsetRotationType OffsetRotation = OffsetRotationType.HorizontalRotation;

        [SerializeField]
        public float Distance = 5.0f;

        [SerializeField]
        public float Sensitivity = 1.0f;

        [SerializeField]
        public float AngleMin = -90.0f;

        [SerializeField]
        public float AngleMax = 90.0f;

        [SerializeField]
        public bool IsCheckWall = true;

        [SerializeField]
        public LayerMask WallLayerMask = ~0;

        [SerializeField]
        public int SmoothingFrameCount = 2;

        [SerializeField]
        private bool _lockAndHideCursor = true;

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

        private Vector3 _lookAngles;
        private List<Vector2> _deltaPositions = new List<Vector2>();
        private List<float> _deltaTimes = new List<float>();

        private void Start()
        {
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

            var deltaAngle = Vector2.zero;

            var deltaPosition = Vector2.zero;

#if SUPPORT_INPUTSYSTEM
            if (_deltaActionReference != null)
            {
                var v = _deltaActionReference.action.ReadValue<Vector2>();
                deltaPosition = v.sqrMagnitude > deltaPosition.sqrMagnitude ? v : deltaPosition;
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
                    deltaPosition = v.sqrMagnitude > deltaPosition.sqrMagnitude ? v : deltaPosition;
                }
            }
#endif

            {
                _deltaPositions.Add(deltaPosition);
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
                var v = smoothDeltaPosition / dpi / InchForTurn * 180;
                deltaAngle = v.sqrMagnitude > deltaAngle.sqrMagnitude ? v : deltaAngle;
            }

#if SUPPORT_INPUTSYSTEM
            if (_continuousActionreference != null)
            {
                const float SecondsForTurn = 1.0f;
                var v = _continuousActionreference.action.ReadValue<Vector2>() * Time.deltaTime / SecondsForTurn * 180;
                deltaAngle = v.sqrMagnitude > deltaAngle.sqrMagnitude ? v : deltaAngle;
            }
#endif

            _lookAngles.x = Mathf.Clamp(_lookAngles.x - deltaAngle.y * Sensitivity, -AngleMax, -AngleMin); // Look up and down
            _lookAngles.y = (_lookAngles.y + deltaAngle.x * Sensitivity) % 360; // Look left and right
            var rotation = Quaternion.Euler(_lookAngles);

            var offsetRotation = OffsetRotation == OffsetRotationType.FullRotation ? rotation : Quaternion.AngleAxis(_lookAngles.y, Vector3.up);
            var pivot = Target.position + offsetRotation * Offset;

            var distance = Distance;
            if (IsCheckWall)
            {
                var ray = new Ray(pivot, rotation * Vector3.back);
                if (Physics.Raycast(ray, out var hit, Distance, WallLayerMask))
                {
                    distance = hit.distance;
                }
            }

            transform.SetPositionAndRotation(pivot + rotation * Vector3.back * distance, rotation);
        }
    }
}

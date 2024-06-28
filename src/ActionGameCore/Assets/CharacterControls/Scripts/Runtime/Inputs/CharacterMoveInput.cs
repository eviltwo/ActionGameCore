using CharacterControls.Movements;
using UnityEngine;

#if SUPPORT_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace CharacterControls.Inputs
{
    public class CharacterMoveInput : MonoBehaviour
    {
#if SUPPORT_INPUTSYSTEM
        [Header("InputSystem")]
        [SerializeField]
        private InputActionReference _moveActionReference = null;

        [SerializeField]
        private InputActionReference _jumpActionReference = null;
#endif

#if SUPPORT_STEAMWORKS
        [Header("SteamInput")]
        [SerializeField]
        private string _steamMoveActionName = "Move";

        [SerializeField]
        private string _steamJumpActionName = "Jump";
#endif

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
        private bool _steamInitialized;
        private InputAnalogActionHandle_t _steamMoveActionHandle;
        private InputDigitalActionHandle_t _steamJumpActionHandle;
        private int _connectedControllerCount = 0;
        private InputHandle_t[] _connectedControllerInputHandles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
#endif

        public IMoveController MoveController { get; set; }

        private void Start()
        {
#if SUPPORT_INPUTSYSTEM
            _moveActionReference?.action.Enable();
            _jumpActionReference?.action.Enable();
            MoveController = GetComponent<IMoveController>();
#endif

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
            // You need to call SteamAPI.Init(), SteamInput.Init() and SteamInput.ActivateActionSet() in other class.
            try
            {
                if (!string.IsNullOrEmpty(_steamMoveActionName))
                {
                    _steamMoveActionHandle = SteamInput.GetAnalogActionHandle(_steamMoveActionName);
                }
                if (!string.IsNullOrEmpty(_steamJumpActionName))
                {
                    _steamJumpActionHandle = SteamInput.GetDigitalActionHandle(_steamJumpActionName);
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
            if (MoveController == null)
            {
                return;
            }

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
            if (_steamInitialized)
            {
                // Get controllers
                _connectedControllerCount = SteamInput.GetConnectedControllers(_connectedControllerInputHandles);
            }
#endif

            UpdateMoveInput();
            UpdateJumpInput();
        }

        private void UpdateMoveInput()
        {
            var value = Vector2.zero;

#if SUPPORT_INPUTSYSTEM
            if (_moveActionReference != null)
            {
                var v = _moveActionReference.action.ReadValue<Vector2>();
                value = v.sqrMagnitude > value.sqrMagnitude ? v : value;
            }
#endif

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
            if (_steamInitialized && _steamMoveActionHandle != null)
            {
                for (int i = 0; i < _connectedControllerCount; i++)
                {
                    var data = SteamInput.GetAnalogActionData(_connectedControllerInputHandles[i], _steamMoveActionHandle);
                    var v = new Vector2(data.x, data.y);
                    value = v.sqrMagnitude > value.sqrMagnitude ? v : value;
                    Debug.Log(v);
                }
            }
#endif

            MoveController.SetMoveInput(value);
        }

        private void UpdateJumpInput()
        {
            var value = false;

#if SUPPORT_INPUTSYSTEM
            value |= _jumpActionReference != null && _jumpActionReference.action.WasPressedThisFrame();
#endif

#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
            if (_steamInitialized && _steamJumpActionHandle != null)
            {
                for (int i = 0; i < _connectedControllerCount; i++)
                {
                    var data = SteamInput.GetDigitalActionData(_connectedControllerInputHandles[i], _steamJumpActionHandle);
                    value |= data.bState > 0;
                }
            }
#endif

            MoveController.SetJumpInput(value ? 1 : 0);
        }
    }
}

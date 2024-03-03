#if SUPPORT_INPUTSYSTEM
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace FPSCameraControls
{
    public class FPSCameraController : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset _inputActionAsset = null;

        [SerializeField]
        private string _actionMapName = "Player";

        [SerializeField]
        private string _lookActionName = "Look";

        [SerializeField]
        public Transform Target = null;

        [SerializeField]
        public float OffsetY = 1.5f;

        [SerializeField]
        public float Sensitivity = 0.1f;

        [SerializeField]
        public float AngleMin = -90.0f;

        [SerializeField]
        public float AngleMax = 90.0f;

        [SerializeField]
        private bool _lockAndHideCursor = true;

        private InputAction _lookAction;
        private Vector3 _lookAngles;

        private void Start()
        {
            Assert.IsNotNull(_inputActionAsset, "InputActionAsset is not set");
            if (_inputActionAsset != null)
            {
                _inputActionAsset.Enable();
                var map = _inputActionAsset.FindActionMap(_actionMapName);
                Assert.IsNotNull(map, $"Action map {_actionMapName} not found");
                if (map != null)
                {
                    _lookAction = map.FindAction(_lookActionName);
                    Assert.IsNotNull(_lookAction, $"Action {_lookActionName} not found in {_actionMapName}");
                }
            }

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
            if (_lookAction != null)
            {
                var look = _lookAction.ReadValue<Vector2>();
                _lookAngles.x = Mathf.Clamp(_lookAngles.x - look.y * Sensitivity, -AngleMax, -AngleMin); // Look up and down
                _lookAngles.y = (_lookAngles.y + look.x * Sensitivity) % 360; // Look left and right
                rotation = Quaternion.Euler(_lookAngles);
            }

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
#endif

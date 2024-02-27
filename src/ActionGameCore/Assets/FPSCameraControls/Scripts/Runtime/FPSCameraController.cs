#if SUPPORT_INPUTSYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSCameraControls
{
    public class FPSCameraController : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset _inputActionAsset = null;

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
            _inputActionAsset.Enable();
            var map = _inputActionAsset.FindActionMap("Player");
            _lookAction = map.FindAction("Look");

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

            var look = _lookAction.ReadValue<Vector2>();
            _lookAngles.x = Mathf.Clamp(_lookAngles.x - look.y * Sensitivity, -AngleMax, -AngleMin); // Look up and down
            _lookAngles.y = (_lookAngles.y + look.x * Sensitivity) % 360; // Look left and right
            var rotation = Quaternion.Euler(_lookAngles);

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
#endif

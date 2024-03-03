#if SUPPORT_INPUTSYSTEM
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace FPSCameraControls
{
    public class TPSCameraController : MonoBehaviour
    {
        public enum OffsetRotationType
        {
            FullRotation = 0,
            HorizontalRotation = 1,
        }

        [SerializeField]
        private InputActionAsset _inputActionAsset = null;

        [SerializeField]
        private string _actionMapName = "Player";

        [SerializeField]
        private string _lookActionName = "Look";

        [SerializeField]
        public Transform Target = null;

        [SerializeField]
        public Vector3 Offset = Vector3.zero;

        [SerializeField]
        public OffsetRotationType OffsetRotation = OffsetRotationType.HorizontalRotation;

        [SerializeField]
        public float Distance = 5.0f;

        [SerializeField]
        public float Sensitivity = 0.1f;

        [SerializeField]
        public float AngleMin = -90.0f;

        [SerializeField]
        public float AngleMax = 90.0f;

        [SerializeField]
        public bool IsCheckWall = true;

        [SerializeField]
        public LayerMask WallLayerMask = ~0;

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

            var rotation = Target.rotation;
            if (_lookAction != null)
            {
                var look = _lookAction.ReadValue<Vector2>();
                _lookAngles.x = Mathf.Clamp(_lookAngles.x - look.y * Sensitivity, -AngleMax, -AngleMin); // Look up and down
                _lookAngles.y = (_lookAngles.y + look.x * Sensitivity) % 360; // Look left and right
                rotation = Quaternion.Euler(_lookAngles);
            }

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
#endif

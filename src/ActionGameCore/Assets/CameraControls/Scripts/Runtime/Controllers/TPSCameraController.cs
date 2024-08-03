using UnityEngine;

namespace CameraControls.Controllers
{
    public class TPSCameraController : MonoBehaviour, ICameraController
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
        private bool _lockAndHideCursor = true;

        private Vector2 _deltaAngles;
        private Vector3 _lookAngles;

        public bool Enabled => enabled;

        public void SetDeltaAngles(Vector2 deltaAngles)
        {
            _deltaAngles = deltaAngles;
        }

        private void Start()
        {
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

            _lookAngles.x = Mathf.Clamp(_lookAngles.x - _deltaAngles.y * Sensitivity, -AngleMax, -AngleMin); // Look up and down
            _lookAngles.y = (_lookAngles.y + _deltaAngles.x * Sensitivity) % 360; // Look left and right
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

using UnityEngine;

namespace CameraControls.Controllers
{
    public class FPSCameraController : MonoBehaviour, ICameraController
    {
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
        public int SmoothingFrameCount = 2;

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

            var position = Target.position + new Vector3(0, OffsetY, 0);

            _lookAngles.x = Mathf.Clamp(_lookAngles.x - _deltaAngles.y * Sensitivity, -AngleMax, -AngleMin); // Look up and down
            _lookAngles.y = (_lookAngles.y + _deltaAngles.x * Sensitivity) % 360; // Look left and right
            var rotation = Quaternion.Euler(_lookAngles);

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}

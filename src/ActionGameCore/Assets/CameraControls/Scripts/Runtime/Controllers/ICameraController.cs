using UnityEngine;

namespace CameraControls.Controllers
{
    public interface ICameraController
    {
        bool Enabled { get; }
        void SetDeltaAngles(Vector2 deltaAngles);
    }
}

using UnityEngine;

namespace CameraControls.Controllers.Effects
{
    public class TPSCameraEffector : MonoBehaviour
    {
        [SerializeField]
        public TPSCameraController Controller = null;

        private Movement _movement;
        private Transform _target;
        private GameObject _handle;

        private void Reset()
        {
            Controller = GetComponent<TPSCameraController>();
        }

        private void OnDestroy()
        {
            if (Controller != null && _target != null)
            {
                Controller.Target = _target;
            }
            if (_handle != null)
            {
                Destroy(_handle);
            }
        }

        private void Update()
        {
            if (_movement != null)
            {
                _movement.Update(Time.deltaTime);
                if (_movement.IsDone())
                {
                    Controller.Target = _target;
                    _target = null;
                    _movement = null;
                }
            }
        }

        public void PlayLinearMove(float duration)
        {
            if (_movement != null)
            {
                return;
            }

            var handle = GetOrCreateHandle();
            handle.transform.position = Controller.transform.position;
            handle.transform.rotation = Controller.transform.rotation;
            _target = Controller.Target;
            Controller.Target = handle.transform;
            _movement = new LinearMovement(_target, handle.transform, duration);
        }

        private GameObject GetOrCreateHandle()
        {
            if (_handle == null)
            {
                _handle = new GameObject("TPSCameraEffector Handle");
            }
            return _handle;
        }

        private abstract class Movement
        {
            protected Transform Target { get; }
            public Transform Handle { get; }
            public Movement(Transform target, Transform handle)
            {
                Target = target;
                Handle = handle;
            }

            public abstract void Update(float deltaTime);

            public abstract bool IsDone();
        }

        private class LinearMovement : Movement
        {
            private readonly Vector3 _start;
            private readonly float _duration;
            private float _elapsedTime;
            public LinearMovement(Transform target, Transform handle, float duration)
                : base(target, handle)
            {
                _start = target.position;
                _duration = duration;
            }

            public override void Update(float deltaTime)
            {
                _elapsedTime += deltaTime;
                var t = _elapsedTime / _duration;
                Handle.position = Vector3.Lerp(_start, Target.position, t);
            }

            public override bool IsDone()
            {
                return _elapsedTime > _duration;
            }
        }
    }
}
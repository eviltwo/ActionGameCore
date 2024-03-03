using UnityEngine;

namespace ActionGameCoreSamples
{
    public class FloorMove : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        public float LoopDuration = 1.0f;

        [SerializeField]
        public AnimationCurve MoveCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        [SerializeField]
        public Vector3 MoveOffset = Vector3.zero;

        private float _elapsedTime = 0.0f;
        private Vector3 _startPosition;

        private void Reset()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            _elapsedTime += Time.fixedDeltaTime;
            var t = Mathf.Repeat(_elapsedTime / LoopDuration, 1.0f);
            var offset = MoveOffset * MoveCurve.Evaluate(t);
            _rigidbody.MovePosition(_startPosition + offset);
        }
    }
}

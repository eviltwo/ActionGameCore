using CharacterControls.Inputs;
using CharacterControls.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActionGameCoreSamples
{
    public class BotInput : MonoBehaviour
    {
        [SerializeField]
        private CharacterMoveInputRelay _moveInputRelay;

        [SerializeField]
        private Transform _chaseTarget;

        [SerializeField]
        private float _chaseDistance = 5.0f;

        [SerializeField]
        private float _jumpInterval = 5.0f;

        private float _jumpElapsedTime;

        private InputPhaseTracker<Vector2> _moveInputPhaseTracker = new(Vector2.zero);

        private InputPhaseTracker<bool> _jumpInputPhaseTracker = new(false);

        private void Awake()
        {
            _moveInputPhaseTracker.OnStarted += value => _moveInputRelay.SendInput(new InputContext("Move", InputActionPhase.Started, value));
            _moveInputPhaseTracker.OnPerformed += value => _moveInputRelay.SendInput(new InputContext("Move", InputActionPhase.Performed, value));
            _moveInputPhaseTracker.OnCanceled += value => _moveInputRelay.SendInput(new InputContext("Move", InputActionPhase.Canceled, value));
            _jumpInputPhaseTracker.OnStarted += value => _moveInputRelay.SendInput(new InputContext("Jump", InputActionPhase.Started, value ? 1f : 0f));
            _jumpInputPhaseTracker.OnPerformed += value => _moveInputRelay.SendInput(new InputContext("Jump", InputActionPhase.Performed, value ? 1f : 0f));
            _jumpInputPhaseTracker.OnCanceled += value => _moveInputRelay.SendInput(new InputContext("Jump", InputActionPhase.Canceled, value ? 1f : 0f));
        }

        private void Update()
        {
            if (_chaseTarget == null)
            {
                return;
            }

            var currentPosition = transform.position;
            var targetPosition = _chaseTarget.position;
            var toTargetVec = targetPosition - currentPosition;
            var direction2D = new Vector2(toTargetVec.x, toTargetVec.z).normalized;
            var isMoving = toTargetVec.sqrMagnitude > _chaseDistance * _chaseDistance;
            _moveInputPhaseTracker.Push(isMoving ? direction2D : Vector2.zero);

            _jumpElapsedTime += Time.deltaTime;
            var isOverJumpInterval = _jumpElapsedTime > _jumpInterval;
            _jumpInputPhaseTracker.Push(isOverJumpInterval);
            if (isOverJumpInterval)
            {
                _jumpElapsedTime = 0.0f;
            }
        }
    }
}

using CharacterControls.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActionGameCoreSamples
{
    public class BotInput : MonoBehaviour
    {
        [SerializeField]
        private CharacterMoveInputRelay moveInputRelay;

        [SerializeField]
        private Transform _chaseTarget;

        [SerializeField]
        private float _chaseDistance = 5.0f;

        [SerializeField]
        private float _jumpInterval = 5.0f;

        private float _jumpElapsedTime;

        private void Update()
        {
            if (_chaseTarget == null)
            {
                return;
            }

            var currentPosition = transform.position;
            var targetPosition = _chaseTarget.position;
            var toTargetVec = targetPosition - currentPosition;
            if (toTargetVec.sqrMagnitude > _chaseDistance * _chaseDistance)
            {
                var direction2D = new Vector2(toTargetVec.x, toTargetVec.z).normalized;
                moveInputRelay.SendInput(new InputContext("Move", InputActionPhase.Performed, direction2D));
            }
            else
            {
                moveInputRelay.SendInput(new InputContext("Move", InputActionPhase.Disabled, Vector2.zero));
            }

            _jumpElapsedTime += Time.deltaTime;
            var isTriggerJump = 0f;
            if (_jumpElapsedTime > _jumpInterval)
            {
                _jumpElapsedTime = 0.0f;
                isTriggerJump = 1f;
            }

            moveInputRelay.SendInput(new InputContext("Jump", isTriggerJump == 0 ? InputActionPhase.Disabled : InputActionPhase.Performed, isTriggerJump));
        }
    }
}

using CharacterControls.Inputs;
using UnityEngine;

namespace CharacterControls.Movements.Modules
{
    public abstract class CharacterModuleBase : MonoBehaviour, ICharacterModule, IInputReceiver
    {
        private CharacterMoveInput _characterInput;

        protected virtual void OnEnable()
        {
            _characterInput = GetComponentInParent<CharacterMoveInput>();
            if (_characterInput != null)
            {
                _characterInput.RegisterReceiver(this);
            }

            var characterMoveController = GetComponentInParent<CharacterMoveController>();
            characterMoveController?.RegisterModule(this);
        }

        protected virtual void OnDisable()
        {
            if (_characterInput != null)
            {
                _characterInput.UnregisterReceiver(this);
                _characterInput = null;
            }

            var characterMoveController = GetComponentInParent<CharacterMoveController>();
            characterMoveController?.UnregisterModule(this);
        }

        public abstract void OnReceiveInput(InputContext context);

        public abstract void FixedUpdateModule(in CharacterMoveModulePayload payload);
    }
}

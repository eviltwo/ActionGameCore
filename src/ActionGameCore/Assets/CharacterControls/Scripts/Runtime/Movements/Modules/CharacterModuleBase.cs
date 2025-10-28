using CharacterControls.Inputs;
using UnityEngine;

namespace CharacterControls.Movements.Modules
{
    public abstract class CharacterModuleBase : MonoBehaviour, ICharacterModule, IInputReceiver
    {
        private CharacterMoveInputRelay _characterInputRelay;

        protected virtual void OnEnable()
        {
            _characterInputRelay = GetComponentInParent<CharacterMoveInputRelay>();
            if (_characterInputRelay != null)
            {
                _characterInputRelay.RegisterReceiver(this);
            }

            var characterMoveController = GetComponentInParent<CharacterMoveController>();
            characterMoveController?.RegisterModule(this);
        }

        protected virtual void OnDisable()
        {
            if (_characterInputRelay != null)
            {
                _characterInputRelay.UnregisterReceiver(this);
                _characterInputRelay = null;
            }

            var characterMoveController = GetComponentInParent<CharacterMoveController>();
            characterMoveController?.UnregisterModule(this);
        }

        public abstract void OnReceiveInput(InputContext context);

        public abstract void FixedUpdateModule(in CharacterMoveModulePayload payload);
    }
}

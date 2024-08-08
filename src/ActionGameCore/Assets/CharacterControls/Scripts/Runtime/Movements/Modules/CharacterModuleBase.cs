using UnityEngine;

namespace CharacterControls.Movements.Modules
{
    public abstract class CharacterModuleBase : MonoBehaviour, ICharacterModule
    {
        protected virtual void Start()
        {
            var characterMoveController = GetComponentInParent<CharacterMoveController>();
            characterMoveController?.RegisterModule(this);
        }

        protected virtual void OnDestroy()
        {
            var characterMoveController = GetComponentInParent<CharacterMoveController>();
            characterMoveController?.UnregisterModule(this);
        }

        public abstract void FixedUpdateModule(in CharacterMoveModulePayload payload);
    }
}

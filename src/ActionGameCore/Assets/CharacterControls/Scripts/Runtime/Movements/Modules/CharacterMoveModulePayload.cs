using UnityEngine;

namespace CharacterControls.Movements.Modules
{
    public struct CharacterMoveModulePayload
    {
        public readonly CharacterMoveController Controller;
        public readonly Transform Root;
        public CharacterMoveModulePayload(CharacterMoveController controller, Transform root)
        {
            Controller = controller;
            Root = root;
        }
    }
}

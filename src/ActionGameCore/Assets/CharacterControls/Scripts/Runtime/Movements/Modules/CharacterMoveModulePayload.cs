using UnityEngine;

namespace CharacterControls.Movements.Modules
{
    public struct CharacterMoveModulePayload
    {
        public readonly Transform Root;
        public readonly Rigidbody Rigidbody;
        public readonly CharacterMoveController Controller;

        public CharacterMoveModulePayload(Transform root, Rigidbody rb, CharacterMoveController controller)
        {
            Root = root;
            Rigidbody = rb;
            Controller = controller;
        }
    }
}

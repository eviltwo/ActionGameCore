namespace CharacterControls.Movements.Modules
{
    public struct CharacterMoveModulePayload
    {
        public readonly CharacterMoveController Controller;
        public CharacterMoveModulePayload(CharacterMoveController controller)
        {
            Controller = controller;
        }
    }
}

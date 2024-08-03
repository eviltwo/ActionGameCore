namespace CharacterControls.Movements.Modules
{
    public interface ICharacterMoveModule
    {
        void FixedUpdateModule(in CharacterMoveModulePayload payload);
    }
}

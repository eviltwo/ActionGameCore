namespace CharacterControls.Movements.Modules
{
    public interface ICharacterModule
    {
        void FixedUpdateModule(in CharacterMoveModulePayload payload);
    }
}

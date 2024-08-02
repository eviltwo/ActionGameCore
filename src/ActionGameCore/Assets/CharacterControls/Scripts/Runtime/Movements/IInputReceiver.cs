namespace CharacterControls.Movements
{
    public interface IInputReceiver<T>
    {
        void OnReceiveInput(string key, T value);
    }
}

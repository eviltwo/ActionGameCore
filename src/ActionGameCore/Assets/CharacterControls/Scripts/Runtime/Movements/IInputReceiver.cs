namespace CharacterControls.Movements
{
    public interface IInputReceiver<T> where T : struct
    {
        void OnReceiveInput(string key, T value);
    }
}

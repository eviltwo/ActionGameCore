namespace Interactions
{
    public interface IInteractHandler
    {
        string InputActionNameFilter { get; }

        void OnInteract(InteractEventData eventData);
    }

    public class InteractEventData
    {
        public string InputActionName { get; set; }
        public IInteractHandler InteractedObject { get; set; }
    }
}

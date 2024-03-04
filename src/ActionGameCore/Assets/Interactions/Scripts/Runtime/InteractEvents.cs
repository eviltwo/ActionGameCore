namespace Interactions
{
    public interface ISelectHandler
    {
        void OnSelect();
        void OnDeselect();
    }

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

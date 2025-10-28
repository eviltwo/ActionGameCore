using UnityEngine.InputSystem;

namespace CharacterControls.Inputs
{
    public struct InputContext
    {
        private readonly bool _hasCallbackContext;

        private InputAction.CallbackContext _callbackContext;

        private readonly string _actionName;

        private readonly InputActionPhase _phase;

        private readonly object _value;

        public InputContext(InputAction.CallbackContext context)
        {
            _hasCallbackContext = true;
            _callbackContext = context;
            _actionName = null;
            _phase = default;
            _value = null;
        }

        public InputContext(string actionName, InputActionPhase phase, object value)
        {
            _hasCallbackContext = false;
            _callbackContext = default;
            _actionName = actionName;
            _phase = phase;
            _value = value;
        }

        public string actionName => _hasCallbackContext ? _callbackContext.action.name : _actionName;

        public InputActionPhase phase => _hasCallbackContext ? _callbackContext.phase : _phase;

        public TValue ReadValue<TValue>() where TValue : struct
        {
            return _hasCallbackContext ? _callbackContext.ReadValue<TValue>() : (TValue)_value;
        }
    }
}

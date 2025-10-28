using UnityEngine.InputSystem;

namespace CharacterControls.Utilities
{
    public class InputPhaseTracker<T>
        where T : struct
    {
        private readonly T _defaultValue;
        private T _lastValue;
        private bool _isDefaultLast;

        public InputActionPhase Phase { get; private set; }

        public event System.Action<T> OnStarted;
        public event System.Action<T> OnPerformed;
        public event System.Action<T> OnCanceled;

        public InputPhaseTracker(T defaultValue)
        {
            _defaultValue = defaultValue;
            _lastValue = defaultValue;
            _isDefaultLast = true;
            Phase = InputActionPhase.Disabled;
        }

        public InputActionPhase Push(T value)
        {
            var isDefault = value.Equals(_defaultValue);
            var isSameLast = value.Equals(_lastValue);
            if (_isDefaultLast && isDefault)
            {
                Phase = InputActionPhase.Waiting;
            }

            if (_isDefaultLast && !isDefault)
            {
                Phase = InputActionPhase.Started;
                OnStarted?.Invoke(value);
                Phase = InputActionPhase.Performed;
                OnPerformed?.Invoke(value);
            }

            if (!_isDefaultLast && !isDefault)
            {
                Phase = InputActionPhase.Performed;
                if (!isSameLast)
                {
                    OnPerformed?.Invoke(value);
                }
            }

            if (!_isDefaultLast && isDefault)
            {
                Phase = InputActionPhase.Canceled;
                OnCanceled?.Invoke(value);
                Phase = InputActionPhase.Waiting;
            }

            _lastValue = value;
            _isDefaultLast = isDefault;
            return Phase;
        }
    }
}

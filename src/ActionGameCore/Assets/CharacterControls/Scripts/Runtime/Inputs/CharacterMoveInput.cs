#if SUPPORT_INPUTSYSTEM
using CharacterControls.Movements;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterControls.Inputs
{
    public class CharacterMoveInput : MonoBehaviour
    {
        [SerializeField]
        public InputActionAsset InputActionAsset;

        public IMoveController MoveController { get; set; }

        private void Start()
        {
            MoveController = GetComponent<IMoveController>();
        }

        private void Update()
        {
            if (InputActionAsset == null || MoveController == null)
            {
                return;
            }

            InputActionAsset.Enable();
            var map = InputActionAsset.FindActionMap("Player");
            var move = map.FindAction("Move");
            var value = move.ReadValue<Vector2>();
            MoveController.SetMoveInput(value);
        }
    }
}
#endif

using UnityEngine;

namespace CharacterControls.Movements
{
    public interface IMoveController
    {
        void SetMoveInput(Vector2 moveInput);
    }
}

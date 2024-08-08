using UnityEngine;

namespace CharacterControls.Movements.Utils
{
    public class IKEventRelay : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] _targets = default;

        private void OnAnimatorIK(int layerIndex)
        {
            var count = _targets.Length;
            for (var i = 0; i < count; i++)
            {
                var target = _targets[i];
                if (target != null)
                {
                    target.SendMessage("OnAnimatorIK", layerIndex, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}

using UnityEngine;

namespace CharacterControls
{
    public class FrictionCalculator
    {
        public bool Slipping { get; private set; }

        public Vector3 FrictionForce { get; private set; }

        public float Strength { get; set; } = 1;

        public float StaticFriction { get; set; } = 1;

        public float DynamicFriction { get; set; } = 1;

        public void Calculate(Vector3 velocity)
        {
            var friction = Slipping ? DynamicFriction : StaticFriction;
            if (velocity.sqrMagnitude < friction * friction)
            {
                Slipping = false;
                FrictionForce = -(velocity * Strength);
            }
            else
            {
                Slipping = true;
                FrictionForce = velocity.normalized * -(friction * Strength);
            }
        }
    }
}

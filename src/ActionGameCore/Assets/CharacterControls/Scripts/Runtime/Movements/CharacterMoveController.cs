using System;
using System.Collections.Generic;
using CharacterControls.Movements.Modules;
using UnityEngine;

namespace CharacterControls.Movements
{
    public class CharacterMoveController : MonoBehaviour, IInputReceiver<Vector2>
    {
        [SerializeField]
        public Rigidbody Rigidbody = null;

        [SerializeField]
        public Transform CameraTransform = null;

        [SerializeField]
        public LayerMask GroundLayer = ~0;

        [SerializeField]
        public float StepHeightMax = 0.25f;

        [SerializeField]
        public float SlopeLimit = 45.0f;

        [SerializeField]
        private bool _autoResizeCapsuleCollider = true;

        [SerializeField]
        private CapsuleCollider _capsuleCollider = null;

        [SerializeField]
        public float LegStrength = 20.0f;

        [SerializeField]
        public float LegDamper = 40.0f;

        [SerializeField]
        public float WalkSpeed = 5.0f;

        [SerializeField]
        public float FrictionStrength = 30;

        [SerializeField]
        public float StaticFriction = 0.1f;

        [SerializeField]
        public float DynamicFriction = 1.0f;

        [SerializeField]
        public float AirWalkSpeed = 5.0f;

        [SerializeField]
        private bool _drawDebug = false;

        private Vector2 _moveInput;
        private FrictionCalculator _frictionCalculator = new FrictionCalculator();
        private ModuleRequestManager _skipGroundCheckRequestManager = new ModuleRequestManager();
        private ModuleRequestManager _stopMoveRequestManager = new ModuleRequestManager();
        private List<ICharacterMoveModule> _modules = new List<ICharacterMoveModule>();

        public bool IsGrounded { get; private set; }

        public Vector3 TargetVelocity { get; private set; }

        public Vector3 RelativeVelocityToGround { get; private set; }

        private void Reset()
        {
            Rigidbody = GetComponent<Rigidbody>();
            CameraTransform = Camera.main?.transform;
            _capsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void Start()
        {
            if (_autoResizeCapsuleCollider && _capsuleCollider != null)
            {
                var height = _capsuleCollider.height;
                var center = _capsuleCollider.center;
                var stepHeight = StepHeightMax;
                var newHeight = height - stepHeight;
                var newCenter = center + new Vector3(0, stepHeight / 2, 0);
                _capsuleCollider.height = newHeight;
                _capsuleCollider.center = newCenter;
            }
        }

        public void OnReceiveInput(string key, Vector2 value)
        {
            if (key == "Move")
            {
                if (value.sqrMagnitude > 1)
                {
                    value.Normalize();
                }

                _moveInput = value;
            }
        }

        private void Update()
        {
            CharacterMoveUtility.DrawDebug = _drawDebug;
        }

        private void FixedUpdate()
        {
            if (Rigidbody == null)
            {
                return;
            }

            const int CheckCount = 8;
            const float Radius = 0.1f;
            const float Margin = 0.1f;
            RaycastHit hit = default;
            var ray = new Ray(transform.position + transform.up * (StepHeightMax + Margin), -transform.up);
            var distance = StepHeightMax * 2 + Margin;
            IsGrounded = !_skipGroundCheckRequestManager.HasRequest()
                && CharacterMoveUtility.CheckGroundSafety(ray, Radius, CheckCount, distance, SlopeLimit, out hit, GroundLayer)
                && hit.distance < StepHeightMax * 2 + Margin;

            if (IsGrounded)
            {
                CalculateLegSpring(hit, Margin);
            }

            TargetVelocity = Vector3.zero;
            RelativeVelocityToGround = Vector3.zero;
            if (!_stopMoveRequestManager.HasRequest())
            {
                if (IsGrounded)
                {
                    Walk(hit);
                }
                else
                {
                    WalkAir();
                }
            }

            var modulePayload = new CharacterMoveModulePayload(transform, Rigidbody, this);
            var count = _modules.Count;
            for (var i = 0; i < count; i++)
            {
                _modules[i].FixedUpdateModule(modulePayload);
            }
        }

        private void CalculateLegSpring(RaycastHit hit, float margin)
        {
            // Spring
            var springLength = hit.distance - margin;
            var springRatio = Mathf.Clamp01(springLength / StepHeightMax);
            var springPushForce = (1f - springRatio) * LegStrength;
            Rigidbody.AddForce(springPushForce * transform.up, ForceMode.Acceleration);

            // Damper
            var currentVelocity = Rigidbody.velocity;
            if (hit.rigidbody != null)
            {
                currentVelocity -= hit.rigidbody.GetPointVelocity(hit.point);
            }
            var damperForce = Vector3.Project(currentVelocity, hit.normal) * -LegDamper * (1f - springRatio);
            Rigidbody.AddForce(damperForce, ForceMode.Acceleration);
        }

        private void Walk(RaycastHit hit)
        {
            var forward = CharacterMoveUtility.GetForwardMovementDirectionFromCamera(transform, CameraTransform);
            var right = Vector3.Cross(transform.up, forward);
            var inputBaseVelocity = (forward * _moveInput.y + right * _moveInput.x) * WalkSpeed;
            TargetVelocity = Quaternion.FromToRotation(transform.up, hit.normal) * inputBaseVelocity;
            RelativeVelocityToGround = Rigidbody.velocity;
            if (hit.rigidbody != null)
            {
                RelativeVelocityToGround -= hit.rigidbody.GetPointVelocity(hit.point);
            }
            var diffVelocity = TargetVelocity - RelativeVelocityToGround;
            diffVelocity -= Vector3.Project(diffVelocity, transform.up); // Remove velocity on normal
            _frictionCalculator.StaticFriction = StaticFriction;
            _frictionCalculator.DynamicFriction = DynamicFriction;
            _frictionCalculator.Strength = FrictionStrength;
            _frictionCalculator.Calculate(-diffVelocity);
            var addVelocity = _frictionCalculator.FrictionForce;
            Rigidbody.AddForce(addVelocity, ForceMode.Acceleration);
        }

        private void WalkAir()
        {
            var forward = CharacterMoveUtility.GetForwardMovementDirectionFromCamera(transform, CameraTransform);
            var right = Vector3.Cross(transform.up, forward);
            TargetVelocity = (forward * _moveInput.y + right * _moveInput.x) * AirWalkSpeed;
            RelativeVelocityToGround = Rigidbody.velocity;
            var addVelocity = TargetVelocity - (Rigidbody.velocity - Vector3.Project(Rigidbody.velocity, transform.up));
            Rigidbody.AddForce(addVelocity, ForceMode.Acceleration);
        }

        public void RegisterModule(ICharacterMoveModule module)
        {
            _modules.Add(module);
        }

        public void UnregisterModule(ICharacterMoveModule module)
        {
            _modules.Remove(module);
        }

        public void GetModules<T>(List<T> results)
            where T : ICharacterMoveModule
        {
            results.Clear();
            var count = _modules.Count;
            for (var i = 0; i < count; i++)
            {
                if (_modules[i] is T module)
                {
                    results.Add(module);
                }
            }
        }

        public IDisposable RequestSkipGroundCheck()
        {
            return _skipGroundCheckRequestManager.GetRequest();
        }

        public IDisposable RequestStopMove()
        {
            return _stopMoveRequestManager.GetRequest();
        }
    }
}

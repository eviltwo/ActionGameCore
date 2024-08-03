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
        public float LegStrength = 40.0f;

        [SerializeField]
        public float LegSuspenion = 20.0f;

        [SerializeField]
        public float WalkSpeed = 5.0f;

        [SerializeField]
        public float FrictionStrength = 20;

        [SerializeField]
        public float StaticFriction = 0.1f;

        [SerializeField]
        public float DynamicFriction = 1.0f;

        [SerializeField]
        public float AirWalkSpeed = 2.0f;

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

        private void FixedUpdate()
        {
            if (Rigidbody == null)
            {
                return;
            }

            const float DistanceTopMergin = 0.2f;
            const float DistanceMergin = 0.3f;
            const float Radius = 0.1f;
            var legRay = new Ray(transform.position + transform.up * (StepHeightMax + Radius + DistanceTopMergin), -transform.up);
            RaycastHit hitInfo = default;
            IsGrounded = !_skipGroundCheckRequestManager.HasRequest()
                && Physics.SphereCast(legRay, Radius, out hitInfo, StepHeightMax + DistanceTopMergin + DistanceMergin, GroundLayer)
                && Vector3.Angle(hitInfo.normal, Vector3.up) < SlopeLimit;
            if (_stopMoveRequestManager.HasRequest())
            {
                TargetVelocity = Vector3.zero;
                RelativeVelocityToGround = Vector3.zero;
            }
            else
            {
                if (IsGrounded)
                {
                    // Leg spring
                    var springLength = hitInfo.distance - DistanceTopMergin;
                    var springRatio = Mathf.Clamp01(springLength / StepHeightMax);
                    var springPushForce = (1f - springRatio) * LegStrength;
                    Rigidbody.AddForce(springPushForce * transform.up, ForceMode.Acceleration);

                    // Leg suspension
                    var currentVelocity = Rigidbody.velocity;
                    if (hitInfo.rigidbody != null)
                    {
                        currentVelocity -= hitInfo.rigidbody.GetPointVelocity(hitInfo.point);
                    }
                    var suspensionForce = Vector3.Project(currentVelocity, transform.up) * -LegSuspenion * (1f - springRatio);
                    Rigidbody.AddForce(suspensionForce, ForceMode.Acceleration);

                    // Move horizontal
                    var forward = GetForwardOfMovementSpace();
                    var right = Vector3.Cross(transform.up, forward);
                    var inputBaseVelocity = (forward * _moveInput.y + right * _moveInput.x) * WalkSpeed;
                    TargetVelocity = Quaternion.FromToRotation(transform.up, hitInfo.normal) * inputBaseVelocity;
                    Debug.DrawRay(transform.position, TargetVelocity, Color.red);
                    RelativeVelocityToGround = Rigidbody.velocity;
                    if (hitInfo.rigidbody != null)
                    {
                        RelativeVelocityToGround -= hitInfo.rigidbody.GetPointVelocity(hitInfo.point);
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
                else
                {
                    var forward = GetForwardOfMovementSpace();
                    var right = Vector3.Cross(transform.up, forward);
                    TargetVelocity = (forward * _moveInput.y + right * _moveInput.x) * AirWalkSpeed;
                    RelativeVelocityToGround = Rigidbody.velocity;
                    var addVelocity = TargetVelocity - (Rigidbody.velocity - Vector3.Project(Rigidbody.velocity, transform.up));
                    Rigidbody.AddForce(addVelocity, ForceMode.Acceleration);
                }
            }

            var modulePayload = new CharacterMoveModulePayload(this, transform);
            var count = _modules.Count;
            for (var i = 0; i < count; i++)
            {
                _modules[i].FixedUpdateModule(modulePayload);
            }
        }

        private Vector3 GetForwardOfMovementSpace()
        {
            if (CameraTransform == null)
            {
                return transform.forward;
            }

            var forward = CameraTransform.forward;
            if (forward == transform.up)
            {
                forward = -CameraTransform.up;
            }
            else if (forward == -transform.up)
            {
                forward = CameraTransform.up;
            }

            return Vector3.ProjectOnPlane(forward, transform.up).normalized;
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

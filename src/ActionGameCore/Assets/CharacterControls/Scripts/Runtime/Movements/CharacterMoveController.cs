using System;
using System.Collections.Generic;
using CharacterControls.Movements.Modules;
using UnityEngine;

namespace CharacterControls.Movements
{
    public class CharacterMoveController : MonoBehaviour
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
        public float LegStrength = 60.0f;

        [SerializeField]
        public float LegDamper = 50.0f;

        [SerializeField]
        private bool _drawDebug = false;

        private ModuleRequestManager _skipGroundCheckRequestManager = new ModuleRequestManager();
        private List<ICharacterModule> _modules = new List<ICharacterModule>();

        public bool IsGrounded { get; private set; }

        public RaycastHit LastGroundHit { get; private set; }

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
                && CharacterMoveUtility.CheckCircleGroundSafety(ray, distance, Radius, CheckCount, SlopeLimit, out hit, GroundLayer)
                && hit.distance < StepHeightMax * 2 + Margin;

            if (IsGrounded)
            {
                LastGroundHit = hit;
                CalculateLegSpring(hit, Margin);
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

        public void RegisterModule(ICharacterModule module)
        {
            _modules.Add(module);
        }

        public void UnregisterModule(ICharacterModule module)
        {
            _modules.Remove(module);
        }

        public T GetModule<T>()
            where T : ICharacterModule
        {
            TryGetModule<T>(out var result);
            return result;
        }

        public bool TryGetModule<T>(out T result)
            where T : ICharacterModule
        {
            var count = _modules.Count;
            for (var i = 0; i < count; i++)
            {
                if (_modules[i] is T module)
                {
                    result = module;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public void GetModules<T>(List<T> results)
            where T : ICharacterModule
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
    }
}

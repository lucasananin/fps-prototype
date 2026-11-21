using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Windows;

namespace FPS
{
    public class BallisticProjectile : AbstractProjectile
    {
        [Header("// Ballistic Projectile - Properties")]
        [SerializeField] Rigidbody _rb = null;
        [SerializeField] SphereCollider _dummyCollider = null;
        [SerializeField] Transform _renderTransform = null;

        [Header("// Ballistic Projectile - Debug")]
        [SerializeField] List<Collider> _collidersHit = default;
        [SerializeField] Vector3 _lastPosition = default;
        [SerializeField] Vector3 _defaultVelocity = default;
        [SerializeField] float _accelerationTime = 0f;
        [SerializeField] float _speedMultiplier = 0f;

        private readonly RaycastHit[] _results = new RaycastHit[9];

        private void Awake()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
            if (_dummyCollider == null)
                _dummyCollider = GetComponent<SphereCollider>();

            _dummyCollider.enabled = false;
        }

        private void FixedUpdate()
        {
            CheckAcceleration();
            CheckDestroyTime();

            //Vector3 newPosition = _rb.position + _speedMultiplier * Time.fixedDeltaTime * transform.forward;
            //_rb.MovePosition(newPosition);
            CheckCollisions();
        }

        //private void LateUpdate()
        //{
        //    CheckCollisions();
        //}

        private void CheckCollisions()
        {
            //float _distance = Vector2.Distance(transform.position, _lastPosition);
            //int _hits = Physics.SphereCastNonAlloc(_lastPosition, _dummyCollider.radius, transform.right, _results, _distance, _projectileSO.LayerMask);

            var _displacement = _speedMultiplier * Time.fixedDeltaTime * transform.forward;
            var _hits = Physics.SphereCastNonAlloc(transform.position, _dummyCollider.radius, _displacement.normalized, _results, _displacement.magnitude, _projectileSO.LayerMask);
            var _nextPosition = _rb.position + _displacement;

            for (int i = 0; i < _hits; i++)
            {
                var _raycastHit = _results[i];
                var _colliderHit = _raycastHit.collider;
                Debug.Log($"_colliderHit = {_colliderHit}");

                if (HasHitSource(_colliderHit.gameObject)) continue;
                if (_collidersHit.Contains(_colliderHit)) continue;
                //if (_colliderHit.TryGetComponent(out HealthBehaviour _healthBehaviour) && !HasAvailableTag(_colliderHit.gameObject)) continue;

                if (_stats.CanBounce && HasBounceTag(_colliderHit.gameObject))
                {
                    TryBounce(_raycastHit);
                    //TryDamage(_healthBehaviour, _raycastHit);
                    continue;
                }
                else
                {
                    _collidersHit.Add(_colliderHit);
                    //TryDamage(_healthBehaviour, _raycastHit);
                    SendRaycastHitEvent(_raycastHit);
                    IncreasePierceCount();

                    if (HasReachedMaxPierceCount() || HasBlockPierceTag(_colliderHit.gameObject))
                    {
                        DestroyThis();
                        break;
                    }
                }
            }

            _rb.MovePosition(_nextPosition);
            //SetLastPosition();
        }

        public override void Init(ShootInfo _newShootModel)
        {
            base.Init(_newShootModel);
            _lastPosition = transform.position;
            _speedMultiplier = Random.Range(_stats.MoveSpeedRange.x, _stats.MoveSpeedRange.y);
            //_rb.linearVelocity = transform.forward * _speedMultiplier;
            _collidersHit.Clear();
            UpdateScale();
            InitAccelerationParameters();
        }

        private void UpdateScale()
        {
            _renderTransform.localScale *= _stats.ScaleMultiplier;
            _dummyCollider.radius *= _stats.ScaleMultiplier;
        }

        private void InitAccelerationParameters()
        {
            _defaultVelocity = _rb.linearVelocity;
            _accelerationTime = 0f;
        }

        private void CheckAcceleration()
        {
            if (_stats.UseAccelerationCurve)
            {
                _accelerationTime += Time.fixedDeltaTime * _stats.AcelerationMultiplier;
                float _curveValue = _stats.AccelerationCurve.Evaluate(_accelerationTime);

                Vector3 _newVelocity = _stats.InvertAcceleration ?
                    Vector3.Lerp(Vector2.zero, _defaultVelocity, _curveValue) :
                    Vector3.Lerp(_defaultVelocity, Vector2.zero, _curveValue);

                _rb.linearVelocity = _newVelocity;
            }

            if (_rb.linearVelocity == Vector3.zero && _stats.DestroyOnStop)
            {
                DestroyByStop();
            }
        }

        //private void TryDamage(HealthBehaviour _healthBehaviour, RaycastHit2D _raycastHit)
        //{
        //    if (_healthBehaviour is null) return;

        //    var _damage = _shootModel.GetDamage();
        //    var _damageModel = new DamageModel(_shootModel.EntitySource, _raycastHit.point, _damage);
        //    _healthBehaviour.TakeDamage(_damageModel);
        //}

        private void TryBounce(RaycastHit _raycastHit)
        {
            var _dot = Vector3.Dot(transform.right, _raycastHit.normal);
            var _hasHitAnOppositeNormal = _dot < 0;

            if (_hasHitAnOppositeNormal)
            {
                SendRaycastHitEvent(_raycastHit);
                Reflect(_raycastHit);
            }
        }

        private void Reflect(RaycastHit _raycastHit)
        {
            var _reflect = Vector2.Reflect(transform.right, _raycastHit.normal);
            transform.right = _reflect;
            _rb.linearVelocity = transform.right * _speedMultiplier;
            _defaultVelocity = _rb.linearVelocity;
        }

        //private void SetLastPosition()
        //{
        //    _lastPosition = transform.position;
        //}

        public float GetAccelerationCurveValue()
        {
            return _stats.AccelerationCurve.Evaluate(_accelerationTime);
        }
    }
}

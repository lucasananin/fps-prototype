using System.Collections.Generic;
using lutilities.Damage;
using lutilities.General;
using UnityEngine;

namespace FPS
{
    public class BallisticProjectile : AbstractProjectile
    {
        [SerializeField] List<Collider> _collidersHit = default;
        //[SerializeField] Vector3 _defaultVelocity = default;
        //[SerializeField] float _accelerationTime = 0f;
        [SerializeField] float _moveSpeed = 0f;

        [Header("// Ballistic Projectile - Properties")]
        [SerializeField] Rigidbody _rb = null;
        [SerializeField] SphereCollider _dummyCollider = null;
        //[SerializeField] Transform _renderTransform = null;

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
            //CheckAcceleration();
            CheckDestroyTime();
            CheckCollisions();
        }

        private void CheckCollisions()
        {
            var _displacement = _moveSpeed * Time.fixedDeltaTime * transform.forward;
            var _hits = Physics.SphereCastNonAlloc(transform.position, _dummyCollider.radius, _displacement.normalized, _results, _displacement.magnitude, _so.LayerMask);
            var _nextPosition = _rb.position + _displacement;

            if (_hits > 1)
            {
                CollectionMethods.SortByDistance(_results, transform.position, hit => hit.collider == null ? Vector3.positiveInfinity : hit.transform.position);
            }

            for (int i = 0; i < _hits; i++)
            {
                var _raycastHit = _results[i];
                var _colliderHit = _raycastHit.collider;

                if (HasHitSource(_colliderHit.gameObject)) continue;
                if (_collidersHit.Contains(_colliderHit)) continue;
                if (_colliderHit.TryGetComponent(out Health _healthBehaviour) && !HasDamageableTag(_colliderHit.gameObject)) continue;

                if (HasChanceToToBounce() && _healthBehaviour == null /*&& HasBounceTag(_colliderHit.gameObject)*/)
                {
                    TryBounce(_raycastHit);
                    //TryDamage(_healthBehaviour, _raycastHit);

                    if (HasReacheMaxBounceCount())
                    {
                        DestroyThis();
                    }

                    break;
                }
                else
                {
                    _collidersHit.Add(_colliderHit);
                    TryDamage(_healthBehaviour, _raycastHit);
                    SendRaycastHitEvent(_raycastHit);
                    IncreasePierceCount();

                    if (HasReachedMaxPierceCount() || _healthBehaviour == null /*|| HasBlockPierceTag(_colliderHit.gameObject)*/)
                    {
                        DestroyThis();
                        break;
                    }
                }
            }

            _rb.MovePosition(_nextPosition);
        }

        public override void Init(ShootInfo _newShootModel)
        {
            base.Init(_newShootModel);
            //_speedMultiplier = Random.Range(_stats.MoveSpeedRange.x, _stats.MoveSpeedRange.y);
            _moveSpeed = _stats.MoveSpeed;
            _collidersHit.Clear();
            //UpdateScale();
            //InitAccelerationParameters();
        }

        //private void UpdateScale()
        //{
        //    _renderTransform.localScale *= _stats.ScaleMultiplier;
        //    _dummyCollider.radius *= _stats.ScaleMultiplier;
        //}

        //private void InitAccelerationParameters()
        //{
        //    _defaultVelocity = _rb.linearVelocity;
        //    _accelerationTime = 0f;
        //}

        //private void CheckAcceleration()
        //{
        //    if (_stats.UseAccelerationCurve)
        //    {
        //        _accelerationTime += Time.fixedDeltaTime * _stats.AcelerationMultiplier;
        //        float _curveValue = _stats.AccelerationCurve.Evaluate(_accelerationTime);

        //        Vector3 _newVelocity = _stats.InvertAcceleration ?
        //            Vector3.Lerp(Vector2.zero, _defaultVelocity, _curveValue) :
        //            Vector3.Lerp(_defaultVelocity, Vector2.zero, _curveValue);

        //        _rb.linearVelocity = _newVelocity;
        //    }

        //    if (_rb.linearVelocity == Vector3.zero && _stats.DestroyOnStop)
        //    {
        //        DestroyByStop();
        //    }
        //}

        private void TryDamage(Health _healthBehaviour, RaycastHit _raycastHit)
        {
            if (_healthBehaviour == null) return;

            var _damage = _shootInfo.GetDamage();
            //var _damageModel = new DamageInfo(_shootInfo.EntitySource, _raycastHit.point, _damage);
            var _damageInfo = new DamageInfo(_damage, gameObject, _shootInfo.EntitySource.gameObject, _raycastHit.point, transform.forward);
            _healthBehaviour.TakeDamage(_damageInfo);
        }

        private void TryBounce(RaycastHit _raycastHit)
        {
            var _dot = Vector3.Dot(transform.forward, _raycastHit.normal);
            var _hasHitAnOppositeNormal = _dot < 0;

            if (_hasHitAnOppositeNormal)
            {
                SendRaycastHitEvent(_raycastHit);
                Reflect(_raycastHit);
            }
        }

        private void Reflect(RaycastHit _raycastHit)
        {
            var _reflect = Vector3.Reflect(transform.forward, _raycastHit.normal);
            transform.rotation = Quaternion.LookRotation(_reflect, Vector3.up);
            IncreaseBounceCount();
        }

        //public float GetAccelerationCurveValue()
        //{
        //    return _stats.AccelerationCurve.Evaluate(_accelerationTime);
        //}
    }
}

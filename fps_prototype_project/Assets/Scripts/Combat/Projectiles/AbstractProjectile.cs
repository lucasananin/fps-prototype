using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public abstract class AbstractProjectile : MonoBehaviour
    {
        [Header("// Projectile - Debug")]
        [SerializeField] protected ShootInfo _shootInfo = null;
        [SerializeField] protected ProjectileSO _projectileSO = null;
        [SerializeField] protected ProjectileStats _stats = null;
        [SerializeField] protected float _timeUntilDestroy = 0f;
        [SerializeField] protected float _destroyTimer = 0f;
        [SerializeField] protected int _currentPierceCount = 0;
        [SerializeField] protected List<string> _tagsList = null;

        private readonly Collider[] _explosionResults = new Collider[9];
        private readonly RaycastHit[] _obstacleResults = new RaycastHit[9];
        private readonly List<AbstractEntity> _entitiesFound = new();

        public event System.Action<ShootInfo> OnInit = null;
        public event System.Action<RaycastHit> OnRaycastHit = null;
        public event System.Action OnDestroy_TimerEnd = null;
        public event System.Action OnDestroy_Stop = null;
        public event System.Action OnExplode = null;

        const string PROJECTILE_TAG = "Projectile";

        public ShootInfo ShootModel { get => _shootInfo; }
        public float TimeUntilDestroy { get => _timeUntilDestroy; }

        public virtual void Init(ShootInfo _shootInfoValue)
        {
            _shootInfo = _shootInfoValue;
            _projectileSO = _shootInfoValue.ProjectileSO;
            _stats = _shootInfoValue.WeaponSource.WeaponSO.ProjectileStats;
            UpdateTagsList();
            SetDestroyTimer();
            TryAutoRotate();
        }

        protected void SendInitEvent()
        {
            OnInit?.Invoke(_shootInfo);
        }

        protected void SendRaycastHitEvent(RaycastHit _value)
        {
            OnRaycastHit?.Invoke(_value);
        }

        protected void CheckDestroyTime()
        {
            _destroyTimer += Time.fixedDeltaTime;

            if (_destroyTimer >= _timeUntilDestroy)
            {
                DestroyByTime();
            }
        }

        protected void SetDestroyTimer()
        {
            _timeUntilDestroy = Random.Range(_stats.DestroyTimeRange.x, _stats.DestroyTimeRange.y);
            _destroyTimer = 0f;
        }

        public void Explode()
        {
            Explode(transform.position);
        }

        public void Explode(Vector3 _point)
        {
            int _hits = Physics.OverlapSphereNonAlloc(_point, _stats.ExplosionRadius, _explosionResults, _projectileSO.LayerMask);

            for (int i = 0; i < _hits; i++)
            {
                var _colliderHit = _explosionResults[i];

                //if (_colliderHit.TryGetComponent(out HealthBehaviour _healthBehaviour))
                //{
                //    if (!_stats.CanDamageSource && HasHitSource(_colliderHit.gameObject)) continue;
                //    if (HasObstacleBetween(_point, _colliderHit.transform.position)) continue;

                //    var _damage = _shootModel.GetExplosiveDamage();
                //    var _damageModel = new DamageModel(_shootModel.EntitySource, transform.position, _damage);
                //    _healthBehaviour.TakeDamage(_damageModel);
                //}
            }

            OnExplode?.Invoke();
        }

        private bool HasObstacleBetween(Vector3 _origin, Vector3 _targetPoint)
        {
            var _vector = _targetPoint - _origin;
            var _rayHitCount = Physics.RaycastNonAlloc(_origin, _vector.normalized, _obstacleResults, _vector.magnitude, _projectileSO.ObstacleLayerMask);
            bool _hitObstacle = _rayHitCount > 0;
            return _hitObstacle;
        }

        private void TryAutoRotate()
        {
            var _so = _projectileSO;

            if (!_stats.CanAutoAim) return;

            _entitiesFound.Clear();
            var _myPosition = transform.position;
            var _hits = Physics.OverlapSphereNonAlloc(_myPosition, _stats.AutoAimDistance, _explosionResults, _so.EntityLayerMask);

            for (int i = 0; i < _hits; i++)
            {
                var _colliderHit = _explosionResults[i];

                if (HasHitSource(_colliderHit.gameObject)) continue;

                if (_colliderHit.TryGetComponent(out AbstractEntity _entity))
                {
                    _entitiesFound.Add(_entity);
                }
            }

            //_entitiesFound = GeneralMethods.OrderListByDistance(_entitiesFound, _myPosition);

            //int _count = _entitiesFound.Count;

            //for (int i = 0; i < _count; i++)
            //{
            //    var _entityPosition = _entitiesFound[i].transform.position;
            //    var _angle = GeneralMethods.CalculateAngle(_entityPosition, transform);

            //    if (_angle < _stats.AutoAimAngle / 2f)
            //    {
            //        transform.rotation = GeneralMethods.GetLookRotation(_myPosition, _entityPosition);
            //        break;
            //    }
            //}
        }

        public float GetExplosionRadius()
        {
            return _stats.ExplosionRadius;
        }

        public void IncreasePierceCount()
        {
            _currentPierceCount++;
        }

        protected void UpdateTagsList()
        {
            _tagsList.Clear();
            //_tagsList.AddRange(_shootModel.EntitySource.GetProjectileHitTags());
            if (_stats.CanDamageProjectiles) _tagsList.Add(PROJECTILE_TAG);
        }

        public bool HasReachedMaxPierceCount()
        {
            return _currentPierceCount >= _stats.MaxPierceCount;
        }

        public bool HasHitSource(GameObject _gameobjectHit)
        {
            return _gameobjectHit == _shootInfo.EntitySource.gameObject;
        }

        public bool HasAvailableTag(GameObject _gameObjectHit)
        {
            //return GeneralMethods.HasAvailableTag(_gameObjectHit, _tagsList);
            return false;
        }

        public bool HasBounceTag(GameObject _gameObjectHit)
        {
            //return GeneralMethods.HasAvailableTag(_gameObjectHit, _stats.BounceTags.Tags);
            return false;
        }

        public bool HasBlockPierceTag(GameObject _gameObjectHit)
        {
            //return GeneralMethods.HasAvailableTag(_gameObjectHit, _stats.BlockPierceTags.Tags);
            return false;
        }

        public float GetExplodeTimeNormalized()
        {
            return Mathf.InverseLerp(0, _timeUntilDestroy, _destroyTimer);
        }

        protected IEnumerator DestroyRoutine()
        {
            yield return new WaitForSeconds(_timeUntilDestroy);
            DestroyByTime();
        }

        public void DestroyByTime()
        {
            OnDestroy_TimerEnd?.Invoke();
            DestroyThis();
        }

        public void DestroyByStop()
        {
            OnDestroy_Stop?.Invoke();
            DestroyThis();
        }

        public void DestroyThis()
        {
            Destroy(gameObject);
        }
    }
}
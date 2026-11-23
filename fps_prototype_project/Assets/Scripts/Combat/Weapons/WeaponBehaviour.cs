using UnityEngine;
using UnityEngine.Events;

namespace FPS
{
    public class WeaponBehaviour : MonoBehaviour
    {
        [SerializeField] protected WeaponSO _so = null;
        [SerializeField] protected Transform _muzzle = null;
        //[SerializeField] private bool _alignToScreenCenter = true;

        private Camera _camera;
        private LayerMask _aimMask = default;
        private float _maxAimDistance = 100f;

        protected AbstractEntity _entitySource = null;
        protected AmmoHandler _ammoHandler = null;

        protected float _nextFire = 0;
        protected float _chargeTimer = 0f;
        protected bool _isCharging = false;

        protected float _currentHeat = 0f;
        protected float _overheatTimer = 0f;
        protected bool _isOverheated = false;

        protected float _nextBurst = 0;
        protected int _currentShootCount = 0;
        protected bool _isBursting = false;
        protected bool _isHoldingTrigger = false;

        public float _currentSpreadAngle = 0f;
        public float _precisionRecoveryTimer = 0f;

        public event UnityAction<WeaponBehaviour> OnInit = null;
        public event UnityAction OnShoot = null;
        public event UnityAction OnPullTrigger = null;
        public event UnityAction OnReleaseTrigger = null;

        public WeaponSO SO { get => _so; }
        public Transform Muzzle { get => _muzzle; }
        public float ChargingTime { get => _so.Stats.ChargeDuration; }

        protected virtual void Awake()
        {
            _nextFire = _so.Stats.FireRate;
            _nextBurst = _so.Stats.BurstRate;
            _currentSpreadAngle = _so.Stats.BaseSpreadAngle;
        }

        private void FixedUpdate()
        {
            UpdateTriggers();
        }

        private void Update()
        {
            DecreaseHeat();
            UpdateOverheatState();
            UpdatePrecision();
        }

        public virtual void Init(WeaponInitInfo _info)
        {
            _entitySource = _info.entity;
            _ammoHandler = _info.ammoHandler;
            _camera = _info.camera;
            _aimMask = _info.aimMask;
            _maxAimDistance = _info.maxAimDistance;
            OnInit?.Invoke(this);
        }

        #region Triggers

        public void UpdateTriggers()
        {
            if (_so.IsTriggerEqual(TriggerType.Semi_Automatic))
            {
                if (_so.HasChargeTime() && _isHoldingTrigger)
                {
                    UpdateChargeTimer();

                    if (HasEnoughChargeTimer() && _so.Stats.AutoFireOnFullCharge)
                        _isBursting = true;
                }

                if (_isBursting)
                {
                    _isCharging = false;
                    _chargeTimer = 0;

                    _nextFire += _nextFire < _so.Stats.FireRate ? Time.fixedDeltaTime : 0;

                    if (_nextFire >= _so.Stats.FireRate)
                    {
                        Shoot();

                        _currentShootCount++;

                        if (_currentShootCount >= _so.Stats.ShotsPerBurst)
                        {
                            _nextFire = _so.Stats.FireRate;
                            _isBursting = false;
                        }
                    }
                }
                else
                {
                    _nextBurst += _nextBurst < _so.Stats.BurstRate ? Time.fixedDeltaTime : 0;
                }
            }
            else if (_so.IsTriggerEqual(TriggerType.Full_Automatic))
            {
                _nextFire += _nextFire < _so.Stats.FireRate ? Time.fixedDeltaTime : 0;

                UpdateChargeTimer();

                if (!HasAmmo()) return;
                if (_isOverheated) return;
                if (!HasEnoughChargeTimer()) return;

                if (_nextFire >= _so.Stats.FireRate && _isHoldingTrigger)
                {
                    Shoot();
                }
            }
        }

        public virtual void PullTrigger()
        {
            if (_so.IsTriggerEqual(TriggerType.Semi_Automatic))
            {
                if (!HasAmmo()) return;
                if (_isOverheated) return;
                if (_nextBurst < _so.Stats.BurstRate) return;

                _nextBurst -= _so.Stats.BurstRate;
                _currentShootCount = 0;

                if (!_so.HasChargeTime()) _isBursting = true;
            }

            if (_so.HasChargeTime())
                _isCharging = true;

            _isHoldingTrigger = true;
            OnPullTrigger?.Invoke();
        }

        public virtual void ReleaseTrigger()
        {
            if (_so.IsTriggerEqual(TriggerType.Semi_Automatic))
            {
                if (!_so.Stats.AutoFireOnFullCharge && HasEnoughChargeTimer() && _so.HasChargeTime())
                {
                    _isBursting = true;
                }

                _chargeTimer = 0;
            }

            _isCharging = false;
            _isHoldingTrigger = false;
            OnReleaseTrigger?.Invoke();
        }

        #endregion

        public virtual void Shoot()
        {
            _nextFire = 0;
            PrepareProjectile(_so.ProjectileSO);
            DecreaseAmmo(_so.ProjectileSO);
            IncreaseHeat(_so.Stats.HeatPerShot);
            IncreaseShootAngle();
            OnShoot?.Invoke();
        }

        private void PrepareProjectile(ProjectileSO _projectileSO)
        {
            var _aimRotation = GetAimRotation();

            for (int i = 0; i < _so.Stats.ProjectilesPerShot; i++)
            {
                var _position = CalculateProjectilePosition(_projectileSO);
                var _rotation = _aimRotation * _so.GetProjectileRotation(_currentSpreadAngle, i);
                var _projectileInstance = Instantiate(_projectileSO.Prefab, _position, _rotation);
                var _shootInfo = new ShootInfo(_entitySource, this, _projectileSO);
                _projectileInstance.Init(_shootInfo);
            }
        }

        protected Quaternion GetAimRotation()
        {
            //if (!_alignToScreenCenter)
            //    return transform.rotation;

            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                return transform.rotation;

            Ray _ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            Vector3 _aimPoint;
            if (Physics.Raycast(_ray, out RaycastHit _hit, _maxAimDistance, _aimMask, QueryTriggerInteraction.Ignore))
                _aimPoint = _hit.point;
            else
                _aimPoint = _ray.GetPoint(_maxAimDistance);

            Vector3 _direction = _aimPoint - _muzzle.position;

            // Aim point is too close or behind the muzzle (e.g. wall right in front of the camera)
            if (_direction.sqrMagnitude < 0.01f || Vector3.Dot(_direction, _camera.transform.forward) < 0f)
                return transform.rotation;

            return Quaternion.LookRotation(_direction, Vector3.up);
        }

        protected Vector3 CalculateProjectilePosition(ProjectileSO _projectileSO)
        {
            var _spawnOffset = _projectileSO.SpawnPositionOffset;
            Vector3 _xOffset = transform.right * _spawnOffset.x;
            Vector3 _yOffset = transform.up * _spawnOffset.y;
            Vector3 _zOffset = transform.forward * _spawnOffset.z;
            return _muzzle.position + _xOffset + _yOffset + _zOffset;
        }

        #region PRECISION

        protected virtual void IncreaseShootAngle()
        {
            var _stats = _so.Stats;

            float _maxAngle = _stats.BaseSpreadAngle + _stats.MaxAdditionalShootAngle;

            _currentSpreadAngle = Mathf.Min(
                _currentSpreadAngle + _stats.ShootAngleIncreasePerShot,
                _maxAngle
            );

            _precisionRecoveryTimer = 0f;
        }

        protected virtual void UpdatePrecision()
        {
            var _stats = _so.Stats;

            if (_currentSpreadAngle <= _stats.BaseSpreadAngle)
                return;

            _precisionRecoveryTimer += Time.deltaTime;

            if (_precisionRecoveryTimer < _stats.PrecisionRecoveryDelay)
                return;

            _currentSpreadAngle = Mathf.MoveTowards(
                _currentSpreadAngle,
                _stats.BaseSpreadAngle,
                _stats.PrecisionRecoverySpeed * Time.deltaTime
            );
        }

        #endregion

        protected void UpdateChargeTimer()
        {
            if (!_so.HasChargeTime()) return;

            if (_isCharging)
            {
                _chargeTimer += _chargeTimer < _so.Stats.ChargeDuration ? Time.fixedDeltaTime : 0;
            }
            else
            {
                _chargeTimer -= _chargeTimer > 0 ? Time.fixedDeltaTime : 0;
            }
        }

        protected bool HasEnoughChargeTimer()
        {
            //if (!_so.HasChargeTime()) return true;
            return _chargeTimer >= _so.Stats.ChargeDuration;
        }

        #region HEAT

        protected void IncreaseHeat(float _value)
        {
            if (!_so.CanOverheat()) return;

            _currentHeat += _value;

            if (_currentHeat >= _so.Stats.MaxHeat)
            {
                _currentHeat = _so.Stats.MaxHeat;
                _overheatTimer = 0;
                _isOverheated = true;
            }
        }

        protected void DecreaseHeat()
        {
            if (!_so.CanOverheat()) return;

            var _overheatMultiplier = _isOverheated ? _so.Stats.OverheatDecreaseMultiplier : 1;
            _currentHeat -= _currentHeat > 0 ? _so.Stats.HeatDecreasePerSecond * _overheatMultiplier * Time.deltaTime : 0;
        }

        protected void UpdateOverheatState()
        {
            if (!_so.CanOverheat()) return;

            if (_isOverheated)
            {
                _overheatTimer += _overheatTimer < _so.Stats.OverheatDuration ? Time.deltaTime : 0;

                if (_overheatTimer >= _so.Stats.OverheatDuration)
                {
                    _isOverheated = false;
                }
            }
        }

        #endregion

        public float GetPullTriggerTotalTime()
        {
            return _so.GetPullTriggerTotalTime();
        }

        public float GetTimeUntilAnotherShot()
        {
            return _overheatTimer + _so.GetTimeUntilAnotherShot();
        }

        public string GetId()
        {
            return _so.Id;
        }

        private void DecreaseAmmo(ProjectileSO _projectileSO)
        {
            if (_ammoHandler != null)
                _ammoHandler.DecreaseAmmo(_projectileSO, _so);
        }

        public bool HasAmmo()
        {
            return _ammoHandler == null || _ammoHandler.HasAmmo(_so.ProjectileSO, _so);
        }

        public string GetAmmoString()
        {
            return _ammoHandler == null ? $"-" : $"{_ammoHandler.GetAmmoQuantity(_so.ProjectileSO)}";
        }

        public int GetDamage()
        {
            return _so.Stats.Damage;
        }

        public int GetExplosiveDamage()
        {
            return _so.Stats.ExplosiveDamage;
        }

        public float GetCastProjectileMaxDistance()
        {
            return _so.ProjectileStats.MaxCastDistance;
        }

        public virtual void ResetRuntimeValues()
        {
            _chargeTimer = 0f;
            _isCharging = false;
            _isBursting = false;
            _isHoldingTrigger = false;
            _currentSpreadAngle = _so.Stats.BaseSpreadAngle;
            _precisionRecoveryTimer = 0f;
        }
    }
}
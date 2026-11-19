using UnityEngine;
using UnityEngine.Events;

namespace FPS
{
    public class WeaponBehaviour : MonoBehaviour
    {
        [Header("// Weapon")]
        [SerializeField] protected WeaponSO _weaponSO = null;
        [SerializeField] protected Transform _muzzle = null;

        [Header("// Debug - Weapon")]
        [SerializeField] protected AbstractEntity _entitySource = null;
        [SerializeField] protected AmmoHandler _ammoHandler = null;
        [SerializeField] protected float _nextFire = 0;
        [SerializeField] protected float _chargeTimer = 0f;
        [SerializeField] protected bool _isCharging = false;
        [SerializeField] protected float _currentHeat = 0f;
        [SerializeField] protected float _overheatTimer = 0f;
        [SerializeField] protected bool _isOverheated = false;

        private float _nextBurst = 0;
        private int _currentShootCount = 0;
        private bool _isBursting = false;
        private bool _isHoldingTrigger = false;

        public event UnityAction<WeaponBehaviour> OnInit = null;
        public event UnityAction OnShoot = null;
        public event UnityAction OnPullTrigger = null;
        public event UnityAction OnReleaseTrigger = null;

        const float HEAT_OFFSET = 0.9f;

        public WeaponSO WeaponSO { get => _weaponSO; private set => _weaponSO = value; }
        public Transform Muzzle { get => _muzzle; private set => _muzzle = value; }
        public float ChargingTime { get => _weaponSO.Stats.ChargeTime; }

        protected virtual void Awake()
        {
            _nextFire = _weaponSO.Stats.FireRate;
            _nextBurst = _weaponSO.Stats.BurstRate;
        }

        private void FixedUpdate()
        {
            //if (!HasAmmo()) return;
            //if (_isOverheated) return;

            if (_weaponSO.TriggerType == TriggerType.Semi_Automatic)
            {
                //if (CanAutoChargeShot())
                //{
                //    _isCharging = false;
                //    _hasShotCharge = true;
                //    ShootChargedShot();
                //}

                if (_weaponSO.HasChargeTime() && _isHoldingTrigger)
                {
                    UpdateChargeTimer();

                    if (HasEnoughChargeTimer())
                        _isBursting = true;
                }

                if (_isBursting)
                {
                    _isCharging = false;
                    _chargeTimer = 0;

                    _nextFire += _nextFire < _weaponSO.Stats.FireRate ? Time.fixedDeltaTime : 0;

                    if (_nextFire >= _weaponSO.Stats.FireRate)
                    {
                        Shoot();

                        _currentShootCount++;

                        if (_currentShootCount >= _weaponSO.Stats.ShotsPerBurst)
                        {
                            _nextFire = _weaponSO.Stats.FireRate;
                            _isBursting = false;
                        }
                    }
                }
                else
                {
                    _nextBurst += _nextBurst < _weaponSO.Stats.BurstRate ? Time.fixedDeltaTime : 0;
                }
            }
            else if (_weaponSO.TriggerType == TriggerType.Full_Automatic)
            {
                _nextFire += _nextFire < _weaponSO.Stats.FireRate ? Time.fixedDeltaTime : 0;

                UpdateChargeTimer();

                if (!HasEnoughChargeTimer()) return;
                if (_isOverheated) return;
                if (!HasAmmo()) return;

                if (_nextFire >= _weaponSO.Stats.FireRate && _isHoldingTrigger /*&& !_isOverheated*/)
                {
                    Shoot();
                }
            }
        }

        protected virtual void Update()
        {
            DecreaseHeat();
        }

        public virtual void Init(AbstractEntity _entityBehaviour, AmmoHandler _ammoHandler)
        {
            this._ammoHandler = _ammoHandler;
            _entitySource = _entityBehaviour;
            OnInit?.Invoke(this);
        }

        public virtual void PullTrigger()
        {
            //if (!HasAmmo()) return;
            //if (_isOverheated) return;

            if (_weaponSO.TriggerType == TriggerType.Semi_Automatic)
            {
                if (!HasAmmo()) return;
                if (_isOverheated) return;
                if (_nextBurst < _weaponSO.Stats.BurstRate) return;

                _nextBurst -= _weaponSO.Stats.BurstRate;
                _currentShootCount = 0;

                if (!_weaponSO.HasChargeTime()) _isBursting = true;

                //_isBursting = true;
                //_isHoldingTrigger = true;
            }
            else if (_weaponSO.TriggerType == TriggerType.Full_Automatic)
            {
                //_isCharging = true;
                //_isHoldingTrigger = true;
            }

            _isHoldingTrigger = true;
            _isCharging = true;
            OnPullTrigger?.Invoke();
        }

        public virtual void ReleaseTrigger()
        {
            if (_weaponSO.TriggerType == TriggerType.Semi_Automatic)
            {
                //
            }
            else if (_weaponSO.TriggerType == TriggerType.Full_Automatic)
            {
                //_isCharging = false;
                //_isHoldingTrigger = false;
            }

            _isCharging = false;
            _isHoldingTrigger = false;
            OnReleaseTrigger?.Invoke();
        }

        public virtual void Shoot()
        {
            //_nextFire -= _weaponSO.FireRate;
            _nextFire = 0;
            //_chargeTimer = 0;
            //_isCharging = false;
            PrepareProjectile(_weaponSO.ProjectileSO/*, false*/);
            DecreaseAmmo(_weaponSO.ProjectileSO);
            IncreaseHeat(_weaponSO.Stats.HeatPerShot);
            OnShoot?.Invoke();
        }

        public virtual void ShootChargedShot()
        {
            //_nextFire -= _weaponSO.FireRate;
            //_chargeTimer -= _weaponSO.MaxChargeTime;
            _nextFire = 0;
            _chargeTimer = 0;
            _isCharging = false;
            PrepareProjectile(_weaponSO.ProjectileSO/*, true*/);
            DecreaseAmmo(_weaponSO.ProjectileSO);
            IncreaseHeat(_weaponSO.Stats.MaxHeat + HEAT_OFFSET);
            OnShoot?.Invoke();
        }

        private void PrepareProjectile(ProjectileSO _projectileSO/*, bool _isChargedShot*/)
        {
            for (int i = 0; i < _weaponSO.Stats.ProjectilesPerShot; i++)
            {
                Debug.Log($"Shoot!");
                continue;

                var _position = CalculateProjectilePosition(_projectileSO);
                var _rotation = CalculateProjectileRotation(i);
                //var _parent = _entitySource.transform;
                var _projectile = Instantiate(_projectileSO.Prefab, _position, _rotation/*, _parent*/);
                var _shootModel = new ShootModel(_entitySource, this, _projectileSO/*, _isChargedShot*/);
                _projectile.Init(_shootModel);
            }
        }

        protected Quaternion CalculateProjectileRotation(int _projectileIndex)
        {
            var _rotationOffset = _weaponSO.GetProjectileRotation(_projectileIndex);
            return transform.rotation * _rotationOffset;
        }

        protected Vector3 CalculateProjectilePosition(ProjectileSO _projectileSO)
        {
            if (_projectileSO.SpawnPositionOffset == Vector2.zero)
            {
                return _muzzle.position;
            }
            else
            {
                Vector3 _xOffset = transform.right * _projectileSO.SpawnPositionOffset.x;
                Vector3 _yOffset = transform.up * _projectileSO.SpawnPositionOffset.y;
                return _muzzle.position + _xOffset + _yOffset;
            }
        }

        protected void UpdateChargeTimer()
        {
            if (!_weaponSO.HasChargeTime()) return;

            if (_isCharging)
            {
                _chargeTimer += _chargeTimer < _weaponSO.Stats.ChargeTime ? Time.fixedDeltaTime : 0;
            }
            else
            {
                _chargeTimer -= _chargeTimer > 0 ? Time.fixedDeltaTime : 0;
            }
        }

        protected bool HasEnoughChargeTimer()
        {
            return _chargeTimer >= _weaponSO.Stats.ChargeTime;
        }

        protected void IncreaseHeat(float _value)
        {
            if (!_weaponSO.CanOverheat()) return;

            _currentHeat += _value;

            if (_currentHeat >= _weaponSO.Stats.MaxHeat)
            {
                _currentHeat = _weaponSO.Stats.MaxHeat;
                _overheatTimer = 0;
                _isOverheated = true;
            }
        }

        protected void DecreaseHeat()
        {
            if (!_weaponSO.CanOverheat()) return;

            _currentHeat -= _currentHeat > 0 ? _weaponSO.Stats.HeatDecreasePerSecond * Time.deltaTime : 0;

            if (_isOverheated)
            {
                _overheatTimer += _overheatTimer < _weaponSO.Stats.MaxOverheatTime ? Time.deltaTime : 0;

                if (_overheatTimer >= _weaponSO.Stats.MaxOverheatTime)
                {
                    _isOverheated = false;
                }
            }
        }

        public float GetPullTriggerTotalTime()
        {
            return _weaponSO.GetPullTriggerTotalTime();
        }

        public float GetTimeUntilAnotherShot()
        {
            return _overheatTimer + _weaponSO.GetTimeUntilAnotherShot();
        }

        public string GetId()
        {
            return _weaponSO.Id;
        }

        private void DecreaseAmmo(ProjectileSO _projectileSO)
        {
            if (_ammoHandler != null)
                _ammoHandler.DecreaseAmmo(_projectileSO, _weaponSO);
        }

        public bool HasAmmo()
        {
            return _ammoHandler == null || _ammoHandler.HasAmmo(_weaponSO.ProjectileSO, _weaponSO);
        }

        //public bool HasChargeAmmo()
        //{
        //    return _ammoHandler is null ? true : _ammoHandler.HasAmmo(_weaponSO.ChargedProjectileSO, _weaponSO);
        //}

        public string GetAmmoString()
        {
            return _ammoHandler == null ? $"-" : $"{_ammoHandler.GetAmmoQuantity(_weaponSO.ProjectileSO)}";
        }

        public int GetDamage(/*bool _isChargedShot*/)
        {
            return _weaponSO.Stats.Damage;
            //return _isChargedShot ? _weaponSO.Stats.ChargeShotDamage : _weaponSO.Stats.Damage;
        }

        public int GetExplosiveDamage()
        {
            return _weaponSO.Stats.ExplosiveDamage;
        }

        public float GetCastProjectileMaxDistance()
        {
            return _weaponSO.ProjectileStats.MaxCastDistance;
        }

        public virtual void ResetRuntimeValues()
        {
            //_nextFire = 0;
            _chargeTimer = 0f;
            _isCharging = false;
            _isBursting = false;
            _isHoldingTrigger = false;
        }
    }

    public enum TriggerType
    {
        Semi_Automatic,
        Full_Automatic,
    }
}
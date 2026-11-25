using UnityEngine;

namespace FPS
{
    [System.Serializable]
    public class ShootInfo
    {
        [SerializeField] AbstractEntity _entitySource = null;
        [SerializeField] WeaponBehaviour _weaponSource = null;
        [SerializeField] ProjectileSO _projectileSO = null;
        [SerializeField] bool _isLastShot = false;

        public AbstractEntity EntitySource { get => _entitySource; }
        public WeaponBehaviour WeaponSource { get => _weaponSource; }
        public ProjectileSO ProjectileSO { get => _projectileSO; }

        public ShootInfo(AbstractEntity _entitySource, WeaponBehaviour _weaponSource, ProjectileSO _projectileSO, bool _isLastShot)
        {
            this._entitySource = _entitySource;
            this._weaponSource = _weaponSource;
            this._projectileSO = _projectileSO;
            this._isLastShot = _isLastShot;
        }

        internal int GetDamage(float _hitDistance)
        {
            var _finalValue = 0;
            _finalValue += GetBaseDamage();
            _finalValue *= GetDistanceDamage(_hitDistance);
            return _finalValue;
        }

        private int GetBaseDamage()
        {
            return _isLastShot ? _weaponSource.GetLastShotDamage() : _weaponSource.GetDamage();
        }

        public int GetDistanceDamage(float _hitDistance)
        {
            var _stats = _weaponSource.SO.Stats;

            // Outside the bonus area: no bonus
            if (_hitDistance > _stats.MaxBonusDistance) return 1;

            // 0 at the edge of the bonus area, 1 at point blank
            float t = 1f - (_hitDistance / _stats.MaxBonusDistance);
            float _multiplier = Mathf.Lerp(_stats.MinDamageMultiplier, _stats.MaxDamageMultiplier, t);

            return (int)_multiplier;
        }

        public int GetExplosiveDamage()
        {
            return _weaponSource.GetExplosiveDamage();
        }
    }
}
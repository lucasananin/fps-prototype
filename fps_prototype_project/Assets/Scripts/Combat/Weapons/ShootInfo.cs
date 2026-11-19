using UnityEngine;

namespace FPS
{
    [System.Serializable]
    public class ShootInfo
    {
        [SerializeField] AbstractEntity _entitySource = null;
        [SerializeField] WeaponBehaviour _weaponSource = null;
        [SerializeField] ProjectileSO _projectileSO = null;

        public AbstractEntity EntitySource { get => _entitySource; }
        public WeaponBehaviour WeaponSource { get => _weaponSource; }
        public ProjectileSO ProjectileSO { get => _projectileSO; }

        public ShootInfo(AbstractEntity _entitySource, WeaponBehaviour _weaponSource, ProjectileSO _projectileSO)
        {
            this._entitySource = _entitySource;
            this._weaponSource = _weaponSource;
            this._projectileSO = _projectileSO;
        }

        public int GetDamage()
        {
            return _weaponSource.GetDamage();
        }

        public int GetExplosiveDamage()
        {
            return _weaponSource.GetExplosiveDamage();
        }
    }
}
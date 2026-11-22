using UnityEngine;

namespace FPS
{
    [CreateAssetMenu(fileName = "Weapon_", menuName = "Scriptable Objects/Combat/Weapon Data")]
    public class WeaponSO : ScriptableObject
    {
        [SerializeField] private string _id = "warmaker_mg";
        [SerializeField] private string _displayName = "Warmaker";
        [SerializeField] private string _typeName = "Machine Gun";
        [SerializeField] private WeaponBehaviour _prefab = null;
        [SerializeField] private ProjectileSO _projectileSO = null;
        [SerializeField] private WeaponStats _stats = null;
        [SerializeField] private ProjectileStats _projectileStats = null;

        public WeaponBehaviour Prefab { get => _prefab; }
        public string Id { get => _id; }
        public string DisplayName { get => _displayName; }
        public string TypeName { get => _typeName; }
        public ProjectileSO ProjectileSO { get => _projectileSO; }
        public WeaponStats Stats { get => _stats; }
        public ProjectileStats ProjectileStats { get => _projectileStats; }

        public bool IsTriggerEqual(TriggerType _type)
        {
            return _stats.TriggerType == _type;
        }

        public bool HasChargeTime()
        {
            return _stats.ChargeDuration > 0;
        }

        public bool CanOverheat()
        {
            return _stats.MaxHeat > 0;
        }

        public float GetPullTriggerTotalTime()
        {
            float _offset = 0.1f;
            return _stats.ChargeDuration + _offset;
        }

        public float GetTimeUntilAnotherShot()
        {
            float _totalFireRate = _stats.ShotsPerBurst > 0 ? _stats.FireRate * _stats.ShotsPerBurst : _stats.FireRate;
            float _offset = 0.1f;
            return _totalFireRate + _stats.BurstRate + _offset;
        }

        public Quaternion GetProjectileRotation(int _projectileIndex)
        {
            if (_stats.ShootAngle <= 0)
            {
                return Quaternion.identity;
            }

            float _halfAngle = _stats.ShootAngle / 2f;

            if (_stats.ArcMode == ShootArcMode.Spread && _stats.ProjectilesPerShot > 1)
            {
                // Evenly spaced pellets around the edge of the cone
                float _roll = 360f / _stats.ProjectilesPerShot * _projectileIndex;
                return Quaternion.AngleAxis(_roll, Vector3.forward)
                     * Quaternion.AngleAxis(_halfAngle, Vector3.up);
            }
            else
            {
                // Random direction inside the cone
                float _roll = Random.Range(0f, 360f);
                float _tilt = _halfAngle * Mathf.Sqrt(Random.value);
                return Quaternion.AngleAxis(_roll, Vector3.forward)
                     * Quaternion.AngleAxis(_tilt, Vector3.up);
            }
        }
    }

    [System.Serializable]
    public class WeaponStats
    {
        [Header("// Trigger")]
        [SerializeField] TriggerType _triggerType = default;

        [Header("// Damage")]
        [SerializeField, Range(0, 99)] int _damage = 1;
        [SerializeField, Range(0, 99)] int _explosiveDamage = 1;

        [Header("// Fire Rate")]
        [SerializeField, Range(0.01f, 2f)] float _fireRate = 0.1f;

        [Header("// Charge")]
        [SerializeField, Range(0f, 2f)] float _chargeDuration = 0f;

        [Header("// Heat")]
        [SerializeField, Range(0f, 99f)] float _maxHeat = 0f;
        [SerializeField, Range(0f, 99f)] float _heatPerShot = 0f;
        [SerializeField, Range(0f, 99f)] float _heatDecreasePerSecond = 0f;
        [SerializeField, Range(0f, 9f)] float _overheatDuration = 0f;
        [SerializeField, Range(1f, 9f)] float _overheatDecreaseMultiplier = 2f;

        [Header("// Precision")]
        [SerializeField, Range(0, 135)] float _shootAngle = 0;
        [SerializeReference] ShootArcMode _arcMode = default;

        [Header("// Ammo")]
        [SerializeField, Range(1, 36)] int _projectilesPerShot = 1;
        [SerializeField, Range(0, 36)] int _ammoPerShot = 1;

        [Header("// Semi-Auto Properties")]
        [SerializeField] bool _autoFireOnFullCharge = false;
        [SerializeField, Range(0f, 2f)] float _burstRate = 0f;
        [SerializeField, Range(0, 99)] int _shotsPerBurst = 0;

        public TriggerType TriggerType { get => _triggerType; }
        public int Damage { get => _damage; }
        public int ExplosiveDamage { get => _explosiveDamage; }
        public float FireRate { get => _fireRate; }
        public float ChargeDuration { get => _chargeDuration; }
        public float MaxHeat { get => _maxHeat; }
        public float HeatPerShot { get => _heatPerShot; }
        public float HeatDecreasePerSecond { get => _heatDecreasePerSecond; }
        public float OverheatDuration { get => _overheatDuration; }
        public float OverheatDecreaseMultiplier { get => _overheatDecreaseMultiplier; }
        public float ShootAngle { get => _shootAngle; }
        public ShootArcMode ArcMode { get => _arcMode; }
        public int ProjectilesPerShot { get => _projectilesPerShot; }
        public int AmmoPerShot { get => _ammoPerShot; }
        public bool AutoFireOnFullCharge { get => _autoFireOnFullCharge; }
        public float BurstRate { get => _burstRate; }
        public int ShotsPerBurst { get => _shotsPerBurst; }
    }

    public enum TriggerType
    {
        Semi_Automatic,
        Full_Automatic,
    }

    public enum ShootArcMode
    {
        Random,
        Spread,
    }
}

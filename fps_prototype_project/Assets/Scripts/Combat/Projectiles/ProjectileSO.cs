using UnityEngine;

namespace FPS
{
    [CreateAssetMenu(fileName = "Projectile_", menuName = "Scriptable Objects/Combat/Projectile")]
    public class ProjectileSO : ScriptableObject
    {
        [Header("// General")]
        [SerializeField] AbstractProjectile _prefab = null;
        //[SerializeField] AmmoSO _ammoSO = null;
        [SerializeField] LayerMask _layerMask = default;
        [SerializeField] LayerMask _entityLayerMask = default;
        [SerializeField] LayerMask _obstacleLayerMask = default;
        [SerializeField] Vector3 _spawnPositionOffset = default;

        public AbstractProjectile Prefab { get => _prefab; }
        //public AmmoSO AmmoSO { get => _ammoSO; }
        public LayerMask LayerMask { get => _layerMask; }
        public LayerMask EntityLayerMask { get => _entityLayerMask; }
        public LayerMask ObstacleLayerMask { get => _obstacleLayerMask; }
        public Vector3 SpawnPositionOffset { get => _spawnPositionOffset; }
    }

    [System.Serializable]
    public class ProjectileStats
    {
        [Header("// General")]
        [SerializeField] Vector2 _destroyTimeRange = default;
        //[SerializeField] bool _canDamageProjectiles = false;

        [Header("// Explosion")]
        [SerializeField, Range(0, 10)] float _explosionRadius = 0f;
        [SerializeField] bool _explodeOnHitAny = false;
        [SerializeField] bool _explodeOnHitEntity = false;
        [SerializeField] bool _explodeOnStop = false;
        [SerializeField] bool _explodeOnTimerEnd = false;
        //[SerializeField] bool _canDamageSource = false;

        [Header("// Piercing")]
        [SerializeField, Range(1, 99)] int _maxPierceCount = 1;
        //[SerializeField] TagCollection _blockPierceTags = null;

        //[Header("// Auto Aim")]
        //[SerializeField] bool _canAutoAim = false;
        //[SerializeField] float _autoAimDistance = 0f;
        //[SerializeField] float _autoAimAngle = 0f;

        [Header("// Ballistic - Properties")]
        //[SerializeField] Vector2 _moveSpeedRange = default;
        [SerializeField] float _moveSpeed = 100;
        //[SerializeField, Range(0.1f, 5f)] float _scaleMultiplier = 1f;
        //[SerializeField] bool _destroyOnStop = false;
        //[SerializeField] bool _useAccelerationCurve = false;
        //[SerializeField] bool _invertAcceleration = false;
        //[SerializeField] AnimationCurve _accelerationCurve = null;
        //[SerializeField, Range(0f, 10f)] float _acelerationMultiplier = 0;
        //[SerializeField] bool _canBounce = false;
        [SerializeField, Range(0, 100)] int _bounceChance = 0;
        [SerializeField, Range(1, 3)] int _maxBounces = 1;
        //[SerializeField] TagCollection _bounceTags = null;
        [SerializeField] bool _useGravity = true;
        [SerializeField, Range(0, 10)] float _gravityMultiplier = 1f; // 0 = straight line, 1 = realistic, >1 = heavier
        [SerializeField, Range(0f, 1f)] float _bounceEnergyKept = 1f; // optional: lose speed on bounces

        [Header("// Hitscan - Properties")]
        [SerializeField, Range(0f, 100f)] float _maxCastDistance = 0f;

        //[Header("// Guided Projectile: Properties")]
        //[SerializeField, Range(0f, 9f)] float _maxGuidedRadius = 0f;

        public Vector2 DestroyTimeRange { get => _destroyTimeRange; }
        //public bool CanDamageProjectiles { get => _canDamageProjectiles; }
        public float ExplosionRadius { get => _explosionRadius; }
        //public bool CanDamageSource { get => _canDamageSource; }
        public int MaxPierceCount { get => _maxPierceCount; }
        //public TagCollection BlockPierceTags { get => _blockPierceTags; }
        //public bool CanAutoAim { get => _canAutoAim; }
        //public float AutoAimDistance { get => _autoAimDistance; }
        //public float AutoAimAngle { get => _autoAimAngle; }
        //public Vector2 MoveSpeedRange { get => _moveSpeedRange; }
        public float MoveSpeed { get => _moveSpeed; }
        //public float ScaleMultiplier { get => _scaleMultiplier; }
        //public bool DestroyOnStop { get => _destroyOnStop; }
        //public bool UseAccelerationCurve { get => _useAccelerationCurve; }
        //public bool InvertAcceleration { get => _invertAcceleration; }
        //public AnimationCurve AccelerationCurve { get => _accelerationCurve; }
        //public float AcelerationMultiplier { get => _acelerationMultiplier; }
        //public bool CanBounce { get => _canBounce; }
        public int BounceChance { get => _bounceChance; }
        public int MaxBounces { get => _maxBounces; }
        //public TagCollection BounceTags { get => _bounceTags; }
        public bool UseGravity { get => _useGravity; }
        public float GravityMultiplier { get => _gravityMultiplier; }
        public float BounceEnergyKept { get => _bounceEnergyKept; }
        public float MaxCastDistance { get => _maxCastDistance; }
        public bool ExplodeOnHitAny { get => _explodeOnHitAny; }
        public bool ExplodeOnHitEntity { get => _explodeOnHitEntity; }
        public bool ExplodeOnStop { get => _explodeOnStop; }
        public bool ExplodeOnTimerEnd { get => _explodeOnTimerEnd; }
        //public float MaxGuidedRadius { get => _maxGuidedRadius; }
    }
}

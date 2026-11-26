using UnityEngine;

namespace FPS
{
    public class ProjectileExplosion : MonoBehaviour
    {
        [SerializeField] AbstractProjectile _projectile = null;
        [SerializeField] SimpleExplosionVfx _simpleExplosionPrefab = null;
        //[SerializeField] GameObject _particleSystemPrefab = null;

        private void Awake()
        {
            if (_projectile == null)
                _projectile = GetComponent<AbstractProjectile>();
        }

        private void OnEnable()
        {
            _projectile.OnRaycastHit += ExplodeByHit;
            _projectile.OnDestroy_Stop += ExplodeByStop;
            _projectile.OnDestroy_TimerEnd += ExplodeByTimer;
        }

        private void OnDisable()
        {
            _projectile.OnRaycastHit -= ExplodeByHit;
            _projectile.OnDestroy_Stop -= ExplodeByStop;
            _projectile.OnDestroy_TimerEnd -= ExplodeByTimer;
        }

        private void ExplodeByHit(RaycastHit _raycastHit)
        {
            if (_projectile.Stats.ExplodeOnHitAny)
            {
                Explode(_raycastHit.point);
            }
            else if (_projectile.Stats.ExplodeOnHitEntity && HasHitEntity(_raycastHit))
            {
                Explode(_raycastHit.point);
            }
        }

        private void ExplodeByStop()
        {
            if (_projectile.Stats.ExplodeOnStop)
                Explode(transform.position);
        }

        private void ExplodeByTimer()
        {
            if (_projectile.Stats.ExplodeOnTimerEnd)
                Explode(transform.position);
        }

        private void Explode(Vector3 _position)
        {
            _projectile.Explode(_position);

            if (_simpleExplosionPrefab != null)
            {
                var _vfxInstance = Instantiate(_simpleExplosionPrefab, _position, Quaternion.identity);
                _vfxInstance.Init(_projectile.GetExplosionRadius());
            }

            //if (_particleSystemPrefab != null)
            //{
            //    Instantiate(_particleSystemPrefab, _position, Quaternion.identity);
            //}
        }

        private bool HasHitEntity(RaycastHit _raycastHit)
        {
            return _raycastHit.collider.TryGetComponent(out AbstractEntity _)
                || _raycastHit.collider.transform.parent.TryGetComponent(out AbstractEntity _);
        }
    }
}
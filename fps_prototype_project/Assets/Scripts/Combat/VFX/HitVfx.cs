using UnityEngine;

namespace FPS
{
    public class HitVfx : MonoBehaviour
    {
        [SerializeField] AbstractProjectile _projectile = null;
        [SerializeField] ParticleSystem _defaultVfx = null;

        private void OnValidate()
        {
            _projectile = GetComponent<AbstractProjectile>();
        }

        private void OnEnable()
        {
            _projectile.OnRaycastHit += SpawnVfx;
        }

        private void OnDisable()
        {
            _projectile.OnRaycastHit -= SpawnVfx;
        }

        private void SpawnVfx(RaycastHit _hitInfo)
        {
            //var _holder = _hitInfo.collider.GetComponent<HitVfxHolder>();
            //var _prefab = _holder == null ? _defaultVfx : _holder.Prefab;
            var _prefab = _defaultVfx;
            var _instance = Instantiate(_prefab, _hitInfo.point, Quaternion.identity);
            _instance.transform.forward = _hitInfo.normal;
        }
    }
}
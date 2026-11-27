using System.Collections.Generic;
using FPS;
using lutilities.Damage;
using lutilities.General;
using UnityEngine;
using UnityEngine.Events;

public class HitscanProjectile : AbstractProjectile
{
    [Header("// Hitscan - Properties")]
    [SerializeField] SphereCollider _dummyCollider = null;

    private readonly RaycastHit[] _results = new RaycastHit[9];
    private readonly List<RaycastHit> _collidersHit = new();

    public event UnityAction<List<RaycastHit>> OnCastEnd = null;

    private void Awake()
    {
        if (_dummyCollider == null)
            _dummyCollider = GetComponent<SphereCollider>();

        _dummyCollider.enabled = false;
    }

    public override void Init(ShootInfo _newShootModel)
    {
        base.Init(_newShootModel);
        _collidersHit.Clear();

        int _hits = Physics.SphereCastNonAlloc(transform.position, _dummyCollider.radius, transform.forward, _results, _stats.MaxCastDistance, _so.LayerMask);

        if (_hits > 1)
        {
            CollectionMethods.SortByDistance(_results, transform.position, hit => hit.collider == null ? Vector3.positiveInfinity : hit.point);
        }

        for (int i = 0; i < _hits; i++)
        {
            var _raycastHit = _results[i];
            var _colliderHit = _raycastHit.collider;

            if (HasHitSource(_colliderHit.gameObject)) continue;
            if (_colliderHit.TryGetComponent(out Health _health) && !HasDamageableTag(_colliderHit.gameObject)) continue;

            TryDamage(_health, _raycastHit);

            _collidersHit.Add(_raycastHit);
            SendRaycastHitEvent(_raycastHit);
            IncreasePierceCount();

            if (HasReachedMaxPierceCount() || _health == null /*HasBlockPierceTag(_colliderHit.gameObject)*/)
            {
                break;
            }
        }

        OnCastEnd?.Invoke(_collidersHit);
        StartCoroutine(DestroyRoutine());
    }
}

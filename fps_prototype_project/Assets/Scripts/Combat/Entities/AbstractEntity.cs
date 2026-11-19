using lutilities.Damage;
using UnityEngine;

namespace FPS
{
    public abstract class AbstractEntity : MonoBehaviour
    {
        [SerializeField] protected AbstractEntitySO _entitySO = null;
        //[SerializeField] protected Health _healthBehaviour = null;

        public T GetEntitySO<T>() where T : AbstractEntitySO
        {
            return _entitySO as T;
        }

        //public bool IsAlive()
        //{
        //    return _healthBehaviour.IsAlive();
        //}

        //public string[] GetOpponentTags()
        //{
        //    return _entitySO.OpponentTags.Tags;
        //}

        //public string[] GetProjectileHitTags()
        //{
        //    return _entitySO.ProjectileHitTags.Tags;
        //}

        public abstract bool IsMoving();
    }
}
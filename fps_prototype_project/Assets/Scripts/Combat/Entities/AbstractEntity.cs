using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public abstract class AbstractEntity : MonoBehaviour
    {
        [SerializeField] protected AbstractEntitySO _so = null;
        //[SerializeField] protected Health _healthBehaviour = null;

        public T GetEntitySO<T>() where T : AbstractEntitySO
        {
            return _so as T;
        }

        //public bool IsAlive()
        //{
        //    return _healthBehaviour.IsAlive();
        //}

        public List<string> GetDamageableTags()
        {
            return _so.DamageableTags.GetTags();
        }

        public List<string> GetOpponentTags()
        {
            return _so.OpponentTags.GetTags();
        }

        public abstract bool IsMoving();
    }
}
using lutilities.TagCollections;
using UnityEngine;

namespace FPS
{
    public abstract class EntitySO : ScriptableObject
    {
        [SerializeField] protected TagCollection _opponentTags = null;
        [SerializeField] protected TagCollection _projectileHitTag = null;

        public TagCollection OpponentTags { get => _opponentTags; private set => _opponentTags = value; }
        public TagCollection ProjectileHitTags { get => _projectileHitTag; private set => _projectileHitTag = value; }
    }
}
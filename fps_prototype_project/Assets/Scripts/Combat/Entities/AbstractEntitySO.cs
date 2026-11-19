using lutilities.TagCollections;
using UnityEngine;

namespace FPS
{
    public abstract class AbstractEntitySO : ScriptableObject
    {
        [SerializeField] protected TagCollection _opponentTags = null;
        [SerializeField] protected TagCollection _projectileHitTag = null;

        public TagCollection OpponentTags { get => _opponentTags; }
        public TagCollection ProjectileHitTags { get => _projectileHitTag; }
    }
}
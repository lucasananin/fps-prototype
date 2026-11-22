using lutilities.TagCollections;
using UnityEngine;

namespace FPS
{
    public abstract class AbstractEntitySO : ScriptableObject
    {
        [SerializeField] protected TagCollection _damageableTags = null;
        [SerializeField] protected TagCollection _opponentTags = null;

        public TagCollection DamageableTags { get => _damageableTags; }
        public TagCollection OpponentTags { get => _opponentTags; }
    }
}
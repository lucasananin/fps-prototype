using UnityEngine;

namespace FPS
{
    [CreateAssetMenu(fileName = "Ammo_", menuName = "Scriptable Objects/Combat/Ammo")]
    public class AmmoSO : ScriptableObject
    {
        [SerializeField] string _id = null;
        [SerializeField] string _displayName = null;
        [SerializeField] int _maxAmount = 123;

        public string Id { get => _id; }
        public string DisplayName { get => _displayName; }
        public int MaxAmount { get => _maxAmount; }
    }
}
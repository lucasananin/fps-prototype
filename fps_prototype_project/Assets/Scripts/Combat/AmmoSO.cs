using UnityEngine;

namespace FPS
{
    [CreateAssetMenu(fileName = "Ammo_", menuName = "SO/Combat/Ammo Type")]
    public class AmmoSO : ScriptableObject
    {
        [SerializeField] string _id = null;
        //[SerializeField] string _displayName = null;
        [SerializeField] int _maxQuantity = 120;

        public string Id { get => _id; private set => _id = value; }
        public int MaxQuantity { get => _maxQuantity; private set => _maxQuantity = value; }
    }
}
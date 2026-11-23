using lutilities;
using UnityEngine;

namespace FPS
{
    [System.Serializable]
    public class AmmoModel
    {
        [SerializeField] AmmoSO _so = null;
        [SerializeField] int _amount = 0;

        public AmmoSO So { get => _so; }
        public int Amount { get => _amount; }

        public AmmoModel(AmmoSO so, int amount)
        {
            _so = so;
            _amount = amount;
        }

        public void DecreaseQuantity(int _value)
        {
            _amount -= _value;

            if (_amount < 0)
                _amount = 0;
        }

        public void RestoreAllAmount()
        {
            _amount = _so.MaxAmount;
        }

        public void RestoreAmount(int _percentage)
        {
            var _value = _so.MaxAmount * (_percentage / 100f);
            _amount += Mathf.RoundToInt(_value);
            _amount = Mathf.Clamp(_amount, 0, _so.MaxAmount);
        }

        public bool HasEnoughAmount(int _value)
        {
            return _amount >= _value;
        }

        public bool IsFull()
        {
            return _amount >= _so.MaxAmount;
        }

        public string GetId()
        {
            return _so.Id;
        }

        public float GetNormalizedValue()
        {
            return _amount / (_so.MaxAmount * 1f);
        }

        internal bool Equals(AmmoSO ammoType)
        {
            return StringMethods.AreStringsEqual(_so.Id, ammoType.Id);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FPS
{
    public class AmmoHandler : MonoBehaviour
    {
        [SerializeField] bool _infiniteAmmo = false;
        [SerializeField, Range(0f, 10f)] int _initialAmmoPercentage = 10;
        [SerializeField] AmmoSO[] _soList = null;

        [Header("// Debug")]
        [SerializeField] List<AmmoModel> _models = null;

        public event UnityAction OnAmmoChanged = null;

        public List<AmmoModel> Models { get => _models; }

        private void Awake()
        {
            CreateModels();
            RestoreAllAmmoModels(_initialAmmoPercentage * 10);
        }

        public void CreateModels()
        {
            int _count = _soList.Length;

            for (int i = 0; i < _count; i++)
            {
                var _newModel = new AmmoModel(_soList[i], 0);
                _models.Add(_newModel);
            }
        }

        public void RestoreAllAmmoModelsToMax()
        {
            RestoreAllAmmoModels(999);
        }

        public void RestoreAllAmmoModels(int _percentage)
        {
            int _count = _models.Count;

            for (int i = 0; i < _count; i++)
            {
                RestoreAmmoModel(_models[i].So, _percentage);
            }
        }

        public void RestoreAmmoModel(AmmoSO _ammoType, int _percentage)
        {
            int _count = _models.Count;

            for (int i = 0; i < _count; i++)
            {
                if (_models[i].Equals(_ammoType))
                {
                    _models[i].RestoreAmount(_percentage);
                }
            }

            OnAmmoChanged?.Invoke();
        }

        //public void RestoreRandomAmmoModel(int _percentage)
        //{
        //    var _weaponHandler = GetComponent<PlayerWeaponHandler>();

        //    var _ammoTypes = _weaponHandler.GetAmmoTypes();
        //    var _filteredList = FilterAmmoTypes(_ammoTypes);
        //    var _randomIndex = Random.Range(0, _filteredList.Count);

        //    var _ammoSO = _filteredList[_randomIndex];
        //    GetModel(_ammoSO).RestoreAmount(_percentage);
        //    OnAmmoChanged?.Invoke();
        //}

        //private List<AmmoSO> FilterAmmoTypes(List<AmmoSO> _list)
        //{
        //    var _newList = new List<AmmoSO>(_list);
        //    int _count = _newList.Count;

        //    for (int i = _count - 1; i >= 0; i--)
        //    {
        //        var _so = _newList[i];

        //        if (GetModel(_so).IsFull())
        //        {
        //            _newList.RemoveAt(i);
        //        }
        //    }

        //    return _newList.Count > 0 ? _newList : _list;
        //}

        public void DecreaseAmmo(WeaponSO _weaponSO)
        {
            if (_infiniteAmmo) return;

            var _model = GetModel(_weaponSO.AmmoSO);
            _model?.DecreaseQuantity(_weaponSO.Stats.AmmoPerShot);
            OnAmmoChanged?.Invoke();
        }

        public bool HasAmmo(WeaponSO _weaponSO)
        {
            var _model = GetModel(_weaponSO.AmmoSO);
            return _model is not null && _model.HasEnoughAmount(_weaponSO.Stats.AmmoPerShot);
        }

        public int GetAmmoAmount(AmmoSO _ammoSO)
        {
            var _model = GetModel(_ammoSO);
            return _model is not null ? _model.Amount : -1;
        }

        private AmmoModel GetModel(AmmoSO _ammoSO)
        {
            int _count = _models.Count;

            for (int i = 0; i < _count; i++)
            {
                if (_models[i].Equals(_ammoSO))
                {
                    return _models[i];
                }
            }

            return null;
        }
    }
}
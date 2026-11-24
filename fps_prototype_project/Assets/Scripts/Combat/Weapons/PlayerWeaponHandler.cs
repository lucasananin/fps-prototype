using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FPS
{
    public class PlayerWeaponHandler : AbstractWeaponHandler
    {
        [SerializeField] AbstractEntity _entitySource = null;
        [SerializeField] AmmoHandler _ammoHandler = null;
        [SerializeField] PlayerInputReader _input = null;
        [SerializeField, Range(1, 9)] int _maxWeaponAmount = 2;
        [SerializeField, Range(0f, 1f)] float _changeWeaponInputDelay = 0.3f;

        [SerializeField] private float _maxAimDistance = 100f;
        [SerializeField] private LayerMask _aimMask = default;

        [Header("// Debug")]
        [SerializeField] List<WeaponBehaviour> _weaponsList = null;

        private Camera _camera = null;
        private WeaponBehaviour _currentWeapon = null;
        private int _currentWeaponIndex = 0;
        private bool _isWaitingChangeWeaponDelay = false;

        public event UnityAction OnWeaponAdded = null;
        public event UnityAction OnWeaponSwapped = null;

        private void Awake()
        {
            if (_entitySource == null)
                _entitySource = GetComponent<PlayerEntity>();

            _camera = Camera.main;

            SetCurrentWeapon();
        }

        private void Update()
        {
            if (_input.ShootAction.WasPerformedThisFrame())
            {
                PullTrigger();
            }

            if (_input.ShootAction.WasReleasedThisFrame())
            {
                ReleaseTrigger();
            }

            if (_input.ReloadAction.WasPerformedThisFrame())
            {
                _currentWeapon.TryReloadMagazine();
            }
        }

        public void PullTrigger()
        {
            if (_currentWeapon != null)
                _currentWeapon.PullTrigger();
        }

        public void ReleaseTrigger()
        {
            if (_currentWeapon != null)
                _currentWeapon.ReleaseTrigger();
        }

        public void ChangeWeaponUsingMouseScroll(float _y)
        {
            if (_isWaitingChangeWeaponDelay) return;
            if (_weaponsList.Count < 2) return;

            if (_y > 0)
                ChangeToNextWeapon();
            else if (_y < 0)
                ChangeToPreviousWeapon();

            OnWeaponSwapped?.Invoke();
            StartCoroutine(DisableWeaponChange_Routine());
        }

        private void ChangeToNextWeapon()
        {
            IncreaseWeaponIndex(1);
        }

        private void ChangeToPreviousWeapon()
        {
            IncreaseWeaponIndex(-1);
        }

        private void IncreaseWeaponIndex(int _value)
        {
            _currentWeaponIndex = _weaponsList.IndexOf(_currentWeapon);
            _currentWeaponIndex += _value;

            if (_currentWeaponIndex >= _weaponsList.Count)
                _currentWeaponIndex = 0;

            if (_currentWeaponIndex < 0)
                _currentWeaponIndex = _weaponsList.Count - 1;

            SetCurrentWeapon();
        }

        private void SetCurrentWeapon()
        {
            if (_weaponsList.Count <= 0) return;

            var _initInfo = new WeaponInitInfo(_entitySource, _ammoHandler, _camera, _aimMask, _maxAimDistance);
            _currentWeapon = _weaponsList[_currentWeaponIndex];
            _currentWeapon.Init(_initInfo);

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            int _count = _weaponsList.Count;

            for (int i = 0; i < _count; i++)
            {
                bool _isCurrentWeapon = i == _currentWeaponIndex;
                _weaponsList[i].gameObject.SetActive(_isCurrentWeapon);
                _weaponsList[i].ResetRuntimeValues();
            }
        }

        public void AddWeapon(WeaponSO _weaponSO, out WeaponSO _droppedWeaponSO)
        {
            var _newWeapon = Instantiate(_weaponSO.Prefab, transform.position, Quaternion.identity, transform);

            if (_weaponsList.Count >= _maxWeaponAmount)
            {
                _weaponsList[_currentWeaponIndex] = _newWeapon;
                _droppedWeaponSO = _currentWeapon.SO;
                Destroy(_currentWeapon.gameObject);
                SetCurrentWeapon();
            }
            else
            {
                _weaponsList.Add(_newWeapon);
                _droppedWeaponSO = null;
                ChangeToNextWeapon();
            }

            OnWeaponAdded?.Invoke();
        }

        public bool HasWeapon(WeaponSO _weaponSO)
        {
            int _count = _weaponsList.Count;

            for (int i = 0; i < _count; i++)
            {
                if (_weaponsList[i].GetId() == _weaponSO.Id)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasMoreThanOneWeapon()
        {
            return _weaponsList.Count > 1;
        }

        public WeaponBehaviour GetCurrentWeapon()
        {
            return _currentWeapon;
        }

        public WeaponBehaviour GetFirstWeapon()
        {
            if (_currentWeapon == null) return null;
            return _weaponsList[0];
        }

        public WeaponBehaviour GetSecondWeapon()
        {
            if (!HasMoreThanOneWeapon()) return null;
            return _weaponsList[1];
        }

        private IEnumerator DisableWeaponChange_Routine()
        {
            _isWaitingChangeWeaponDelay = true;
            yield return new WaitForSeconds(_changeWeaponInputDelay);
            _isWaitingChangeWeaponDelay = false;
        }

        public List<AmmoSO> GetAmmoTypes()
        {
            var _list = new List<AmmoSO>();
            var _count = _weaponsList.Count;

            for (int i = 0; i < _count; i++)
            {
                var _ammoSO = _weaponsList[i].SO.AmmoSO;
                _list.Add(_ammoSO);
            }

            return _list;
        }
    }
}

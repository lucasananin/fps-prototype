using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class PlayerWeaponHandler : WeaponHandler
    {
        [SerializeField] AbstractEntity _entitySource = null;
        [SerializeField] AmmoHandler _ammoHandler = null;
        [SerializeField] PlayerInputReader _input = null;
        [SerializeField, Range(1, 9)] int _maxWeaponsCount = 2;
        [SerializeField, Range(0f, 1f)] float _swapInputDelay = 0.3f;
        [SerializeField] List<WeaponBehaviour> _weaponsList = null;

        [Header("// Debug")]
        [SerializeField] WeaponBehaviour _currentWeapon = null;
        [SerializeField] int _currentWeaponIndex = 0;
        [SerializeField] bool _isWaitingSwapDelay = false;

        public event System.Action OnWeaponAdded = null;
        public event System.Action OnWeaponSwapped = null;

        private void Awake()
        {
            if (_entitySource == null)
                _entitySource = GetComponent<PlayerEntity>();

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

        public void SwapThroughInput(float _y)
        {
            if (_isWaitingSwapDelay) return;
            if (_weaponsList.Count < 2) return;

            if (_y > 0)
                SwapToNextWeapon();
            else if (_y < 0)
                SwapToPreviousWeapon();

            OnWeaponSwapped?.Invoke();
            StartCoroutine(DisableWeaponSwap_routine());
        }

        private void SwapToNextWeapon()
        {
            IncreaseWeaponIndex(1);
        }

        private void SwapToPreviousWeapon()
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

            _currentWeapon = _weaponsList[_currentWeaponIndex];
            _currentWeapon.Init(_entitySource, _ammoHandler);

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
            var _newWeapon = Instantiate(_weaponSO.WeaponPrefab, transform.position, Quaternion.identity, transform);

            if (_weaponsList.Count >= _maxWeaponsCount)
            {
                _weaponsList[_currentWeaponIndex] = _newWeapon;
                _droppedWeaponSO = _currentWeapon.WeaponSO;
                Destroy(_currentWeapon.gameObject);
                SetCurrentWeapon();
            }
            else
            {
                _weaponsList.Add(_newWeapon);
                _droppedWeaponSO = null;
                SwapToNextWeapon();
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

        private IEnumerator DisableWeaponSwap_routine()
        {
            _isWaitingSwapDelay = true;
            yield return new WaitForSeconds(_swapInputDelay);
            _isWaitingSwapDelay = false;
        }

        public List<AmmoSO> GetAmmoTypes()
        {
            var _list = new List<AmmoSO>();
            var _count = _weaponsList.Count;

            for (int i = 0; i < _count; i++)
            {
                var _ammoSO = _weaponsList[i].WeaponSO.ProjectileSO.AmmoSO;
                _list.Add(_ammoSO);
            }

            return _list;
        }
    }
}

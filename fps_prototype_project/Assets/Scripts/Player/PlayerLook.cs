using UnityEngine;

namespace FPS
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private AimFriction _aimFriction;
        [SerializeField] private AimStickiness _aimStickiness;

        [Header("Look Settings")]
        [SerializeField] private float _mouseSensitivity = 0.1f;
        [SerializeField] private float _gamepadSensitivity = 100f;
        [SerializeField] private float _minPitch = -90f;
        [SerializeField] private float _maxPitch = 90f;

        [Header("Recoil Settings")]
        [SerializeField] private float _recoilRecoverySpeed = 8f;
        [SerializeField] private float _maxRecoilPitch = 15f;
        [SerializeField] private float _maxRecoilYaw = 5f;

        private float _yaw;
        private float _pitch;
        private float _recoilPitch;
        private float _recoilYaw;

        private void Start()
        {
            _yaw = transform.eulerAngles.y;
            _pitch = _cameraPivot.localEulerAngles.x;

            if (_pitch > 180f)
                _pitch -= 360f;
        }

        private void Update()
        {
            HandleLook();
        }

        private void LateUpdate()
        {
            RecoverRecoil();
            transform.rotation = Quaternion.Euler(0f, _yaw + _recoilYaw, 0f);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch - _recoilPitch, 0f, 0f);
        }

        private void HandleLook()
        {
            Vector2 lookInput = _inputReader.LookInput;
            bool isMouse = _inputReader.IsLookInputFromMouse;

            float deltaTime = isMouse ? 1f : Time.deltaTime;

            var sensitivity = isMouse ? _mouseSensitivity : _gamepadSensitivity;
            sensitivity *= GetFrictionMultiplier(isMouse);

            float horizontalInput = lookInput.x * sensitivity * deltaTime;
            float verticalInput = lookInput.y * sensitivity * deltaTime;

            _yaw += horizontalInput;
            _pitch -= verticalInput;

            Vector2 stickyDelta = GetStickyDelta(isMouse);
            _yaw += stickyDelta.x;
            _pitch += stickyDelta.y;

            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        }

        // Aim friction only applies to gamepad input.
        private float GetFrictionMultiplier(bool isMouse)
        {
            if (_aimFriction == null)
                return 1f;

            if (isMouse)
            {
                _aimFriction.ResetFriction();
                Debug.Log($"// _friction");
                return 1f;
            }

            Debug.Log($"// _friction");
            return _aimFriction.GetFriction();
        }

        // Aim stickiness only applies to gamepad input.
        private Vector2 GetStickyDelta(bool isMouse)
        {
            if (_aimStickiness == null)
                return Vector2.zero;

            if (isMouse)
            {
                _aimStickiness.ResetStickiness();
                Debug.Log($"// _stickiness");
                return Vector2.zero;
            }

            Debug.Log($"// _stickiness");
            return _aimStickiness.GetStickyDelta();
        }

        public void ApplyRecoil(float _pitchAmount, float _yawAmount)
        {
            _recoilPitch = Mathf.Clamp(_recoilPitch + _pitchAmount, 0f, _maxRecoilPitch);
            _recoilYaw = Mathf.Clamp(_recoilYaw + _yawAmount, -_maxRecoilYaw, _maxRecoilYaw);
        }

        private void RecoverRecoil()
        {
            _recoilPitch = Mathf.Lerp(_recoilPitch, 0f, _recoilRecoverySpeed * Time.deltaTime);
            _recoilYaw = Mathf.Lerp(_recoilYaw, 0f, _recoilRecoverySpeed * Time.deltaTime);
        }
    }
}

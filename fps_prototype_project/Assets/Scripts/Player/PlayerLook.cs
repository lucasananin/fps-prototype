using UnityEngine;

namespace FPS
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private Transform _cameraPivot;

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
            //transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            //_cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            RecoverRecoil();
            transform.rotation = Quaternion.Euler(0f, _yaw + _recoilYaw, 0f);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch - _recoilPitch, 0f, 0f);
        }

        private void HandleLook()
        {
            Vector2 lookInput = _inputReader.LookInput;

            float deltaTime = _inputReader.IsLookInputFromMouse ? 1f : Time.deltaTime;

            var sensitivity = _inputReader.IsLookInputFromMouse ? _mouseSensitivity : _gamepadSensitivity;
            float horizontalInput = lookInput.x * sensitivity * deltaTime;
            float verticalInput = lookInput.y * sensitivity * deltaTime;

            _yaw += horizontalInput;
            _pitch -= verticalInput;

            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
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

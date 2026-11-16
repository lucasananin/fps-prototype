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

        private Rigidbody _rigidbody;
        private float _yaw;
        private float _pitch;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

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
            _rigidbody.MoveRotation(Quaternion.Euler(0f, _yaw, 0f));
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
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

            _pitch = Mathf.Clamp(
                _pitch,
                _minPitch,
                _maxPitch
            );
        }
    }
}

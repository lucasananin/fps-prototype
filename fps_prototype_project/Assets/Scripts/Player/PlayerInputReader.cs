using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _jumpAction;

        [Header("Camera")]
        [SerializeField] private InputActionReference _lookAction;

        private bool _jumpPressed;

        public Vector2 Move { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsLookInputFromMouse { get; private set; }

        private void OnEnable()
        {
            _moveAction.action.Enable();
            _lookAction.action.Enable();
            _jumpAction.action.Enable();

            _jumpAction.action.performed += OnJump;
        }

        private void OnDisable()
        {
            _moveAction.action.Disable();
            _lookAction.action.Disable();
            _jumpAction.action.Disable();

            _jumpAction.action.performed -= OnJump;

            Move = Vector2.zero;
            LookInput = Vector2.zero;
            _jumpPressed = false;
        }

        private void Update()
        {
            InputAction action = _lookAction.action;
            LookInput = _lookAction.action.ReadValue<Vector2>();
            IsLookInputFromMouse = action.activeControl?.device is Mouse;
            Move = _moveAction.action.ReadValue<Vector2>();
        }

        public bool ConsumeJumpPressed()
        {
            bool pressed = _jumpPressed;
            _jumpPressed = false;

            return pressed;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            _jumpPressed = true;
        }
    }
}

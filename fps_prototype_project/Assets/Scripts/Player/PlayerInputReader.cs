using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _lookAction;
        [SerializeField] private InputActionReference _jumpAction;
        [SerializeField] private InputActionReference _dashAction;

        private bool _jumpPressed;
        private bool _dashPressed;

        public Vector2 Move { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsLookInputFromMouse { get; private set; }
        public bool JumpHeld { get; private set; }

        private void OnEnable()
        {
            _moveAction.action.Enable();
            _lookAction.action.Enable();
            _jumpAction.action.Enable();
            _dashAction.action.Enable();

            _jumpAction.action.performed += OnJumpPressed;
            _jumpAction.action.canceled += OnJumpReleased;
            _dashAction.action.performed += OnDashPressed;
        }

        private void OnDisable()
        {
            _moveAction.action.Disable();
            _lookAction.action.Disable();
            _jumpAction.action.Disable();
            _dashAction.action.Disable();

            _jumpAction.action.performed -= OnJumpPressed;
            _jumpAction.action.canceled -= OnJumpReleased;
            _dashAction.action.performed -= OnDashPressed;

            Move = Vector2.zero;
            LookInput = Vector2.zero;
            _jumpPressed = false;
            _jumpPressed = false;
            _dashPressed = false;
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

        private void OnJumpPressed(InputAction.CallbackContext context)
        {
            _jumpPressed = true;
            JumpHeld = true;
        }

        private void OnJumpReleased(InputAction.CallbackContext context)
        {
            JumpHeld = false;
        }

        public bool ConsumeDashPressed()
        {
            bool pressed = _dashPressed;
            _dashPressed = false;

            return pressed;
        }

        private void OnDashPressed(InputAction.CallbackContext context)
        {
            _dashPressed = true;
        }
    }
}

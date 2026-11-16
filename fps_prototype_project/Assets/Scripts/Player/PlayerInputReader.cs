using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _lookAction;

        public Vector2 Move { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsLookInputFromMouse { get; private set; }

        private void OnEnable()
        {
            _moveAction.action.Enable();
            _lookAction.action.Enable();
        }

        private void OnDisable()
        {
            _moveAction.action.Disable();
            _lookAction.action.Disable();
            LookInput = Vector2.zero;
        }

        private void Update()
        {
            InputAction action = _lookAction.action;
            LookInput = _lookAction.action.ReadValue<Vector2>();
            IsLookInputFromMouse = action.activeControl?.device is Mouse;
            Move = _moveAction.action.ReadValue<Vector2>();
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference _lookAction;

        public Vector2 LookInput { get; private set; }
        public bool IsLookInputFromMouse { get; private set; }

        private void OnEnable()
        {
            if (_lookAction == null)
                return;

            _lookAction.action.Enable();
        }

        private void OnDisable()
        {
            if (_lookAction == null)
                return;

            _lookAction.action.Disable();
            LookInput = Vector2.zero;
        }

        private void Update()
        {
            if (_lookAction == null)
                return;

            InputAction action = _lookAction.action;

            LookInput = action.ReadValue<Vector2>();

            IsLookInputFromMouse = action.activeControl?.device is Mouse;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS
{
    public enum ControlScheme
    {
        KeyboardMouse,
        Gamepad
    }

    /// <summary>
    /// Holds the control scheme the player used most recently.
    /// It listens to every input action, so the scheme updates when ANY action is used
    /// (look, move, fire, jump...) and stays the same while nothing is being pressed or moved.
    /// </summary>
    public class ControlSchemeTracker : MonoBehaviour
    {
        [Tooltip("Scheme used until the player touches an input device.")]
        [SerializeField] private ControlScheme _defaultScheme = ControlScheme.KeyboardMouse;

        public ControlScheme CurrentScheme { get; private set; }

        public bool IsGamepad => CurrentScheme == ControlScheme.Gamepad;
        public bool IsKeyboardMouse => CurrentScheme == ControlScheme.KeyboardMouse;

        /// <summary>Raised only when the scheme actually changes.</summary>
        public event Action<ControlScheme> SchemeChanged;

        private void Awake()
        {
            CurrentScheme = _defaultScheme;
        }

        private void OnEnable()
        {
            InputSystem.onActionChange += OnActionChange;
        }

        private void OnDisable()
        {
            InputSystem.onActionChange -= OnActionChange;
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed)
                return;

            if (obj is not InputAction action)
                return;

            InputDevice device = action.activeControl?.device;
            if (device == null)
                return;

            if (device is Gamepad)
                SetScheme(ControlScheme.Gamepad);
            else if (device is Mouse || device is Keyboard)
                SetScheme(ControlScheme.KeyboardMouse);
            // Other devices (touch, joysticks, etc.) are ignored.
        }

        private void SetScheme(ControlScheme newScheme)
        {
            if (newScheme == CurrentScheme)
                return;

            CurrentScheme = newScheme;
            SchemeChanged?.Invoke(newScheme);
        }
    }
}

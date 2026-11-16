using UnityEngine;
using UnityEditor;

namespace FPS
{
    [InitializeOnLoad]
    public static class UnlockCursorOnEditorPause
    {
        static UnlockCursorOnEditorPause()
        {
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
        }

        private static void OnPauseStateChanged(PauseState state)
        {
            if (state == PauseState.Paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
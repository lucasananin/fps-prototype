using UnityEngine;

namespace FPS
{
    public class CursorLocker : MonoBehaviour
    {
        private void OnApplicationFocus(bool focus)
        {
            Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}

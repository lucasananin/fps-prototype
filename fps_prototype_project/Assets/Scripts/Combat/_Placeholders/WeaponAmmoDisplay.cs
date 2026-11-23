using TMPro;
using UnityEngine;

namespace FPS
{
    public class WeaponAmmoDisplay : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _text = null;

        private PlayerWeaponHandler _handler = null;

        private void Awake()
        {
            _handler = FindFirstObjectByType<PlayerWeaponHandler>();
        }

        private void LateUpdate()
        {
            _text.text = $"III {_handler.GetCurrentWeapon().GetAmmoString()}";
        }
    }
}

using System.Collections;
using UnityEngine;

namespace FPS
{
    public class WeaponShootListener : MonoBehaviour
    {
        [SerializeField] WeaponBehaviour _weapon = null;
        [SerializeField] GameObject _content = null;

        private void Awake()
        {
            _content.SetActive(false);
        }

        private void OnEnable()
        {
            _weapon.OnShoot += Weapon_OnShoot;
        }

        private void OnDisable()
        {
            _weapon.OnShoot -= Weapon_OnShoot;
        }

        private void Weapon_OnShoot()
        {
            StopAllCoroutines();
            StartCoroutine(Routine());
        }

        IEnumerator Routine()
        {
            _content.SetActive(true);
            yield return new WaitForSeconds(0.03f);
            _content.SetActive(false);
        }
    }
}

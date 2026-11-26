using System.Collections;
using UnityEngine;

namespace FPS
{
    public class SimpleExplosionVfx : MonoBehaviour
    {
        [SerializeField] MaterialFader _fader = null;
        [SerializeField] float _duration = 1f;

        [ContextMenu("Test()")]
        public void Test()
        {
            Init(5);
        }

        public void Init(float _radius)
        {
            _fader.MeshRenderer.transform.localScale = _radius * 2f * Vector3.one;
            _fader.FadeAlpha(0, _duration);
            StartCoroutine(DestroyRoutine());
        }

        private IEnumerator DestroyRoutine()
        {
            yield return new WaitForSeconds(_duration + Random.Range(1f, 2f));

            Destroy(gameObject);
        }
    }
}
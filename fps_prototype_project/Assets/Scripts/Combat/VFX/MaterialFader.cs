using UnityEngine;

namespace FPS
{
    using System.Collections;
    using UnityEngine;

    public class MaterialFader : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        // URP/HDRP use _BaseColor; Built-in uses _Color
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock _block;
        private Coroutine _fadeRoutine;
        private Color _currentColor;

        public MeshRenderer MeshRenderer { get => meshRenderer; }

        private void Awake()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();

            _block = new MaterialPropertyBlock();

            // Read the starting color from the shared material (no instance created)
            _currentColor = ReadColorFromSharedMaterial();
        }

        /// <summary>
        /// Fades the color to a target color over time.
        /// </summary>
        public void FadeToColor(Color targetColor, float duration)
        {
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(FadeRoutine(targetColor, duration));
        }

        /// <summary>
        /// Fades only the alpha to a target value.
        /// </summary>
        public void FadeAlpha(float targetAlpha, float duration)
        {
            Color target = new Color(_currentColor.r, _currentColor.g, _currentColor.b, targetAlpha);
            FadeToColor(target, duration);
        }

        /// <summary>
        /// Removes the override and returns to the material's original color.
        /// </summary>
        public void ResetColor()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            meshRenderer.SetPropertyBlock(null);
            _currentColor = ReadColorFromSharedMaterial();
        }

        private IEnumerator FadeRoutine(Color targetColor, float duration)
        {
            Color startColor = _currentColor;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                ApplyColor(Color.Lerp(startColor, targetColor, t));
                yield return null;
            }

            ApplyColor(targetColor);
            _fadeRoutine = null;
        }

        private void ApplyColor(Color color)
        {
            _currentColor = color;

            meshRenderer.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, color);
            _block.SetColor(ColorId, color);
            meshRenderer.SetPropertyBlock(_block);
        }

        private Color ReadColorFromSharedMaterial()
        {
            Material mat = meshRenderer.sharedMaterial;
            if (mat == null) return Color.white;

            if (mat.HasProperty(BaseColorId)) return mat.GetColor(BaseColorId);
            if (mat.HasProperty(ColorId)) return mat.GetColor(ColorId);
            return Color.white;
        }
    }
}

using lutilities.TagCollections;
using UnityEngine;

namespace FPS
{
    public class AimFriction : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform the cast starts from, usually the camera pivot.")]
        [SerializeField] private Transform _aimOrigin;

        [Header("Detection")]
        [Tooltip("Layers of the things that should slow the aim down (enemies).")]
        [SerializeField] private LayerMask _targetLayers;
        [Tooltip("Layers that can block the view of a target (walls, props). Do NOT include the player's own layer.")]
        [SerializeField] private LayerMask _obstructionLayers;
        [SerializeField] TagCollection _targetTags = null;
        [Tooltip("Max distance at which friction can apply.")]
        [SerializeField] private float _range = 40f;
        [Tooltip("Width of the detection cast. Bigger = friction starts when the crosshair is farther from the target.")]
        [SerializeField] private float _radius = 0.6f;

        [Header("Friction")]
        [Tooltip("Sensitivity multiplier at point blank. 1 = no friction, 0.5 = half sensitivity.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float _minMultiplier = 0.5f;
        [Tooltip("How fast the multiplier blends in/out, to avoid sudden sensitivity jumps.")]
        [SerializeField] private float _blendSpeed = 10f;

        private float _currentMultiplier = 1f;

        /// <summary>
        /// Returns the sensitivity multiplier (between min and 1) for this frame.
        /// Call once per frame while friction should be active.
        /// </summary>
        public float GetFriction()
        {
            float target = CalculateTargetMultiplier();
            _currentMultiplier = Mathf.MoveTowards(_currentMultiplier, target, _blendSpeed * Time.deltaTime);
            return _currentMultiplier;
        }

        /// <summary>
        /// Snaps the multiplier back to 1 (no friction). Call when friction is not in use.
        /// </summary>
        public void ResetFriction()
        {
            _currentMultiplier = 1f;
        }

        private float CalculateTargetMultiplier()
        {
            LayerMask castMask = _targetLayers | _obstructionLayers;

            if (Physics.SphereCast(
                    _aimOrigin.position,
                    _radius,
                    _aimOrigin.forward,
                    out RaycastHit hit,
                    _range,
                    castMask,
                    QueryTriggerInteraction.Ignore))
            {
                // Only apply friction if the first thing hit is a target (not a wall in front of it).
                bool hitIsTarget = (_targetLayers.value & (1 << hit.collider.gameObject.layer)) != 0;

                if (hitIsTarget && _targetTags.CompareTag(hit.collider))
                {
                    // Strongest friction up close, fading out toward max range.
                    float distanceRatio = hit.distance / _range;
                    return Mathf.Lerp(_minMultiplier, 1f, distanceRatio);
                }
            }

            return 1f;
        }
    }
}

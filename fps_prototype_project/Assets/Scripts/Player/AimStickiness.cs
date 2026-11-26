using lutilities.TagCollections;
using UnityEngine;

namespace FPS
{
    public class AimStickiness : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform the cast starts from, usually the camera pivot.")]
        [SerializeField] private Transform _aimOrigin;

        [Header("Detection")]
        [Tooltip("Layers of the things the aim can stick to (enemies).")]
        [SerializeField] private LayerMask _targetLayers;
        [Tooltip("Layers that can block the view of a target (walls, props). Do NOT include the player's own layer.")]
        [SerializeField] private LayerMask _obstructionLayers;
        [SerializeField] TagCollection _targetTags = null;
        [Tooltip("Max distance at which stickiness can apply.")]
        [SerializeField] private float _range = 30f;
        [Tooltip("Width of the detection cast. Bigger = the crosshair can be farther from the target and still stick.")]
        [SerializeField] private float _radius = 0.4f;

        [Header("Stickiness")]
        [Tooltip("How much of the target's movement the camera follows. 0 = none, 1 = perfect tracking.")]
        [Range(0f, 1f)]
        [SerializeField] private float _strength = 0.6f;
        [Tooltip("If the target moves more than this many degrees in one frame (teleport, respawn), ignore it.")]
        [SerializeField] private float _maxDeltaPerFrame = 5f;

        private Collider _currentTarget;
        private Vector3 _lastTargetPoint;

        /// <summary>
        /// Returns the extra rotation (in degrees) to follow the target this frame.
        /// x = yaw, y = pitch. Both use the same convention as PlayerLook's _yaw and _pitch fields,
        /// so they can be added to them directly.
        /// Call once per frame while stickiness should be active.
        /// </summary>
        public Vector2 GetStickyDelta()
        {
            if (!TryGetTarget(out Collider target, out float distance))
            {
                ResetStickiness();
                return Vector2.zero;
            }

            Vector3 targetPoint = target.bounds.center;
            Vector2 delta = Vector2.zero;

            // Only follow if we were already tracking this same target last frame.
            if (target == _currentTarget)
            {
                delta = CalculateAngularDelta(_lastTargetPoint, targetPoint);

                if (delta.magnitude > _maxDeltaPerFrame)
                {
                    delta = Vector2.zero;
                }
                else
                {
                    // Weaker stickiness the farther the target is.
                    float distanceFalloff = 1f - Mathf.Clamp01(distance / _range);
                    delta *= _strength * distanceFalloff;
                }
            }

            _currentTarget = target;
            _lastTargetPoint = targetPoint;

            return delta;
        }

        /// <summary>
        /// Forgets the current target. Call when stickiness is not in use.
        /// </summary>
        public void ResetStickiness()
        {
            _currentTarget = null;
        }

        private bool TryGetTarget(out Collider target, out float distance)
        {
            target = null;
            distance = 0f;

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
                // Only a target if the first thing hit is on a target layer (not a wall in front of it).
                bool hitIsTarget = (_targetLayers.value & (1 << hit.collider.gameObject.layer)) != 0;

                if (hitIsTarget && _targetTags.CompareTag(hit.collider))
                {
                    target = hit.collider;
                    distance = hit.distance;
                    return true;
                }
            }

            return false;
        }

        // Measures how much the target moved around the camera between two frames.
        // Both points are measured from the CURRENT camera position, so only the
        // target's own movement is counted (not the player's movement or rotation).
        private Vector2 CalculateAngularDelta(Vector3 previousPoint, Vector3 currentPoint)
        {
            Vector3 origin = _aimOrigin.position;
            Vector3 from = previousPoint - origin;
            Vector3 to = currentPoint - origin;

            if (from.sqrMagnitude < 0.0001f || to.sqrMagnitude < 0.0001f)
                return Vector2.zero;

            // Yaw: positive = target moved to the right.
            float yawDelta = Vector3.SignedAngle(
                Vector3.ProjectOnPlane(from, Vector3.up),
                Vector3.ProjectOnPlane(to, Vector3.up),
                Vector3.up);

            // Pitch: PlayerLook's _pitch decreases when looking up, so the sign is flipped.
            float elevationFrom = Mathf.Asin(Mathf.Clamp(from.normalized.y, -1f, 1f)) * Mathf.Rad2Deg;
            float elevationTo = Mathf.Asin(Mathf.Clamp(to.normalized.y, -1f, 1f)) * Mathf.Rad2Deg;
            float pitchDelta = -(elevationTo - elevationFrom);

            return new Vector2(yawDelta, pitchDelta);
        }
    }
}

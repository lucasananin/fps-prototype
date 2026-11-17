using UnityEngine;

namespace FPS
{
    public class PlayerCrouchCamera : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The object to move. If empty, this GameObject's transform is used.")]
        [SerializeField] private Transform referenceObject;

        [Header("Settings")]
        [Tooltip("When true, the object moves to 'Active Local Position'. When false, it returns to its default.")]
        [SerializeField] private bool isActive;

        [SerializeField] private Vector3 activeLocalPosition;

        [Tooltip("Approximate time (in seconds) it takes to reach the destination. Lower = faster.")]
        [SerializeField, Min(0.01f)] private float smoothTime = 0.3f;

        PlayerMovement _movement = null;
        private Vector3 defaultLocalPosition;
        private Vector3 velocity;

        /// <summary>
        /// Set this from other scripts, UI events, etc.
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        private void OnValidate()
        {
            _movement = GetComponent<PlayerMovement>();
        }

        private void Awake()
        {
            if (_movement == null) _movement = GetComponent<PlayerMovement>();

            defaultLocalPosition = referenceObject.localPosition;
        }

        private void Update()
        {
            Vector3 goal = _movement.IsSliding || _movement.IsCrouching ? activeLocalPosition : defaultLocalPosition;

            referenceObject.localPosition = Vector3.SmoothDamp(
                referenceObject.localPosition,
                goal,
                ref velocity,
                smoothTime
            );
        }
    }
}

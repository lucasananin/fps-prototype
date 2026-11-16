using UnityEngine;

namespace FPS
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader _playerInput;
        [SerializeField] private Transform _orientation;

        [Header("Ground Movement")]
        [SerializeField] private float _moveSpeed = 7f;
        [SerializeField] private float _groundAcceleration = 40f;
        [SerializeField] private float _groundDeceleration = 40f;

        [Header("Air Movement")]
        [SerializeField] private float _airAcceleration = 15f;
        [SerializeField] private float _airControl = 0.5f;

        [Header("Ground Detection")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayers;

        private Rigidbody _rigidbody;

        private Vector3 _moveDirection;
        private Vector3 _horizontalVelocity;

        private bool _isGrounded;

        public bool IsGrounded => _isGrounded;
        public Vector3 Velocity => _rigidbody.linearVelocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _rigidbody.freezeRotation = true;
        }

        private void Update()
        {
            CheckGround();
            CalculateMovement();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        private void CheckGround()
        {
            _isGrounded = Physics.CheckSphere(
                _groundCheck.position,
                _groundCheckRadius,
                _groundLayers,
                QueryTriggerInteraction.Ignore
            );
        }

        private void CalculateMovement()
        {
            Vector2 input = _playerInput.Move;

            Vector3 forward = _orientation.forward;
            Vector3 right = _orientation.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            _moveDirection = (forward * input.y + right * input.x).normalized;
        }

        private void ApplyMovement()
        {
            Vector3 velocity = _rigidbody.linearVelocity;

            Vector3 currentHorizontalVelocity = new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

            Vector3 targetVelocity = _moveDirection * _moveSpeed;

            float acceleration;

            if (_isGrounded)
            {
                acceleration = _moveDirection.sqrMagnitude > 0.01f
                    ? _groundAcceleration
                    : _groundDeceleration;
            }
            else
            {
                acceleration = _airAcceleration * _airControl;
            }

            Vector3 newHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );

            _rigidbody.linearVelocity = new Vector3(
                newHorizontalVelocity.x,
                velocity.y,
                newHorizontalVelocity.z
            );
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheck == null)
                return;

            Gizmos.color = _isGrounded ? Color.green : Color.red;

            Gizmos.DrawWireSphere(
                _groundCheck.position,
                _groundCheckRadius
            );
        }
    }
}

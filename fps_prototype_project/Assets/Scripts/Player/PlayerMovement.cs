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

        [Header("Jump")]
        [SerializeField] private int _maxJumps = 2;
        [SerializeField] private float _jumpForce = 7f;

        [Header("Ground Detection")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayers;

        private Rigidbody _rigidbody;

        private Vector3 _moveDirection;

        private bool _isGrounded;
        private bool _jumpRequested;

        private int _jumpsRemaining;

        public bool IsGrounded => _isGrounded;
        public int JumpsRemaining => _jumpsRemaining;
        public Vector3 Velocity => _rigidbody.linearVelocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _rigidbody.freezeRotation = true;

            _jumpsRemaining = _maxJumps;
        }

        private void Update()
        {
            CalculateMovement();

            if (_playerInput.ConsumeJumpPressed())
            {
                _jumpRequested = true;
            }
        }

        private void FixedUpdate()
        {
            CheckGround();

            if (_isGrounded)
            {
                _jumpsRemaining = _maxJumps;
            }

            HandleJump();
            ApplyMovement();
        }

        private void CheckGround()
        {
            bool touchingGround = Physics.CheckSphere(
                _groundCheck.position,
                _groundCheckRadius,
                _groundLayers,
                QueryTriggerInteraction.Ignore
            );

            // Prevent ground detection while moving upward.
            _isGrounded = touchingGround &&
                          _rigidbody.linearVelocity.y <= 0.01f;
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

            _moveDirection = (
                forward * input.y +
                right * input.x
            ).normalized;
        }

        private void HandleJump()
        {
            if (!_jumpRequested)
                return;

            _jumpRequested = false;

            if (_jumpsRemaining <= 0)
                return;

            _jumpsRemaining--;

            Vector3 velocity = _rigidbody.linearVelocity;

            velocity.y = _jumpForce;

            _rigidbody.linearVelocity = velocity;

            _isGrounded = false;
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


using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] private float _airSpeedLimit = 7f;

        [Header("Jump")]
        [SerializeField, Min(1)] private int _maxJumps = 2;
        [SerializeField] private float _jumpForce = 7f;
        [SerializeField] private float _coyoteTime = 0.12f;
        [SerializeField] private float _jumpBufferTime = 0.15f;

        [Header("Ground Detection")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayers;

        private Rigidbody _rigidbody;

        private Vector3 _moveDirection;

        private bool _isGrounded;
        private bool _hasJumpedSinceGround;

        private int _jumpsRemaining;

        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;

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
                _jumpBufferCounter = _jumpBufferTime;
            }
        }

        private void FixedUpdate()
        {
            CheckGround();

            if (_isGrounded)
            {
                _coyoteTimeCounter = _coyoteTime;
                _jumpsRemaining = _maxJumps;
                _hasJumpedSinceGround = false;
            }

            HandleJump();
            UpdateCoyoteTime();

            if (_isGrounded)
            {
                ApplyGroundMovement();
            }
            else
            {
                ApplyAirMovement();
            }

            UpdateJumpBuffer();
        }

        private void CheckGround()
        {
            bool touchingGround = Physics.CheckSphere(
                _groundCheck.position,
                _groundCheckRadius,
                _groundLayers,
                QueryTriggerInteraction.Ignore
            );

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
            if (_jumpBufferCounter <= 0f)
                return;

            if (_jumpsRemaining <= 0)
                return;

            bool canJump = _isGrounded ||
                           _coyoteTimeCounter > 0f ||
                           _hasJumpedSinceGround;

            if (!canJump)
                return;

            _jumpsRemaining--;

            Vector3 velocity = _rigidbody.linearVelocity;

            velocity.y = _jumpForce;

            _rigidbody.linearVelocity = velocity;

            _isGrounded = false;
            _hasJumpedSinceGround = true;

            _coyoteTimeCounter = 0f;
            _jumpBufferCounter = 0f;
        }

        private void UpdateCoyoteTime()
        {
            if (_isGrounded || _hasJumpedSinceGround)
                return;

            _coyoteTimeCounter -= Time.fixedDeltaTime;

            if (_coyoteTimeCounter <= 0f)
            {
                _coyoteTimeCounter = 0f;

                // The first jump opportunity expires.
                // Remaining jumps are preserved.
                _jumpsRemaining = Mathf.Max(
                    0,
                    _jumpsRemaining - 1
                );

                _hasJumpedSinceGround = true;
            }
        }

        private void UpdateJumpBuffer()
        {
            if (_jumpBufferCounter <= 0f)
                return;

            _jumpBufferCounter = Mathf.Max(
                0f,
                _jumpBufferCounter - Time.fixedDeltaTime
            );
        }

        private void ApplyGroundMovement()
        {
            Vector3 velocity = _rigidbody.linearVelocity;

            Vector3 horizontalVelocity = new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

            Vector3 targetVelocity = _moveDirection * _moveSpeed;

            float acceleration = _moveDirection.sqrMagnitude > 0.01f
                ? _groundAcceleration
                : _groundDeceleration;

            Vector3 newHorizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );

            _rigidbody.linearVelocity = new Vector3(
                newHorizontalVelocity.x,
                velocity.y,
                newHorizontalVelocity.z
            );
        }

        private void ApplyAirMovement()
        {
            if (_moveDirection.sqrMagnitude < 0.01f)
                return;

            Vector3 velocity = _rigidbody.linearVelocity;

            Vector3 horizontalVelocity = new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

            float currentSpeed = Vector3.Dot(
                horizontalVelocity,
                _moveDirection
            );

            float speedToAdd = _airSpeedLimit - currentSpeed;

            if (speedToAdd <= 0f)
                return;

            float acceleration = _airAcceleration * _airControl;

            float accelerationAmount = Mathf.Min(
                acceleration * Time.fixedDeltaTime,
                speedToAdd
            );

            horizontalVelocity += _moveDirection * accelerationAmount;

            _rigidbody.linearVelocity = new Vector3(
                horizontalVelocity.x,
                velocity.y,
                horizontalVelocity.z
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

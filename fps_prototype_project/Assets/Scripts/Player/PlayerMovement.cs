
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader _playerInput;
        [SerializeField] private Transform _orientation;

        [Header("Ground Movement")]
        [SerializeField] private float _moveSpeed = 7f;
        [SerializeField] private float _groundAcceleration = 40f;
        [SerializeField] private float _groundDeceleration = 40f;

        [Header("Sprint Movement")]
        [SerializeField] private float _sprintSpeed = 10f;

        [Header("Air Movement")]
        [SerializeField] private float _airAcceleration = 15f;
        [SerializeField] private float _airControl = 0.5f;
        [SerializeField] private float _airSpeedLimit = 7f;

        [Header("Jump")]
        [SerializeField, Min(1)] private int _maxJumps = 2;
        [SerializeField, Min(0.1f)] private float _jumpHeight = 2f;
        [SerializeField, Min(0.1f)] private float _gravity = 25f;
        [SerializeField, Min(1f)] private float _fallMultiplier = 2f;
        [SerializeField, Min(1f)] private float _jumpReleaseGravityMultiplier = 2.5f;
        [SerializeField] private float _coyoteTime = 0.12f;
        [SerializeField] private float _jumpBufferTime = 0.15f;

        [Header("Dash")]
        [SerializeField, Min(0f)] private float _dashSpeed = 18f;
        [SerializeField, Min(0.01f)] private float _dashDuration = 0.2f;
        [SerializeField, Min(0f)] private float _dashCooldown = 0.8f;
        [SerializeField] private bool _allowGroundDash = true;
        [SerializeField] private bool _allowAirDash = true;
        [SerializeField] private bool _dashUsesGravity;

        [Header("Crouch")]
        [SerializeField] private float _crouchHeight = 1f;
        [SerializeField] private float _crouchSpeed = 3.5f;
        [SerializeField] private float _crouchAcceleration = 30f;
        [SerializeField] private float _crouchDeceleration = 30f;

        [Header("Slide")]
        [SerializeField] private LayerMask _standCheckLayers = ~0;
        [SerializeField] private float _slideDuration = 0.8f;
        [SerializeField] private float _slideCooldown = 0.5f;
        [SerializeField] private float _slideFriction = 8f;
        [SerializeField] private float _slideMinimumSpeed = 6f;
        [SerializeField] private float _slideExitSpeed = 3f;
        [SerializeField] private float _slideHeight = 1f;
        [SerializeField] private float _slideCenterOffset = 0.5f;

        [Header("Ground Detection")]
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private LayerMask _groundLayers;

        private CharacterController _controller;

        private Vector3 _moveDirection;
        private Vector3 _horizontalVelocity;
        private Vector3 _dashDirection;

        private float _verticalVelocity;

        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;

        private float _dashTimeRemaining;
        private float _dashCooldownCounter;

        private int _jumpsRemaining;

        private bool _isGrounded;
        private bool _hasJumpedSinceGround;
        private bool _isDashing;

        private float _slideTimeRemaining;
        private float _slideCooldownCounter;
        private float _originalControllerHeight;
        private Vector3 _originalControllerCenter;
        private bool _isSliding;
        private bool _isCrouched;

        public bool IsGrounded => _isGrounded;
        public bool IsSprinting { get; private set; }
        public bool IsDashing => _isDashing;
        public bool IsSliding => _isSliding;
        public bool IsCrouching => _isCrouched && !_isSliding;
        public int JumpsRemaining => _jumpsRemaining;

        public Vector3 Velocity =>
            _horizontalVelocity + Vector3.up * _verticalVelocity;

        public float DashCooldownRemaining => _dashCooldownCounter;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _originalControllerHeight = _controller.height;
            _originalControllerCenter = _controller.center;

            _jumpsRemaining = _maxJumps;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            CheckGround();
            CalculateMovement();

            HandleJumpInput(deltaTime);
            UpdateDashCooldown(deltaTime);
            UpdateSlideCooldown(deltaTime);

            if (!_isDashing && !_isSliding)
            {
                HandleDashInput();
            }

            if (_isDashing)
            {
                ApplyDashMovement(deltaTime);
                return;
            }

            //if (!_isSliding)
            //{
            //    HandleSlideInput();
            //}

            //if (_isSliding)
            //{
            //    ApplySlideMovement(deltaTime);
            //    return;
            //}

            //if (_isCrouched && !TryStandUp())
            //{
            //    UpdateVerticalVelocity(deltaTime);
            //    ApplyMovement(deltaTime);
            //    return;
            //}

            HandleCrouchInput();
            HandleJump();

            UpdateVerticalVelocity(deltaTime);
            ApplyHorizontalMovement(deltaTime);
            ApplyMovement(deltaTime);
        }

        private void CheckGround()
        {
            _isGrounded = _controller.isGrounded;

            if (_isGrounded)
            {
                _coyoteTimeCounter = _coyoteTime;
                _jumpsRemaining = _maxJumps;
                _hasJumpedSinceGround = false;

                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -2f;
                }
            }
            else
            {
                UpdateCoyoteTime(Time.deltaTime);
            }
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

        #region Jump

        private void HandleJumpInput(float deltaTime)
        {
            if (_playerInput.ConsumeJumpPressed())
            {
                _jumpBufferCounter = _jumpBufferTime;
            }
            else if (_jumpBufferCounter > 0f)
            {
                _jumpBufferCounter = Mathf.Max(
                    0f,
                    _jumpBufferCounter - deltaTime
                );
            }
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

            _verticalVelocity = Mathf.Sqrt(
                2f * _gravity * _jumpHeight
            );

            _isGrounded = false;
            _hasJumpedSinceGround = true;

            _coyoteTimeCounter = 0f;
            _jumpBufferCounter = 0f;
        }

        private void UpdateCoyoteTime(float deltaTime)
        {
            if (_hasJumpedSinceGround)
                return;

            _coyoteTimeCounter -= deltaTime;

            if (_coyoteTimeCounter <= 0f)
            {
                _coyoteTimeCounter = 0f;

                _jumpsRemaining = Mathf.Max(
                    0,
                    _jumpsRemaining - 1
                );

                _hasJumpedSinceGround = true;
            }
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (_isGrounded && _verticalVelocity <= 0f)
                return;

            float gravityMultiplier = 1f;

            if (_verticalVelocity < 0f)
            {
                gravityMultiplier = _fallMultiplier;
            }
            else if (!_playerInput.JumpHeld)
            {
                gravityMultiplier = _jumpReleaseGravityMultiplier;
            }

            _verticalVelocity -=
                _gravity * gravityMultiplier * deltaTime;
        }

        #endregion

        #region Dash

        private void UpdateDashCooldown(float deltaTime)
        {
            if (_dashCooldownCounter <= 0f)
                return;

            _dashCooldownCounter = Mathf.Max(
                0f,
                _dashCooldownCounter - deltaTime
            );
        }

        private void HandleDashInput()
        {
            if (!_playerInput.ConsumeDashPressed())
                return;

            if (_dashCooldownCounter > 0f)
                return;

            if (_isGrounded && !_allowGroundDash)
                return;

            if (!_isGrounded && !_allowAirDash)
                return;

            StartDash();
        }

        private void StartDash()
        {
            if (_moveDirection.sqrMagnitude > 0.01f)
            {
                _dashDirection = _moveDirection;
            }
            else
            {
                Vector3 forward = _orientation.forward;
                forward.y = 0f;

                _dashDirection = forward.normalized;
            }

            _isDashing = true;

            _dashTimeRemaining = _dashDuration;
            _dashCooldownCounter = _dashCooldown;

            _horizontalVelocity = _dashDirection * _dashSpeed;
        }

        private void ApplyDashMovement(float deltaTime)
        {
            if (_dashUsesGravity)
            {
                UpdateVerticalVelocity(deltaTime);
            }

            Vector3 velocity = _dashDirection * _dashSpeed;

            velocity.y = _verticalVelocity;

            CollisionFlags collisionFlags = _controller.Move(
                velocity * deltaTime
            );

            _dashTimeRemaining -= deltaTime;

            bool hitWall = (collisionFlags & CollisionFlags.Sides) != 0;

            bool hitCeiling = (collisionFlags & CollisionFlags.Above) != 0;

            if (hitCeiling && _verticalVelocity > 0f)
            {
                _verticalVelocity = 0f;
            }

            _isGrounded = _controller.isGrounded;

            if (_isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            if (hitWall || _dashTimeRemaining <= 0f)
            {
                StopDash(hitWall);
            }
        }

        private void StopDash(bool hitWall)
        {
            _isDashing = false;

            _dashTimeRemaining = 0f;

            if (hitWall)
            {
                _horizontalVelocity = Vector3.zero;
            }
        }

        #endregion

        #region Crouch

        private void HandleCrouchInput()
        {
            if (_isSliding)
            {
                return;
            }

            if (_playerInput.CrouchHeld)
            {
                if (!_isCrouched)
                {
                    StartCrouch();
                }

                return;
            }

            if (_isCrouched)
            {
                TryStandUp();
            }
        }

        private void StartCrouch()
        {
            if (_isSliding || !_isGrounded)
            {
                return;
            }

            _isCrouched = true;

            IsSprinting = false;

            SetControllerHeight(_crouchHeight);
        }

        #endregion

        #region Slide

        private void UpdateSlideCooldown(float deltaTime)
        {
            if (_slideCooldownCounter > 0f)
            {
                _slideCooldownCounter -= deltaTime;
            }
        }

        private void HandleSlideInput()
        {
            if (!_playerInput.ConsumeSlidePressed())
            {
                return;
            }

            if (!_isGrounded || _slideCooldownCounter > 0f)
            {
                return;
            }

            if (_horizontalVelocity.magnitude < _slideMinimumSpeed)
            {
                return;
            }

            StartSlide();
        }

        private void StartSlide()
        {
            _isSliding = true;
            _isCrouched = true;

            _slideTimeRemaining = _slideDuration;
            _slideCooldownCounter = _slideCooldown;

            IsSprinting = false;

            SetControllerHeight(_slideHeight);
        }

        private void SetControllerHeight(float height)
        {
            height = Mathf.Max(height, _controller.radius * 2f);

            float feetPosition = _originalControllerCenter.y -
                                 _originalControllerHeight * 0.5f;

            _controller.height = height;

            _controller.center = new Vector3(
                _originalControllerCenter.x,
                feetPosition + height * 0.5f,
                _originalControllerCenter.z
            );
        }

        private bool CanStandUp()
        {
            Vector3 worldCenter = transform.TransformPoint(_originalControllerCenter);

            float scaleY = transform.lossyScale.y;
            float scaleXZ = Mathf.Max(
                transform.lossyScale.x,
                transform.lossyScale.z
            );

            float radius = _controller.radius * scaleXZ;
            float height = _originalControllerHeight * scaleY;

            float halfSegment = Mathf.Max(0f, height * 0.5f - radius);

            Vector3 point1 = worldCenter + transform.up * halfSegment;
            Vector3 point2 = worldCenter - transform.up * halfSegment;

            Collider[] colliders = Physics.OverlapCapsule(
                point1,
                point2,
                radius,
                _standCheckLayers,
                QueryTriggerInteraction.Ignore
            );

            foreach (Collider hit in colliders)
            {
                if (hit != _controller)
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplySlideMovement(float deltaTime)
        {
            _slideTimeRemaining -= deltaTime;

            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity,
                Vector3.zero,
                _slideFriction * deltaTime
            );

            UpdateVerticalVelocity(deltaTime);
            ApplyMovement(deltaTime);

            bool shouldEndSlide =
                !_isGrounded ||
                _slideTimeRemaining <= 0f ||
                _horizontalVelocity.magnitude <= _slideExitSpeed ||
                !_playerInput.SlideHeld;

            if (shouldEndSlide)
            {
                EndSlide();
            }
        }

        private void EndSlide()
        {
            if (!_isSliding)
            {
                return;
            }

            _isSliding = false;

            TryStandUp();
        }

        private bool TryStandUp()
        {
            if (!_isCrouched)
            {
                return true;
            }

            if (!CanStandUp())
            {
                return false;
            }

            SetControllerHeight(_originalControllerHeight);

            _isCrouched = false;

            return true;
        }

        #endregion

        #region Regular Movement

        private void ApplyHorizontalMovement(float deltaTime)
        {
            if (_isGrounded)
            {
                ApplyGroundMovement(deltaTime);
            }
            else
            {
                ApplyAirMovement(deltaTime);
            }
        }

        private void ApplyGroundMovement(float deltaTime)
        {
            bool hasMovementInput = _moveDirection.sqrMagnitude > 0.01f;

            if (_isCrouched)
            {
                IsSprinting = false;

                Vector3 targetVelocity = _moveDirection * _crouchSpeed;

                float acceleration = hasMovementInput
                    ? _crouchAcceleration
                    : _crouchDeceleration;

                _horizontalVelocity = Vector3.MoveTowards(
                    _horizontalVelocity,
                    targetVelocity,
                    acceleration * deltaTime
                );

                return;
            }

            IsSprinting = _playerInput.SprintHeld && hasMovementInput;

            float targetSpeed = IsSprinting ? _sprintSpeed : _moveSpeed;

            Vector3 groundTargetVelocity = _moveDirection * targetSpeed;

            float groundAcceleration = hasMovementInput
                ? _groundAcceleration
                : _groundDeceleration;

            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity,
                groundTargetVelocity,
                groundAcceleration * deltaTime
            );
        }

        private void ApplyAirMovement(float deltaTime)
        {
            if (_moveDirection.sqrMagnitude < 0.01f)
                return;

            float currentSpeed = Vector3.Dot(
                _horizontalVelocity,
                _moveDirection
            );

            float speedToAdd = _airSpeedLimit - currentSpeed;

            if (speedToAdd <= 0f)
                return;

            float acceleration = _airAcceleration * _airControl;

            float accelerationAmount = Mathf.Min(
                acceleration * deltaTime,
                speedToAdd
            );

            _horizontalVelocity +=
                _moveDirection * accelerationAmount;
        }

        private void ApplyMovement(float deltaTime)
        {
            Vector3 velocity = _horizontalVelocity +
                               Vector3.up * _verticalVelocity;

            CollisionFlags collisionFlags = _controller.Move(
                velocity * deltaTime
            );

            if ((collisionFlags & CollisionFlags.Above) != 0 &&
                _verticalVelocity > 0f)
            {
                _verticalVelocity = 0f;
            }

            if ((collisionFlags & CollisionFlags.Below) != 0 &&
                _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _isGrounded = _controller.isGrounded;
        }

        #endregion
    }
}

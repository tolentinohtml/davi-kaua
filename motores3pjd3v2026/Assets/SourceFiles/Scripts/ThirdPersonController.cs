using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    [RequireComponent(typeof(PlayerInput))]
    public class ThirdPersonController : MonoBehaviour
    {
        public int PlayerID = 1;

        public Transform playerCameraTransform;

        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;

        public float SpeedBoostPerCoin = 0.5f;

        [Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        public GameObject CinemachineCameraTarget;

        public bool IsRespawning { get; set; } = false;

        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private int _moedasColetadas = 0;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private PlayerInput _playerInput;

        private bool _hasAnimator;

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _playerInput = GetComponent<PlayerInput>();

            if (_playerInput != null && _playerInput.currentActionMap != null)
            {
                _playerInput.currentActionMap.Enable();
            }

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            if (playerCameraTransform == null && Camera.main != null)
            {
                playerCameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            AutoAlignCamera();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Coin") || other.gameObject.name.Contains("Coin"))
            {
                Destroy(other.gameObject);
                _moedasColetadas++;
                ApplySpeedBoost();
                PlayerOM.NotifyCoinCollected(PlayerID, _moedasColetadas);
            }
        }

        public void ApplySpeedBoost()
        {
            MoveSpeed += SpeedBoostPerCoin;
            SprintSpeed += SpeedBoostPerCoin;
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private Vector2 GetMoveInput()
        {
            if (_playerInput != null && _playerInput.actions != null && _playerInput.actions.FindAction("Move") != null)
            {
                return _playerInput.actions["Move"].ReadValue<Vector2>();
            }
            return _input != null ? _input.move : Vector2.zero;
        }

        private bool GetSprintInput()
        {
            if (_playerInput != null && _playerInput.actions != null && _playerInput.actions.FindAction("Sprint") != null)
            {
                return _playerInput.actions["Sprint"].IsPressed();
            }
            return _input != null && _input.sprint;
        }

        private bool GetJumpInput()
        {
            if (_playerInput != null && _playerInput.actions != null && _playerInput.actions.FindAction("Jump") != null)
            {
                return _playerInput.actions["Jump"].IsPressed();
            }
            return _input != null && _input.jump;
        }

        private void Move()
        {
            Vector2 moveInput = GetMoveInput();
            bool isSprinting = GetSprintInput();

            float targetSpeed = isSprinting ? SprintSpeed : MoveSpeed;
            if (moveInput == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = (_input != null && _input.analogMovement) ? moveInput.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

            if (moveInput != Vector2.zero)
            {
                float cameraYaw = playerCameraTransform != null ? playerCameraTransform.eulerAngles.y : 0f;
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraYaw;

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void AutoAlignCamera()
        {
            if (CinemachineCameraTarget == null) return;

            Vector2 moveInput = GetMoveInput();

            if (moveInput != Vector2.zero)
            {
                Quaternion targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                CinemachineCameraTarget.transform.rotation = Quaternion.Slerp(
                    CinemachineCameraTarget.transform.rotation,
                    targetRotation,
                    Time.deltaTime * 3.0f
                );
            }
        }

        private void JumpAndGravity()
        {
            bool jumpPressed = GetJumpInput();

            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

                if (jumpPressed && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    if (_hasAnimator) _animator.SetBool(_animIDJump, true);
                }

                if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;
                if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
                else if (_hasAnimator) _animator.SetBool(_animIDFreeFall, true);

                if (_input != null) _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity) _verticalVelocity += Gravity * Time.deltaTime;
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f && FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public void ResetCameraRotation(float targetYaw)
        {
            if (CinemachineCameraTarget != null)
            {
                CinemachineCameraTarget.transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
            }
        }
    }
}
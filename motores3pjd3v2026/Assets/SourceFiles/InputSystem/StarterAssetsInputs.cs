using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(PlayerInput))]
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        private PlayerInput _playerInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            SetCursorState(cursorLocked);

#if ENABLE_INPUT_SYSTEM
            // Força a Unity a vincular o teclado atual a este PlayerInput (resolve o bloqueio do Player 2)
            if (_playerInput != null && Keyboard.current != null)
            {
                InputUser.PerformPairingWithDevice(Keyboard.current, _playerInput.user);
            }
#endif
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            // Leitura direta do Input Action Asset configurado no PlayerInput
            if (_playerInput != null && _playerInput.actions != null)
            {
                InputAction moveAction = _playerInput.actions.FindAction("Move");
                if (moveAction != null) move = moveAction.ReadValue<Vector2>();

                InputAction jumpAction = _playerInput.actions.FindAction("Jump");
                if (jumpAction != null) jump = jumpAction.IsPressed();

                InputAction sprintAction = _playerInput.actions.FindAction("Sprint");
                if (sprintAction != null) sprint = sprintAction.IsPressed();
            }
#endif
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !newState;
        }
    }
}
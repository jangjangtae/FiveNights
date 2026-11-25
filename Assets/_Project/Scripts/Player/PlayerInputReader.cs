using UnityEngine;
using UnityEngine.InputSystem;

namespace FiveNights.Player
{
    /// <summary>
    /// 프로젝트 전역 Input Actions(Assets/Settings/InputSystem_Actions)에서 플레이어 입력을 읽는다.
    /// 다른 컴포넌트는 Input System을 직접 보지 않고 이 값만 읽는다.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        private InputAction _move;
        private InputAction _sprint;
        private InputAction _crouch;

        public Vector2 Move { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool CrouchHeld { get; private set; }
        /// <summary>이번 프레임에 웅크리기 키를 눌렀는지. 토글 처리에 쓴다.</summary>
        public bool CrouchPressedThisFrame { get; private set; }

        private void Awake()
        {
            var actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogError("[PlayerInputReader] 프로젝트 전역 Input Actions가 설정되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _move = actions.FindAction("Player/Move", throwIfNotFound: true);
            _sprint = actions.FindAction("Player/Sprint", throwIfNotFound: true);
            _crouch = actions.FindAction("Player/Crouch", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _move?.Enable();
            _sprint?.Enable();
            _crouch?.Enable();
        }

        private void Update()
        {
            Move = _move.ReadValue<Vector2>();
            SprintHeld = _sprint.IsPressed();
            CrouchHeld = _crouch.IsPressed();
            CrouchPressedThisFrame = _crouch.WasPressedThisFrame();
        }
    }
}

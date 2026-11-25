using UnityEngine;

namespace FiveNights.Player
{
    /// <summary>
    /// 플레이어 이동 로직. 입력을 받아 이동 방식(MovementMode)을 정하고 Rigidbody2D를 움직인다.
    /// 발소리 반경(CurrentNoiseRadius)은 나중에 경비의 청각(Perception)이 읽는다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlayerMovementSettings _settings;

        private Rigidbody2D _body;
        private PlayerInputReader _input;
        private bool _crouchToggled;

        public MovementMode CurrentMode { get; private set; } = MovementMode.Idle;
        public bool IsCrouching => CurrentMode == MovementMode.Crouch || (CurrentMode == MovementMode.Idle && WantsCrouch);
        public float CurrentNoiseRadius => _settings != null ? _settings.GetNoiseRadius(CurrentMode) : 0f;
        public Vector2 Velocity => _body != null ? _body.linearVelocity : Vector2.zero;
        public Vector2 Facing { get; private set; } = Vector2.right;
        public PlayerMovementSettings Settings => _settings;

        private bool WantsCrouch => _settings != null && _settings.crouchIsToggle ? _crouchToggled : _input.CrouchHeld;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _input = GetComponent<PlayerInputReader>();

            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (_settings == null)
                Debug.LogError("[PlayerMotor] PlayerMovementSettings가 지정되지 않았습니다.", this);
        }

        private void Update()
        {
            if (_settings == null) return;

            if (_settings.crouchIsToggle && _input.CrouchPressedThisFrame)
                _crouchToggled = !_crouchToggled;

            Vector2 move = PlayerMovementRules.ClampInput(_input.Move);
            CurrentMode = PlayerMovementRules.Resolve(move, WantsCrouch, _input.SprintHeld);

            // 달리기를 시작하면 토글된 웅크리기는 풀린다.
            if (CurrentMode == MovementMode.Sprint)
                _crouchToggled = false;

            if (move.sqrMagnitude > PlayerMovementRules.MoveDeadZone * PlayerMovementRules.MoveDeadZone)
                Facing = move.normalized;
        }

        private void FixedUpdate()
        {
            if (_settings == null) return;

            Vector2 move = PlayerMovementRules.ClampInput(_input.Move);
            Vector2 target = move * _settings.GetSpeed(CurrentMode);
            float rate = target.sqrMagnitude > 0.0001f ? _settings.acceleration : _settings.deceleration;
            _body.linearVelocity = Vector2.MoveTowards(_body.linearVelocity, target, rate * Time.fixedDeltaTime);
        }
    }
}

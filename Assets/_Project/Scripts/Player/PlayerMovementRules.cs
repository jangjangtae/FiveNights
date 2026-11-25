using UnityEngine;

namespace FiveNights.Player
{
    /// <summary>
    /// 입력 → 이동 방식 결정 규칙. 그래픽·물리와 무관한 순수 로직이라 EditMode 테스트로 검증할 수 있다.
    /// </summary>
    public static class PlayerMovementRules
    {
        /// <summary>이 값보다 작은 입력은 정지로 본다 (스틱 미세 흔들림 무시).</summary>
        public const float MoveDeadZone = 0.1f;

        /// <summary>
        /// 웅크린 상태에서 달리기를 누르면 달리기가 우선한다 (웅크리기 해제는 호출 쪽에서 처리).
        /// </summary>
        public static MovementMode Resolve(Vector2 moveInput, bool crouching, bool sprintHeld)
        {
            if (moveInput.sqrMagnitude < MoveDeadZone * MoveDeadZone)
                return MovementMode.Idle;
            if (sprintHeld)
                return MovementMode.Sprint;
            return crouching ? MovementMode.Crouch : MovementMode.Walk;
        }

        /// <summary>대각선 이동이 더 빠르지 않도록 길이를 1로 제한한다.</summary>
        public static Vector2 ClampInput(Vector2 moveInput)
        {
            return moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput;
        }
    }
}

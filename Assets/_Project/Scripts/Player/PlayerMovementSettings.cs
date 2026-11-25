using UnityEngine;

namespace FiveNights.Player
{
    /// <summary>플레이어 이동 수치. 값은 플레이테스트로 조정한다.</summary>
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "FiveNights/Player Movement Settings")]
    public class PlayerMovementSettings : ScriptableObject
    {
        [Header("속도 (유닛/초)")]
        [Min(0f)] public float crouchSpeed = 1.6f;
        [Min(0f)] public float walkSpeed = 3.2f;
        [Min(0f)] public float sprintSpeed = 5.5f;

        [Header("가감속 (유닛/초²)")]
        [Min(0f)] public float acceleration = 30f;
        [Min(0f)] public float deceleration = 40f;

        [Header("발소리 반경 (유닛)")]
        [Min(0f)] public float idleNoiseRadius = 0f;
        [Min(0f)] public float crouchNoiseRadius = 1f;
        [Min(0f)] public float walkNoiseRadius = 3f;
        [Min(0f)] public float sprintNoiseRadius = 6f;

        [Header("조작")]
        [Tooltip("켜면 웅크리기 키를 눌러 토글, 끄면 누르고 있는 동안만 웅크림")]
        public bool crouchIsToggle = true;

        public float GetSpeed(MovementMode mode)
        {
            switch (mode)
            {
                case MovementMode.Crouch: return crouchSpeed;
                case MovementMode.Walk: return walkSpeed;
                case MovementMode.Sprint: return sprintSpeed;
                default: return 0f;
            }
        }

        public float GetNoiseRadius(MovementMode mode)
        {
            switch (mode)
            {
                case MovementMode.Crouch: return crouchNoiseRadius;
                case MovementMode.Walk: return walkNoiseRadius;
                case MovementMode.Sprint: return sprintNoiseRadius;
                default: return idleNoiseRadius;
            }
        }
    }
}

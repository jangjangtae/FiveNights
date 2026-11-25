using UnityEngine;

namespace FiveNights.Guards
{
    /// <summary>경비 수치. 값은 플레이테스트로 조정한다.</summary>
    [CreateAssetMenu(fileName = "GuardSettings", menuName = "FiveNights/Guard Settings")]
    public class GuardSettings : ScriptableObject
    {
        [Header("이동 속도 (유닛/초)")]
        [Min(0f)] public float patrolSpeed = 1.8f;
        [Min(0f)] public float investigateSpeed = 2.2f;
        [Min(0f)] public float searchSpeed = 2.6f;
        [Min(0f)] public float chaseSpeed = 4.4f;
        [Min(0f)] public float returnSpeed = 2.2f;
        [Tooltip("초당 회전 각도")]
        [Min(0f)] public float turnSpeed = 300f;

        [Header("시야")]
        [Min(0f)] public float viewDistance = 7f;
        [Range(10f, 180f)] public float viewAngle = 70f;
        [Tooltip("이 거리 안은 방향과 상관없이 알아챈다")]
        [Min(0f)] public float proximityRadius = 1.0f;

        [Header("청각")]
        [Tooltip("벽 너머 소리 반경을 얼마나 줄이는가 (0 = 그대로, 1 = 안 들림)")]
        [Range(0f, 1f)] public float wallDampening = 0.5f;

        [Header("의심 게이지")]
        [Min(0.05f)] public float fillTimeAtMaxRange = 1.5f;
        [Min(1f)] public float closeRangeMultiplier = 4f;
        [Min(0f)] public float hearingRate = 0.5f;
        [Range(0f, 1f)] public float hearingCap = 0.8f;
        [Min(0f)] public float decayRate = 0.2f;

        [Header("행동")]
        [Tooltip("순찰 지점에서 멈춰 둘러보는 시간")]
        [Min(0f)] public float waypointWait = 1.0f;
        [Tooltip("놓친 뒤 마지막 위치 주변을 수색하는 시간")]
        [Min(0f)] public float searchDuration = 6f;
        [Min(0f)] public float searchRadius = 3f;
        [Tooltip("의심 지점에 도착한 뒤 둘러보는 시간")]
        [Min(0f)] public float investigateLookTime = 2f;
        [Tooltip("이 거리 안에서 추격 중이면 잡는다")]
        [Min(0f)] public float catchDistance = 0.75f;
        [Tooltip("추격 경로를 다시 계산하는 간격(초)")]
        [Min(0.05f)] public float repathInterval = 0.25f;
        [Tooltip("도착으로 보는 거리")]
        [Min(0.01f)] public float arriveDistance = 0.25f;
    }
}

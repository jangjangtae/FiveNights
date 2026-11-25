namespace FiveNights.Guards
{
    /// <summary>경비의 행동 상태. GDD 5장: 순찰 → 의심 → 추격 → 수색 → 복귀.</summary>
    public enum GuardState
    {
        Patrol,
        Investigate,
        Chase,
        Search,
        Return
    }
}

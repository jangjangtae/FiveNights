using FiveNights.Perception;

namespace FiveNights.Guards
{
    /// <summary>
    /// 경비의 판단부. 관측(GuardObservation) + 자기 정보(GuardContext) → 행동 명령(GuardCommand).
    /// 1차 구현은 FsmGuardBrain(유한 상태 기계), 4단계에서 ML-Agents 정책으로 교체해 비교한다.
    /// 판단 코드는 Transform이나 Rigidbody를 직접 만지지 않는다.
    /// </summary>
    public interface IGuardBrain
    {
        GuardState State { get; }
        GuardCommand Decide(in GuardObservation observation, in GuardContext context, float deltaTime);
        /// <summary>잡힘 처리 후 등, 처음 상태로 되돌린다.</summary>
        void ResetBrain(int startWaypoint);
    }
}

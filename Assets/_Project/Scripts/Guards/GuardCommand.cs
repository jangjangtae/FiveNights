using UnityEngine;

namespace FiveNights.Guards
{
    /// <summary>
    /// 두뇌(IGuardBrain)가 내리는 한 번의 행동 명령. 실행은 GuardMover/GuardController가 한다.
    /// RL 정책으로 바꿀 때도 같은 형식으로 출력하게 한다.
    /// </summary>
    public struct GuardCommand
    {
        public GuardState State;

        public bool HasMoveTarget;
        public Vector2 MoveTarget;
        public float MoveSpeed;

        /// <summary>이동 방향 대신 이 지점을 바라본다.</summary>
        public bool HasLookTarget;
        public Vector2 LookTarget;

        /// <summary>제자리에서 고개를 돌리며 둘러본다 (초당 각도, 0이면 안 함).</summary>
        public float ScanSpeed;

        /// <summary>플레이어를 잡았다.</summary>
        public bool Catch;
    }
}

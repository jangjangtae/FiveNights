using System.Collections.Generic;
using FiveNights.Core.Navigation;
using UnityEngine;

namespace FiveNights.Guards
{
    /// <summary>두뇌가 판단할 때 참고하는 경비 자신의 정보 (위치, 방향, 순찰 경로, 길찾기).</summary>
    public struct GuardContext
    {
        public Vector2 Position;
        public Vector2 Facing;
        public IReadOnlyList<Vector2> PatrolPoints;
        public INavQuery Nav;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace FiveNights.Core.Navigation
{
    /// <summary>경비 두뇌·이동이 쓰는 길찾기 질의. 구현(격자 A*, NavMesh 등)을 바꿔도 이 형식은 유지한다.</summary>
    public interface INavQuery
    {
        bool IsWalkable(Vector2 point);
        Vector2 ClosestWalkable(Vector2 point);
        /// <summary>from → to 경로(웨이포인트 목록)를 result에 채운다. 못 찾으면 false.</summary>
        bool FindPath(Vector2 from, Vector2 to, List<Vector2> result);
        /// <summary>center 주변 radius 안에서 걸어갈 수 있는 임의의 지점.</summary>
        Vector2 RandomWalkableNear(Vector2 center, float radius, System.Random rng);
    }
}

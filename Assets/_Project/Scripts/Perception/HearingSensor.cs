using UnityEngine;

namespace FiveNights.Perception
{
    /// <summary>
    /// 청각 판정: 소리 반경 안에 경비가 있으면 들린다.
    /// 벽 너머 소리는 반경을 줄여서 계산한다 (벽이 소리를 일부 막음).
    /// </summary>
    public class HearingSensor
    {
        public float WallDampening { get; set; }
        public LayerMask Obstacles { get; set; }

        public HearingSensor(float wallDampening, LayerMask obstacles)
        {
            WallDampening = wallDampening;
            Obstacles = obstacles;
        }

        public bool CanHear(Vector2 ear, Vector2 source, float noiseRadius)
        {
            if (noiseRadius <= 0f) return false;

            float dist = Vector2.Distance(ear, source);
            if (dist > noiseRadius) return false;

            bool blocked = Physics2D.Linecast(ear, source, Obstacles).collider != null;
            float effective = blocked ? noiseRadius * (1f - WallDampening) : noiseRadius;
            return dist <= effective;
        }
    }
}

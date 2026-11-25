using UnityEngine;

namespace FiveNights.Perception
{
    /// <summary>
    /// 시야 판정: 부채꼴(거리·각도) 안에 있고, 장애물 레이어에 가리지 않으면 보인다.
    /// 아주 가까운 거리는 방향과 상관없이 알아챈다.
    /// </summary>
    public class VisionSensor
    {
        public float ViewDistance { get; set; }
        public float ViewAngle { get; set; }
        public float ProximityRadius { get; set; }
        public LayerMask Obstacles { get; set; }

        public VisionSensor(float viewDistance, float viewAngle, float proximityRadius, LayerMask obstacles)
        {
            ViewDistance = viewDistance;
            ViewAngle = viewAngle;
            ProximityRadius = proximityRadius;
            Obstacles = obstacles;
        }

        public bool CanSee(Vector2 eye, Vector2 facing, Vector2 target, out float distance01)
        {
            distance01 = 1f;
            Vector2 to = target - eye;
            float dist = to.magnitude;

            bool inProximity = dist <= ProximityRadius;
            bool inCone = dist <= ViewDistance && Vector2.Angle(facing, to) <= ViewAngle * 0.5f;
            if (!inProximity && !inCone) return false;

            if (!HasLineOfSight(eye, target)) return false;

            distance01 = ViewDistance > 0f ? Mathf.Clamp01(dist / ViewDistance) : 0f;
            return true;
        }

        public bool HasLineOfSight(Vector2 from, Vector2 to)
        {
            return Physics2D.Linecast(from, to, Obstacles).collider == null;
        }
    }
}

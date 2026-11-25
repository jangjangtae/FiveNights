using UnityEngine;

namespace FiveNights.Perception
{
    /// <summary>
    /// 경비 한 명이 한 번의 판단에 쓰는 관측값. 규칙 기반 두뇌와 나중의 RL 정책이 같은 형식을 쓴다.
    /// 플레이어의 실제 위치는 감지했을 때만 들어간다.
    /// </summary>
    public struct GuardObservation
    {
        /// <summary>시야 안에 플레이어가 보이는가.</summary>
        public bool SeesTarget;
        /// <summary>본 위치 (SeesTarget일 때만 유효).</summary>
        public Vector2 SeenPosition;
        /// <summary>본 거리 / 시야 거리 (0 = 바로 앞, 1 = 시야 끝).</summary>
        public float SeenDistance01;

        /// <summary>발소리가 들리는가.</summary>
        public bool HearsNoise;
        /// <summary>들린 위치 (HearsNoise일 때만 유효).</summary>
        public Vector2 NoisePosition;

        /// <summary>잡을 수 있을 만큼 가까이 닿아 있는가.</summary>
        public bool TouchingTarget;

        /// <summary>감지로 누적된 의심 수치 (0~1).</summary>
        public float Suspicion;
    }
}

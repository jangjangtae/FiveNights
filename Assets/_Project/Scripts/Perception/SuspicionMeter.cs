using UnityEngine;

namespace FiveNights.Perception
{
    /// <summary>
    /// 의심 게이지 (0~1). 보이면 가까울수록 빨리 차고, 소리만으로는 상한까지만 찬다.
    /// 감지가 없으면 천천히 줄어든다. 순수 로직이라 테스트할 수 있다.
    /// </summary>
    public class SuspicionMeter
    {
        public float Value { get; private set; }

        /// <summary>시야 끝에서 0 → 1까지 걸리는 시간(초).</summary>
        public float FillTimeAtMaxRange = 1.5f;
        /// <summary>바로 앞에서는 몇 배 빨리 차는가.</summary>
        public float CloseRangeMultiplier = 4f;
        /// <summary>소리가 들리는 동안 초당 증가량.</summary>
        public float HearingRate = 0.5f;
        /// <summary>소리만으로 올라갈 수 있는 최대치 (발각은 눈으로 봐야 함).</summary>
        public float HearingCap = 0.8f;
        /// <summary>감지가 없을 때 초당 감소량.</summary>
        public float DecayRate = 0.2f;

        public bool IsFull => Value >= 1f;
        public bool IsEmpty => Value <= 0f;

        public void Tick(bool sees, float distance01, bool hears, float deltaTime)
        {
            if (sees)
            {
                float speed = Mathf.Lerp(CloseRangeMultiplier, 1f, distance01) / Mathf.Max(0.01f, FillTimeAtMaxRange);
                Value += speed * deltaTime;
            }
            else if (hears && Value < HearingCap)
            {
                Value = Mathf.Min(HearingCap, Value + HearingRate * deltaTime);
            }
            else if (!hears)
            {
                Value -= DecayRate * deltaTime;
            }

            Value = Mathf.Clamp01(Value);
        }

        public void SetFull() => Value = 1f;
        public void Reset() => Value = 0f;
    }
}

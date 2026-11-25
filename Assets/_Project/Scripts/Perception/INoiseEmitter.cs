using UnityEngine;

namespace FiveNights.Perception
{
    /// <summary>소리를 내는 대상. 반경 안에 있는 경비가 들을 수 있다.</summary>
    public interface INoiseEmitter
    {
        Vector2 NoisePosition { get; }
        float NoiseRadius { get; }
    }
}

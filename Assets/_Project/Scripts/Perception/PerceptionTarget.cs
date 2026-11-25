using System.Collections.Generic;
using UnityEngine;

namespace FiveNights.Perception
{
    /// <summary>
    /// 경비가 감지하는 대상(플레이어)에 붙인다. 감지 쪽은 이 등록 목록만 보고, 플레이어 코드를 직접 참조하지 않는다.
    /// 같은 오브젝트에 INoiseEmitter가 있으면 발소리도 여기서 읽는다.
    /// </summary>
    public class PerceptionTarget : MonoBehaviour
    {
        private static readonly List<PerceptionTarget> _all = new List<PerceptionTarget>();
        public static IReadOnlyList<PerceptionTarget> All => _all;
        public static PerceptionTarget Primary => _all.Count > 0 ? _all[0] : null;

        private INoiseEmitter _noise;

        public Vector2 Position => transform.position;
        public float NoiseRadius => _noise != null ? _noise.NoiseRadius : 0f;

        private void Awake()
        {
            _noise = GetComponent<INoiseEmitter>();
        }

        private void OnEnable()
        {
            if (!_all.Contains(this)) _all.Add(this);
        }

        private void OnDisable()
        {
            _all.Remove(this);
        }
    }
}

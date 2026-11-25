using UnityEngine;

namespace FiveNights.Player
{
    /// <summary>
    /// 발소리 반경을 청록색 원으로 그린다 (LineRenderer). 반경 값은 PlayerMotor에서 읽기만 한다.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class NoiseRadiusVisualizer : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _motor;
        [SerializeField, Range(16, 128)] private int _segments = 64;
        [SerializeField] private float _lerpSpeed = 10f;
        [SerializeField] private Color _color = new Color(0.25f, 0.9f, 0.85f, 0.55f);
        [SerializeField] private float _lineWidth = 0.05f;

        private LineRenderer _line;
        private float _shownRadius;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = false;
            _line.loop = true;
            _line.positionCount = _segments;
            _line.startWidth = _line.endWidth = _lineWidth;
            _line.startColor = _line.endColor = _color;
        }

        private void LateUpdate()
        {
            if (_motor == null) return;

            float target = _motor.CurrentNoiseRadius;
            _shownRadius = Mathf.Lerp(_shownRadius, target, 1f - Mathf.Exp(-_lerpSpeed * Time.deltaTime));

            bool visible = _shownRadius > 0.05f;
            _line.enabled = visible;
            if (!visible) return;

            for (int i = 0; i < _segments; i++)
            {
                float a = i * Mathf.PI * 2f / _segments;
                _line.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * _shownRadius);
            }
        }
    }
}

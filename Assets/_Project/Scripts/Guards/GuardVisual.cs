using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FiveNights.Guards
{
    /// <summary>
    /// 경비 그래픽: 상태별 몸 색, 바라보는 방향(머리·손전등), 머리 위 의심 게이지.
    /// GuardController 상태를 읽기만 한다.
    /// </summary>
    public class GuardVisual : MonoBehaviour
    {
        [SerializeField] private GuardController _guard;
        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private Transform _head;
        [SerializeField] private Light2D _flashlight;
        [SerializeField] private Transform _suspicionFill;
        [SerializeField] private SpriteRenderer _suspicionFillRenderer;
        [SerializeField] private GameObject _suspicionBar;

        [Header("색 (GDD 7장 색 역할)")]
        [SerializeField] private Color _calmColor = new Color(0.95f, 0.82f, 0.35f);     // 손전등 노랑
        [SerializeField] private Color _suspiciousColor = new Color(1f, 0.55f, 0.15f);
        [SerializeField] private Color _alertColor = new Color(1f, 0.15f, 0.12f);        // 경보 빨강
        [SerializeField] private Color _flashlightCalm = new Color(1f, 0.92f, 0.6f);

        private void LateUpdate()
        {
            if (_guard == null) return;

            Vector2 f = _guard.Facing;
            float angle = Mathf.Atan2(f.y, f.x) * Mathf.Rad2Deg;
            if (_head != null) _head.rotation = Quaternion.Euler(0f, 0f, angle);

            GuardState state = _guard.State;
            Color stateColor = state == GuardState.Chase ? _alertColor
                : state == GuardState.Investigate || state == GuardState.Search ? _suspiciousColor
                : _calmColor;
            if (_body != null) _body.color = stateColor;

            if (_flashlight != null)
            {
                // Light2D 스팟은 로컬 위쪽(+Y)을 비춘다.
                _flashlight.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
                _flashlight.color = state == GuardState.Chase ? Color.Lerp(_flashlightCalm, _alertColor, 0.6f) : _flashlightCalm;
                var s = _guard.Settings;
                if (s != null)
                {
                    _flashlight.pointLightOuterRadius = s.viewDistance;
                    _flashlight.pointLightOuterAngle = s.viewAngle;
                    _flashlight.pointLightInnerAngle = s.viewAngle * 0.6f;
                }
            }

            float sus = _guard.Suspicion;
            if (_suspicionBar != null) _suspicionBar.SetActive(sus > 0.01f || state == GuardState.Chase);
            if (_suspicionFill != null)
            {
                Vector3 sc = _suspicionFill.localScale;
                sc.x = Mathf.Max(0.0001f, sus);
                _suspicionFill.localScale = sc;
                _suspicionFill.localPosition = new Vector3(-0.5f + sus * 0.5f, 0f, 0f);
            }
            if (_suspicionFillRenderer != null)
                _suspicionFillRenderer.color = Color.Lerp(_suspiciousColor, _alertColor, sus);
        }
    }
}

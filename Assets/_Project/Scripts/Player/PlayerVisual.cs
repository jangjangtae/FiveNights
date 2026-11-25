using UnityEngine;

namespace FiveNights.Player
{
    /// <summary>
    /// 플레이어 그래픽. PlayerMotor의 상태를 읽어서 웅크리면 납작하게, 바라보는 방향 표시를 돌린다.
    /// 로직은 건드리지 않는다.
    /// </summary>
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private Transform _body;
        [SerializeField] private Transform _facingIndicator;
        [SerializeField] private float _crouchScale = 0.75f;
        [SerializeField] private float _scaleLerpSpeed = 12f;

        private Vector3 _baseScale;

        private void Awake()
        {
            if (_body != null) _baseScale = _body.localScale;
        }

        private void LateUpdate()
        {
            if (_motor == null) return;

            if (_body != null)
            {
                float k = _motor.IsCrouching ? _crouchScale : 1f;
                Vector3 target = _baseScale * k;
                _body.localScale = Vector3.Lerp(_body.localScale, target, 1f - Mathf.Exp(-_scaleLerpSpeed * Time.deltaTime));
            }

            if (_facingIndicator != null)
            {
                Vector2 f = _motor.Facing;
                float angle = Mathf.Atan2(f.y, f.x) * Mathf.Rad2Deg;
                _facingIndicator.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}

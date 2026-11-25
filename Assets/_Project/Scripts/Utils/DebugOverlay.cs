using FiveNights.Core;
using FiveNights.Guards;
using FiveNights.Player;
using UnityEngine;

namespace FiveNights.Utils
{
    /// <summary>
    /// 프로토타입용 화면 왼쪽 위 디버그 표시 (IMGUI). 정식 HUD가 생기면 지운다.
    /// F1로 켜고 끈다.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private GameSession _session;
        [SerializeField] private PlayerMotor _player;
        [SerializeField] private bool _visible = true;

        private GuardController[] _guards;
        private GUIStyle _style;

        private void Start()
        {
            _guards = FindObjectsByType<GuardController>(FindObjectsSortMode.None);
        }

        private void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.f1Key.wasPressedThisFrame) _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible) return;
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true };
                _style.normal.textColor = Color.white;
            }

            var sb = new System.Text.StringBuilder();
            if (_session != null)
                sb.AppendLine($"목숨 {_session.Lives}/{_session.MaxLives}   잡힌 횟수 {_session.TimesCaught}");
            if (_player != null)
                sb.AppendLine($"플레이어 {_player.CurrentMode}   발소리 {_player.CurrentNoiseRadius:0.0}");
            if (_guards != null)
                foreach (var g in _guards)
                    sb.AppendLine($"{g.name}: {g.State}   의심 {g.Suspicion:0.00}");
            sb.Append("<size=12>WASD 이동 · Shift 달리기 · C 웅크리기 · F1 이 표시 끄기</size>");

            GUI.Box(new Rect(8, 8, 330, 96), GUIContent.none);
            GUI.Label(new Rect(16, 12, 320, 92), sb.ToString(), _style);
        }
    }
}

using System;
using FiveNights.Guards;
using FiveNights.Player;
using UnityEngine;

namespace FiveNights.Core
{
    /// <summary>
    /// 하룻밤 진행 상태 (프로토타입 버전): 목숨, 잡힘 처리, 시작 위치로 리셋.
    /// GDD 4장: 잡히면 목숨 -1, 그날 밤 시작 위치로 돌아가 재시도. 목숨을 다 잃으면 그날 밤 실패.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _player;
        [SerializeField, Min(1)] private int _maxLives = 3;
        [Tooltip("잡힌 직후 다시 잡히지 않는 시간(초)")]
        [SerializeField, Min(0f)] private float _respawnGrace = 1.5f;

        private Vector2 _playerStart;
        private GuardController[] _guards;
        private float _graceUntil;

        public int Lives { get; private set; }
        public int MaxLives => _maxLives;
        public int TimesCaught { get; private set; }
        public int NightsFailed { get; private set; }

        public event Action<int> LivesChanged;
        public event Action NightFailed;

        private void Awake()
        {
            Lives = _maxLives;
            if (_player != null) _playerStart = _player.transform.position;
            _guards = FindObjectsByType<GuardController>(FindObjectsSortMode.None);
        }

        private void OnEnable()
        {
            if (_guards == null) return;
            foreach (var g in _guards) g.Caught += OnCaught;
        }

        private void OnDisable()
        {
            if (_guards == null) return;
            foreach (var g in _guards) g.Caught -= OnCaught;
        }

        private void OnCaught(GuardController guard)
        {
            if (Time.time < _graceUntil) return;
            _graceUntil = Time.time + _respawnGrace;

            TimesCaught++;
            Lives--;
            Debug.Log($"[GameSession] 잡힘 ({guard.name}). 남은 목숨 {Lives}/{_maxLives}");

            if (Lives <= 0)
            {
                // 프로토타입: 밤 실패 처리 대신 목숨을 채우고 다시 시작한다. (경계도 상승은 2단계)
                NightsFailed++;
                Debug.Log("[GameSession] 목숨을 모두 잃음 → 이번 밤 실패 (프로토타입에서는 목숨을 채우고 재시작)");
                NightFailed?.Invoke();
                Lives = _maxLives;
            }
            LivesChanged?.Invoke(Lives);

            if (_player != null) _player.Teleport(_playerStart);
            foreach (var g in _guards) g.ResetGuard();
        }
    }
}

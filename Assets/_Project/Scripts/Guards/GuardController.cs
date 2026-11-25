using System;
using FiveNights.Core.Navigation;
using FiveNights.Perception;
using UnityEngine;

namespace FiveNights.Guards
{
    /// <summary>
    /// 경비 한 명을 묶는 컴포넌트: 감지(Perception) → 판단(IGuardBrain) → 행동(GuardMover) 순서로 매 프레임 실행한다.
    /// 두뇌는 교체 가능하다 (지금은 FsmGuardBrain).
    /// </summary>
    [RequireComponent(typeof(GuardMover))]
    public class GuardController : MonoBehaviour
    {
        [SerializeField] private GuardSettings _settings;
        [SerializeField] private PatrolRoute _route;
        [SerializeField] private NavGrid _nav;
        [SerializeField] private LayerMask _obstacles;
        [Tooltip("시야·청각을 재는 눈 높이 오프셋 (탑다운이라 보통 0)")]
        [SerializeField] private Vector2 _eyeOffset = Vector2.zero;
        [Tooltip("시작할 때 바라보는 방향")]
        [SerializeField] private Vector2 _initialFacing = Vector2.left;

        private GuardMover _mover;
        private IGuardBrain _brain;
        private VisionSensor _vision;
        private HearingSensor _hearing;
        private SuspicionMeter _suspicion;

        private Vector2 _spawnPosition;
        private Vector2 _spawnFacing;

        /// <summary>플레이어를 잡았을 때.</summary>
        public event Action<GuardController> Caught;

        public GuardSettings Settings => _settings;
        public GuardState State => _brain != null ? _brain.State : GuardState.Patrol;
        public float Suspicion => _suspicion != null ? _suspicion.Value : 0f;
        public Vector2 Facing => _mover != null ? _mover.Facing : Vector2.right;
        public GuardObservation LastObservation { get; private set; }
        public LayerMask Obstacles => _obstacles;

        private void Awake()
        {
            _mover = GetComponent<GuardMover>();
            if (_settings == null)
            {
                Debug.LogError("[GuardController] GuardSettings가 지정되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _brain = new FsmGuardBrain(_settings, GetInstanceID());
            _vision = new VisionSensor(_settings.viewDistance, _settings.viewAngle, _settings.proximityRadius, _obstacles);
            _hearing = new HearingSensor(_settings.wallDampening, _obstacles);
            _suspicion = new SuspicionMeter
            {
                FillTimeAtMaxRange = _settings.fillTimeAtMaxRange,
                CloseRangeMultiplier = _settings.closeRangeMultiplier,
                HearingRate = _settings.hearingRate,
                HearingCap = _settings.hearingCap,
                DecayRate = _settings.decayRate
            };
            _mover.Configure(_nav, _settings.turnSpeed, _settings.repathInterval);

            // 루트는 회전시키지 않는다 (방향은 Facing 값으로만 관리, 그래픽은 GuardVisual이 돌림).
            transform.rotation = Quaternion.identity;
            _spawnPosition = transform.position;
            _spawnFacing = _initialFacing.sqrMagnitude > 0.0001f ? _initialFacing.normalized : Vector2.left;
        }

        private void Start()
        {
            _mover.Teleport(_spawnPosition, _spawnFacing);
            _brain.ResetBrain(0);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Vector2 eye = _mover.Position + _eyeOffset;

            // 1) 감지
            var obs = new GuardObservation();
            var target = PerceptionTarget.Primary;
            if (target != null)
            {
                obs.SeesTarget = _vision.CanSee(eye, _mover.Facing, target.Position, out obs.SeenDistance01);
                if (obs.SeesTarget) obs.SeenPosition = target.Position;

                obs.HearsNoise = _hearing.CanHear(eye, target.Position, target.NoiseRadius);
                if (obs.HearsNoise) obs.NoisePosition = target.Position;

                obs.TouchingTarget = Vector2.Distance(eye, target.Position) <= _settings.catchDistance
                                     && _vision.HasLineOfSight(eye, target.Position);
            }
            _suspicion.Tick(obs.SeesTarget, obs.SeenDistance01, obs.HearsNoise, dt);
            obs.Suspicion = _suspicion.Value;
            LastObservation = obs;

            // 2) 판단
            var ctx = new GuardContext
            {
                Position = _mover.Position,
                Facing = _mover.Facing,
                PatrolPoints = _route != null ? _route.Points : null,
                Nav = _nav
            };
            GuardCommand cmd = _brain.Decide(obs, ctx, dt);

            // 3) 행동
            if (cmd.HasMoveTarget) _mover.SetDestination(cmd.MoveTarget, cmd.MoveSpeed);
            else _mover.Stop();
            _mover.SetLook(cmd.HasLookTarget, cmd.LookTarget, cmd.ScanSpeed);

            if (cmd.Catch) Caught?.Invoke(this);
        }

        /// <summary>잡힘 처리 후 처음 위치·상태로 되돌린다.</summary>
        public void ResetGuard()
        {
            _suspicion.Reset();
            _brain.ResetBrain(0);
            _mover.Teleport(_spawnPosition, _spawnFacing);
        }

        private void OnDrawGizmosSelected()
        {
            if (_settings == null) return;
            Vector2 f = Application.isPlaying && _mover != null ? _mover.Facing : _initialFacing.normalized;
            Vector3 p = transform.position;
            float half = _settings.viewAngle * 0.5f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(p, p + (Vector3)(Quaternion.Euler(0, 0, half) * f * _settings.viewDistance));
            Gizmos.DrawLine(p, p + (Vector3)(Quaternion.Euler(0, 0, -half) * f * _settings.viewDistance));
            Gizmos.DrawWireSphere(p, _settings.proximityRadius);
        }
    }
}

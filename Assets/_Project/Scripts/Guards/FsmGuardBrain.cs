using FiveNights.Perception;
using UnityEngine;

namespace FiveNights.Guards
{
    /// <summary>
    /// 규칙 기반 경비 두뇌 (유한 상태 기계).
    /// 순찰 → (보거나 들음) 의심 → (게이지 가득) 추격 → (놓침) 수색 → 복귀 → 순찰.
    /// 순수 C# 클래스라 Unity 오브젝트 없이 테스트할 수 있다.
    /// </summary>
    public class FsmGuardBrain : IGuardBrain
    {
        private readonly GuardSettings _s;
        private readonly System.Random _rng;

        private int _waypoint;
        private float _waitTimer;

        private Vector2 _lastKnown;      // 추격 중 마지막으로 본 위치
        private Vector2 _investigatePoint;
        private float _investigateLookTimer;

        private Vector2 _searchCenter;
        private Vector2 _searchPoint;
        private bool _hasSearchPoint;
        private float _searchTimer;

        public GuardState State { get; private set; } = GuardState.Patrol;

        public FsmGuardBrain(GuardSettings settings, int seed = 0)
        {
            _s = settings;
            _rng = new System.Random(seed);
        }

        public void ResetBrain(int startWaypoint)
        {
            State = GuardState.Patrol;
            _waypoint = startWaypoint;
            _waitTimer = 0f;
            _hasSearchPoint = false;
        }

        public GuardCommand Decide(in GuardObservation obs, in GuardContext ctx, float dt)
        {
            bool stimulus = obs.SeesTarget || obs.HearsNoise;
            Vector2 stimulusPos = obs.SeesTarget ? obs.SeenPosition : obs.NoisePosition;

            // 공통 전이: 눈으로 보고 게이지가 가득 차면 어떤 상태든 추격.
            if (obs.SeesTarget && obs.Suspicion >= 1f)
            {
                State = GuardState.Chase;
                _lastKnown = obs.SeenPosition;
            }

            switch (State)
            {
                case GuardState.Patrol:
                case GuardState.Return:
                    if (stimulus && obs.Suspicion > 0f)
                    {
                        EnterInvestigate(stimulusPos);
                        return Investigate(obs, ctx, dt);
                    }
                    return State == GuardState.Patrol ? Patrol(ctx, dt) : Return(ctx);

                case GuardState.Investigate:
                    return Investigate(obs, ctx, dt);

                case GuardState.Chase:
                    return Chase(obs, ctx);

                case GuardState.Search:
                    if (stimulus && obs.Suspicion > 0f)
                    {
                        EnterInvestigate(stimulusPos);
                        return Investigate(obs, ctx, dt);
                    }
                    return Search(ctx, dt);
            }

            return default;
        }

        // ── 순찰 ─────────────────────────────────────────────
        private GuardCommand Patrol(in GuardContext ctx, float dt)
        {
            var cmd = new GuardCommand { State = GuardState.Patrol };
            if (ctx.PatrolPoints == null || ctx.PatrolPoints.Count == 0) return cmd;

            _waypoint %= ctx.PatrolPoints.Count;
            Vector2 target = ctx.PatrolPoints[_waypoint];

            if (Arrived(ctx.Position, target))
            {
                _waitTimer += dt;
                cmd.ScanSpeed = 60f; // 지점에서 잠깐 둘러본다
                if (_waitTimer >= _s.waypointWait)
                {
                    _waitTimer = 0f;
                    _waypoint = (_waypoint + 1) % ctx.PatrolPoints.Count;
                }
                return cmd;
            }

            return MoveTo(cmd, target, _s.patrolSpeed);
        }

        // ── 의심 (소리나 형체를 확인하러 감) ─────────────────────
        private void EnterInvestigate(Vector2 point)
        {
            State = GuardState.Investigate;
            _investigatePoint = point;
            _investigateLookTimer = 0f;
        }

        private GuardCommand Investigate(in GuardObservation obs, in GuardContext ctx, float dt)
        {
            var cmd = new GuardCommand { State = GuardState.Investigate };

            if (obs.SeesTarget)
            {
                // 뭔가 보인다: 멈춰서 그쪽을 주시한다 (게이지가 차면 위에서 추격으로 넘어감).
                _investigatePoint = obs.SeenPosition;
                _investigateLookTimer = 0f;
                cmd.HasLookTarget = true;
                cmd.LookTarget = obs.SeenPosition;
                return cmd;
            }

            if (obs.HearsNoise)
            {
                _investigatePoint = obs.NoisePosition;
                _investigateLookTimer = 0f;
            }

            if (obs.Suspicion <= 0f)
            {
                State = GuardState.Return;
                return Return(ctx);
            }

            if (Arrived(ctx.Position, Snap(ctx, _investigatePoint)))
            {
                _investigateLookTimer += dt;
                cmd.ScanSpeed = 120f;
                if (_investigateLookTimer >= _s.investigateLookTime)
                {
                    State = GuardState.Return;
                    return Return(ctx);
                }
                return cmd;
            }

            cmd = MoveTo(cmd, _investigatePoint, _s.investigateSpeed);
            cmd.HasLookTarget = true;
            cmd.LookTarget = _investigatePoint;
            return cmd;
        }

        // ── 추격 ─────────────────────────────────────────────
        private GuardCommand Chase(in GuardObservation obs, in GuardContext ctx)
        {
            var cmd = new GuardCommand { State = GuardState.Chase };

            if (obs.SeesTarget)
            {
                _lastKnown = obs.SeenPosition;
                if (obs.TouchingTarget)
                {
                    cmd.Catch = true;
                    return cmd;
                }
                cmd = MoveTo(cmd, _lastKnown, _s.chaseSpeed);
                cmd.HasLookTarget = true;
                cmd.LookTarget = _lastKnown;
                return cmd;
            }

            // 놓쳤다: 마지막으로 본 위치까지 가 본 뒤 수색.
            if (Arrived(ctx.Position, Snap(ctx, _lastKnown)))
            {
                EnterSearch(_lastKnown);
                return Search(ctx, 0f);
            }
            return MoveTo(cmd, _lastKnown, _s.chaseSpeed);
        }

        // ── 수색 ─────────────────────────────────────────────
        private void EnterSearch(Vector2 center)
        {
            State = GuardState.Search;
            _searchCenter = center;
            _searchTimer = 0f;
            _hasSearchPoint = false;
        }

        private GuardCommand Search(in GuardContext ctx, float dt)
        {
            var cmd = new GuardCommand { State = GuardState.Search };
            _searchTimer += dt;
            if (_searchTimer >= _s.searchDuration)
            {
                State = GuardState.Return;
                return Return(ctx);
            }

            if (!_hasSearchPoint || Arrived(ctx.Position, _searchPoint))
            {
                _searchPoint = ctx.Nav != null
                    ? ctx.Nav.RandomWalkableNear(_searchCenter, _s.searchRadius, _rng)
                    : _searchCenter;
                _hasSearchPoint = true;
            }
            return MoveTo(cmd, _searchPoint, _s.searchSpeed);
        }

        // ── 복귀 ─────────────────────────────────────────────
        private GuardCommand Return(in GuardContext ctx)
        {
            var cmd = new GuardCommand { State = GuardState.Return };
            if (ctx.PatrolPoints == null || ctx.PatrolPoints.Count == 0)
            {
                State = GuardState.Patrol;
                return cmd;
            }

            // 가장 가까운 순찰 지점으로 돌아가 거기서부터 순찰을 이어간다.
            int nearest = 0;
            float best = float.MaxValue;
            for (int i = 0; i < ctx.PatrolPoints.Count; i++)
            {
                float d = (ctx.PatrolPoints[i] - ctx.Position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    nearest = i;
                }
            }
            _waypoint = nearest;

            if (Arrived(ctx.Position, ctx.PatrolPoints[nearest]))
            {
                State = GuardState.Patrol;
                _waitTimer = 0f;
                return cmd;
            }
            return MoveTo(cmd, ctx.PatrolPoints[nearest], _s.returnSpeed);
        }

        // ── 공통 ─────────────────────────────────────────────
        private GuardCommand MoveTo(GuardCommand cmd, Vector2 target, float speed)
        {
            cmd.HasMoveTarget = true;
            cmd.MoveTarget = target;
            cmd.MoveSpeed = speed;
            return cmd;
        }

        /// <summary>벽에 붙은 지점은 걸어갈 수 있는 가장 가까운 지점으로 바꾼다 (이동 쪽과 같은 기준).</summary>
        private static Vector2 Snap(in GuardContext ctx, Vector2 p)
        {
            if (ctx.Nav == null || ctx.Nav.IsWalkable(p)) return p;
            return ctx.Nav.ClosestWalkable(p);
        }

        private bool Arrived(Vector2 position, Vector2 target)
        {
            return (target - position).sqrMagnitude <= _s.arriveDistance * _s.arriveDistance;
        }
    }
}

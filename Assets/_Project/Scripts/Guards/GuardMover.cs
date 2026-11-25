using System.Collections.Generic;
using FiveNights.Core.Navigation;
using UnityEngine;

namespace FiveNights.Guards
{
    /// <summary>
    /// 경비 이동 실행부. 목적지를 받으면 길찾기 경로를 따라 움직이고, 바라보는 방향을 천천히 돌린다.
    /// 판단은 하지 않는다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class GuardMover : MonoBehaviour
    {
        private Rigidbody2D _body;
        private INavQuery _nav;
        private readonly List<Vector2> _path = new List<Vector2>();
        private int _pathIndex;

        private bool _hasDestination;
        private Vector2 _destination;
        private float _speed;
        private float _repathTimer;
        private float _repathInterval = 0.25f;

        private bool _hasLookTarget;
        private Vector2 _lookTarget;
        private float _scanSpeed;
        private float _turnSpeed = 300f;

        public Vector2 Facing { get; private set; } = Vector2.right;
        public Vector2 Position => _body != null ? _body.position : (Vector2)transform.position;
        public bool IsMoving { get; private set; }
        public IReadOnlyList<Vector2> CurrentPath => _path;
        public int CurrentPathIndex => _pathIndex;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void Configure(INavQuery nav, float turnSpeed, float repathInterval)
        {
            _nav = nav;
            _turnSpeed = turnSpeed;
            _repathInterval = repathInterval;
        }

        public void SetDestination(Vector2 destination, float speed)
        {
            bool changed = !_hasDestination || (destination - _destination).sqrMagnitude > 0.25f;
            _destination = destination;
            _speed = speed;
            _hasDestination = true;
            if (changed) Repath();
        }

        public void Stop()
        {
            _hasDestination = false;
            _path.Clear();
            _pathIndex = 0;
        }

        public void SetLook(bool hasTarget, Vector2 target, float scanSpeed)
        {
            _hasLookTarget = hasTarget;
            _lookTarget = target;
            _scanSpeed = scanSpeed;
        }

        public void Teleport(Vector2 position, Vector2 facing)
        {
            Stop();
            _body.position = position;
            transform.position = position;
            if (facing.sqrMagnitude > 0.0001f) Facing = facing.normalized;
        }

        private void Repath()
        {
            _repathTimer = 0f;
            _pathIndex = 0;
            if (_nav == null || !_nav.FindPath(_body.position, _destination, _path))
            {
                _path.Clear();
                _path.Add(_destination);
            }
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            Vector2 pos = _body.position;
            Vector2 moveDir = Vector2.zero;
            IsMoving = false;

            if (_hasDestination)
            {
                _repathTimer += dt;
                if (_repathTimer >= _repathInterval) Repath();

                float step = _speed * dt;
                while (_pathIndex < _path.Count && step > 0f)
                {
                    Vector2 next = _path[_pathIndex];
                    Vector2 to = next - pos;
                    float dist = to.magnitude;
                    if (dist <= step)
                    {
                        pos = next;
                        step -= dist;
                        _pathIndex++;
                        if (dist > 0.0001f) moveDir = to / dist;
                    }
                    else
                    {
                        moveDir = to / dist;
                        pos += moveDir * step;
                        step = 0f;
                    }
                }

                IsMoving = moveDir.sqrMagnitude > 0f;
                _body.MovePosition(pos);
            }

            // 바라보는 방향: 지정 지점 > 이동 방향 > 제자리 둘러보기
            Vector2 desired = Facing;
            if (_hasLookTarget && (_lookTarget - pos).sqrMagnitude > 0.01f) desired = (_lookTarget - pos).normalized;
            else if (IsMoving) desired = moveDir;

            if (!_hasLookTarget && !IsMoving && _scanSpeed > 0f)
            {
                Facing = Rotate(Facing, Mathf.Sin(Time.time * 1.3f) * _scanSpeed * dt);
            }
            else
            {
                float current = Mathf.Atan2(Facing.y, Facing.x) * Mathf.Rad2Deg;
                float target = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;
                float next = Mathf.MoveTowardsAngle(current, target, _turnSpeed * dt);
                Facing = new Vector2(Mathf.Cos(next * Mathf.Deg2Rad), Mathf.Sin(next * Mathf.Deg2Rad));
            }
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        private void OnDrawGizmos()
        {
            if (_path.Count == 0) return;
            Gizmos.color = new Color(1f, 0.85f, 0.3f, 0.8f);
            Vector3 prev = transform.position;
            for (int i = _pathIndex; i < _path.Count; i++)
            {
                Gizmos.DrawLine(prev, _path[i]);
                prev = _path[i];
            }
        }
    }
}

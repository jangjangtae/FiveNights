using System.Collections.Generic;
using UnityEngine;

namespace FiveNights.Core.Navigation
{
    /// <summary>
    /// 장애물 레이어의 콜라이더로 걸을 수 있는 칸을 구운 2D 격자. 경로는 A*로 찾고 직선으로 펴서 돌려준다.
    /// 이 오브젝트의 위치가 격자 중심이다.
    /// </summary>
    public class NavGrid : MonoBehaviour, INavQuery
    {
        [SerializeField] private Vector2 _size = new Vector2(24f, 14f);
        [SerializeField, Min(0.1f)] private float _cellSize = 0.5f;
        [SerializeField, Min(0f)] private float _agentRadius = 0.35f;
        [SerializeField] private LayerMask _obstacles;
        [SerializeField] private bool _drawGizmos = true;

        private bool[] _walkable;
        private int _width;
        private int _height;
        private Vector2 _origin;
        private GridPathfinder _pathfinder;
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();

        public float CellSize => _cellSize;

        private void Awake()
        {
            Build();
        }

        /// <summary>장애물 배치가 바뀌면 다시 호출한다 (예: 통로 잠금).</summary>
        public void Build()
        {
            Physics2D.SyncTransforms();
            _width = Mathf.Max(1, Mathf.CeilToInt(_size.x / _cellSize));
            _height = Mathf.Max(1, Mathf.CeilToInt(_size.y / _cellSize));
            _origin = (Vector2)transform.position - new Vector2(_width, _height) * _cellSize * 0.5f;
            _walkable = new bool[_width * _height];

            Vector2 box = Vector2.one * (_cellSize * 0.9f + _agentRadius * 2f);
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                _walkable[y * _width + x] = Physics2D.OverlapBox(CellCenter(x, y), box, 0f, _obstacles) == null;

            _pathfinder = new GridPathfinder(_width, _height, (x, y) => _walkable[y * _width + x]);
        }

        public Vector2 CellCenter(int x, int y) => _origin + new Vector2((x + 0.5f) * _cellSize, (y + 0.5f) * _cellSize);

        public Vector2Int WorldToCell(Vector2 p)
        {
            Vector2 local = (p - _origin) / _cellSize;
            return new Vector2Int(Mathf.Clamp(Mathf.FloorToInt(local.x), 0, _width - 1), Mathf.Clamp(Mathf.FloorToInt(local.y), 0, _height - 1));
        }

        private bool CellWalkable(Vector2Int c) =>
            c.x >= 0 && c.y >= 0 && c.x < _width && c.y < _height && _walkable[c.y * _width + c.x];

        public bool IsWalkable(Vector2 point)
        {
            if (_walkable == null) Build();
            return CellWalkable(WorldToCell(point));
        }

        public Vector2 ClosestWalkable(Vector2 point)
        {
            Vector2Int c = ClosestWalkableCell(point);
            return CellCenter(c.x, c.y);
        }

        private Vector2Int ClosestWalkableCell(Vector2 point)
        {
            if (_walkable == null) Build();
            Vector2Int c = WorldToCell(point);
            if (CellWalkable(c)) return c;

            // 바깥으로 한 겹씩 넓혀 가며 가장 가까운 칸을 찾는다.
            for (int r = 1; r < Mathf.Max(_width, _height); r++)
            {
                float best = float.MaxValue;
                Vector2Int bestCell = c;
                bool found = false;
                for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
                    var n = new Vector2Int(c.x + dx, c.y + dy);
                    if (!CellWalkable(n)) continue;
                    float d = (CellCenter(n.x, n.y) - point).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        bestCell = n;
                        found = true;
                    }
                }
                if (found) return bestCell;
            }
            return c;
        }

        public bool FindPath(Vector2 from, Vector2 to, List<Vector2> result)
        {
            result.Clear();
            if (_walkable == null) Build();

            Vector2Int start = ClosestWalkableCell(from);
            Vector2Int goal = ClosestWalkableCell(to);
            if (!_pathfinder.FindPath(start, goal, _cells)) return false;

            var raw = new List<Vector2>(_cells.Count + 1);
            for (int i = 1; i < _cells.Count; i++) raw.Add(CellCenter(_cells[i].x, _cells[i].y));
            Vector2 end = IsWalkable(to) ? to : CellCenter(goal.x, goal.y);
            if (raw.Count > 0) raw[raw.Count - 1] = end;
            else raw.Add(end);

            // 경로 펴기: 지금 위치에서 곧장 보이는 가장 먼 점으로 건너뛴다.
            Vector2 anchor = from;
            int idx = 0;
            while (idx < raw.Count)
            {
                int far = idx;
                for (int j = raw.Count - 1; j > idx; j--)
                {
                    if (Clear(anchor, raw[j]))
                    {
                        far = j;
                        break;
                    }
                }
                result.Add(raw[far]);
                anchor = raw[far];
                idx = far + 1;
            }
            return true;
        }

        private bool Clear(Vector2 a, Vector2 b)
        {
            Vector2 d = b - a;
            float len = d.magnitude;
            if (len < 0.001f) return true;
            return Physics2D.CircleCast(a, _agentRadius * 0.95f, d / len, len, _obstacles).collider == null;
        }

        public Vector2 RandomWalkableNear(Vector2 center, float radius, System.Random rng)
        {
            for (int i = 0; i < 24; i++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                float r = Mathf.Sqrt((float)rng.NextDouble()) * radius;
                Vector2 p = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
                if (IsWalkable(p)) return p;
            }
            return ClosestWalkable(center);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos) return;
            Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
            Gizmos.DrawWireCube(transform.position, _size);
            if (_walkable == null) return;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (!_walkable[y * _width + x])
                    Gizmos.DrawCube(CellCenter(x, y), Vector3.one * _cellSize * 0.9f);
        }
    }
}

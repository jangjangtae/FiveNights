using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveNights.Core.Navigation
{
    /// <summary>
    /// 격자 A* (8방향, 벽 모서리 가로지르기 금지, 옥타일 휴리스틱). Unity 씬과 무관한 순수 로직.
    /// 프로토타입 규모(수천 칸)에서는 단순한 선형 open 목록으로 충분하다.
    /// </summary>
    public class GridPathfinder
    {
        private const float Straight = 1f;
        private const float Diagonal = 1.41421356f;

        private static readonly Vector2Int[] Dirs =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        private readonly int _width;
        private readonly int _height;
        private readonly Func<int, int, bool> _walkable;

        private readonly float[] _g;
        private readonly float[] _f;
        private readonly int[] _parent;
        private readonly byte[] _state; // 0 = 미방문, 1 = open, 2 = closed
        private readonly List<int> _open = new List<int>();

        public GridPathfinder(int width, int height, Func<int, int, bool> walkable)
        {
            _width = width;
            _height = height;
            _walkable = walkable;
            int n = width * height;
            _g = new float[n];
            _f = new float[n];
            _parent = new int[n];
            _state = new byte[n];
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;
        private bool Walkable(int x, int y) => InBounds(x, y) && _walkable(x, y);

        public bool FindPath(Vector2Int start, Vector2Int goal, List<Vector2Int> result)
        {
            result.Clear();
            if (!Walkable(start.x, start.y) || !Walkable(goal.x, goal.y)) return false;
            if (start == goal)
            {
                result.Add(goal);
                return true;
            }

            Array.Clear(_state, 0, _state.Length);
            _open.Clear();

            int s = Index(start.x, start.y);
            int gIdx = Index(goal.x, goal.y);
            _g[s] = 0f;
            _f[s] = Heuristic(start, goal);
            _parent[s] = -1;
            _state[s] = 1;
            _open.Add(s);

            while (_open.Count > 0)
            {
                int bestI = 0;
                for (int i = 1; i < _open.Count; i++)
                    if (_f[_open[i]] < _f[_open[bestI]]) bestI = i;

                int current = _open[bestI];
                _open[bestI] = _open[_open.Count - 1];
                _open.RemoveAt(_open.Count - 1);

                if (current == gIdx)
                {
                    Reconstruct(current, result);
                    return true;
                }

                _state[current] = 2;
                int cx = current % _width;
                int cy = current / _width;

                for (int d = 0; d < Dirs.Length; d++)
                {
                    int nx = cx + Dirs[d].x;
                    int ny = cy + Dirs[d].y;
                    if (!Walkable(nx, ny)) continue;

                    bool diagonal = d >= 4;
                    // 대각선으로 벽 모서리를 스쳐 지나가지 않는다.
                    if (diagonal && (!Walkable(cx + Dirs[d].x, cy) || !Walkable(cx, cy + Dirs[d].y))) continue;

                    int n = Index(nx, ny);
                    if (_state[n] == 2) continue;

                    float tentative = _g[current] + (diagonal ? Diagonal : Straight);
                    if (_state[n] == 1 && tentative >= _g[n]) continue;

                    _g[n] = tentative;
                    _f[n] = tentative + Heuristic(new Vector2Int(nx, ny), goal);
                    _parent[n] = current;
                    if (_state[n] != 1)
                    {
                        _state[n] = 1;
                        _open.Add(n);
                    }
                }
            }

            return false;
        }

        private int Index(int x, int y) => y * _width + x;

        private void Reconstruct(int node, List<Vector2Int> result)
        {
            while (node != -1)
            {
                result.Add(new Vector2Int(node % _width, node / _width));
                node = _parent[node];
            }
            result.Reverse();
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return Straight * (dx + dy) + (Diagonal - 2f * Straight) * Mathf.Min(dx, dy);
        }
    }
}

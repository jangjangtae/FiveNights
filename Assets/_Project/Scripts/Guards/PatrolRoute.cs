using System.Collections.Generic;
using UnityEngine;

namespace FiveNights.Guards
{
    /// <summary>순찰 경로. 자식 Transform들이 순서대로 순찰 지점이 된다 (마지막 → 처음으로 반복).</summary>
    public class PatrolRoute : MonoBehaviour
    {
        private readonly List<Vector2> _points = new List<Vector2>();

        public IReadOnlyList<Vector2> Points
        {
            get
            {
                _points.Clear();
                foreach (Transform child in transform) _points.Add(child.position);
                return _points;
            }
        }

        private void OnDrawGizmos()
        {
            int n = transform.childCount;
            if (n == 0) return;
            Gizmos.color = new Color(1f, 0.85f, 0.3f, 0.6f);
            for (int i = 0; i < n; i++)
            {
                Vector3 a = transform.GetChild(i).position;
                Vector3 b = transform.GetChild((i + 1) % n).position;
                Gizmos.DrawWireSphere(a, 0.2f);
                Gizmos.DrawLine(a, b);
            }
        }
    }
}

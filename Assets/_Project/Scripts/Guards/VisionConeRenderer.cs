using UnityEngine;

namespace FiveNights.Guards
{
    /// <summary>
    /// 경비 시야 부채꼴을 메쉬로 그린다. 레이캐스트로 벽에 잘린 실제 보이는 영역만 칠한다.
    /// 시야 범위가 항상 읽히게 하는 것이 목적 (GDD 7장).
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VisionConeRenderer : MonoBehaviour
    {
        [SerializeField] private GuardController _guard;
        [SerializeField, Range(8, 96)] private int _rays = 40;
        [SerializeField] private Color _calmColor = new Color(1f, 0.9f, 0.45f, 0.10f);
        [SerializeField] private Color _alertColor = new Color(1f, 0.2f, 0.15f, 0.18f);
        [SerializeField] private int _sortingOrder = 5;

        private Mesh _mesh;
        private Vector3[] _vertices;
        private Color[] _colors;
        private int[] _triangles;

        private void Awake()
        {
            _mesh = new Mesh { name = "VisionCone" };
            _mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
            GetComponent<MeshRenderer>().sortingOrder = _sortingOrder;
            Allocate();
        }

        private void Allocate()
        {
            _vertices = new Vector3[_rays + 2];
            _colors = new Color[_rays + 2];
            _triangles = new int[_rays * 3];
            for (int i = 0; i < _rays; i++)
            {
                _triangles[i * 3] = 0;
                _triangles[i * 3 + 1] = i + 2;
                _triangles[i * 3 + 2] = i + 1;
            }
        }

        private void LateUpdate()
        {
            if (_guard == null || _guard.Settings == null) return;
            if (_vertices == null || _vertices.Length != _rays + 2) Allocate();

            // 메쉬는 월드 기준으로 계산하고, 이 오브젝트는 회전 없이 경비를 따라간다.
            transform.rotation = Quaternion.identity;
            var s = _guard.Settings;
            Vector2 origin = transform.position;
            Vector2 f = _guard.Facing;
            float baseAngle = Mathf.Atan2(f.y, f.x) * Mathf.Rad2Deg;
            float half = s.viewAngle * 0.5f;
            Color c = _guard.State == GuardState.Chase ? _alertColor : Color.Lerp(_calmColor, _alertColor, _guard.Suspicion * 0.6f);
            Color edge = new Color(c.r, c.g, c.b, 0f);

            _vertices[0] = Vector3.zero;
            _colors[0] = c;
            for (int i = 0; i <= _rays; i++)
            {
                float a = (baseAngle - half + s.viewAngle * i / _rays) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                float dist = s.viewDistance;
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, s.viewDistance, _guard.Obstacles);
                if (hit.collider != null) dist = hit.distance;
                _vertices[i + 1] = dir * dist;
                _colors[i + 1] = Color.Lerp(c, edge, dist / s.viewDistance * 0.7f);
            }

            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _mesh.triangles = _triangles;
            _mesh.RecalculateBounds();
        }
    }
}

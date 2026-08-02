using Match3.Board.Data;
using Match3.Scene;
using UnityEngine;

namespace Match3.Board.View
{
    public sealed class BoardAreaAuthoring : MonoBehaviour
    {
#if UNITY_EDITOR
        private static readonly Color AreaGizmoColor = new(1f, 0.85f, 0.2f, 1f);
        private static readonly Color EffectiveGizmoColor = new(0.3f, 1f, 0.4f, 1f);
        private static readonly Color GridGizmoColor = new(0.3f, 1f, 0.4f, 0.35f);
#endif

        [SerializeField] private MainCameraSceneProvider _cameraProvider;
        [Tooltip("Родитель для вьюх блоков")]
        [SerializeField] private Transform _gridRoot;

        [Header("Область под поле")]
        [Tooltip("Высота области в мировых единицах. Центр по вертикали — позиция этого объекта")]
        [SerializeField] private float _height = 10f;
        [Tooltip("Отступ слева и справа в долях видимой ширины — так поле одинаково на любом портрете")]
        [Range(0f, 0.45f)]
        [SerializeField] private float _horizontalMargin = 0.05f;

        [Header("Потолки")]
        [Tooltip("Максимальный размер поля в клетках. Под него один раз выделяются все массивы и пул блоков")]
        [SerializeField] private Vector2Int _maxBoardSize = new(10, 12);
        [Tooltip("Максимальный размер одной клетки в мировых единицах — держит мелкие поля от растягивания")]
        [SerializeField] private float _maxCellSize = 1.2f;

        public Transform GridRoot => _gridRoot;
        public float MaxCellSize => _maxCellSize;

        public BoardCapacity Capacity => new(_maxBoardSize.x, _maxBoardSize.y);

        public Rect WorldArea
        {
            get
            {
                var view = CameraRect;

                var size = new Vector2(view.width * (1f - _horizontalMargin * 2f), _height);
                var center = new Vector2(view.center.x, transform.position.y);

                return new Rect(center - size * 0.5f, size);
            }
        }

        public Rect EffectiveWorldArea
        {
            get
            {
                var area = WorldArea;
                var view = CameraRect;

                var xMin = Mathf.Max(area.xMin, view.xMin);
                var xMax = Mathf.Min(area.xMax, view.xMax);
                var yMin = Mathf.Max(area.yMin, view.yMin);
                var yMax = Mathf.Min(area.yMax, view.yMax);

                if (xMax <= xMin || yMax <= yMin)
                    return new Rect(area.center, Vector2.zero);

                return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            }
        }

        private Rect CameraRect
        {
            get
            {
                var camera = _cameraProvider.Camera;

                var height = camera.orthographicSize * 2f;
                var size = new Vector2(height * camera.aspect, height);

                return new Rect((Vector2)camera.transform.position - size * 0.5f, size);
            }
        }

#if UNITY_EDITOR
        private float CellSizeFor(int columns, int rows)
        {
            if (columns <= 0 || rows <= 0)
                return _maxCellSize;

            var area = EffectiveWorldArea;
            return Mathf.Min(_maxCellSize, area.width / columns, area.height / rows);
        }

        private void OnDrawGizmos()
        {
            if (_cameraProvider == null || _cameraProvider.Camera == null)
                return;

            DrawRect(WorldArea, AreaGizmoColor);

            var effective = EffectiveWorldArea;
            if (effective != WorldArea)
                DrawRect(effective, EffectiveGizmoColor);
        }

        private void OnDrawGizmosSelected()
        {
            if (_cameraProvider == null || _cameraProvider.Camera == null)
                return;

            var columns = _maxBoardSize.x;
            var rows = _maxBoardSize.y;
            if (columns <= 0 || rows <= 0)
                return;

            var cellSize = CellSizeFor(columns, rows);
            var area = EffectiveWorldArea;
            var origin = new Vector2(area.center.x - columns * cellSize * 0.5f, area.yMin);

            Gizmos.color = GridGizmoColor;

            for (var x = 0; x <= columns; x++)
            {
                var px = origin.x + x * cellSize;
                Gizmos.DrawLine(new Vector3(px, origin.y), new Vector3(px, origin.y + rows * cellSize));
            }

            for (var y = 0; y <= rows; y++)
            {
                var py = origin.y + y * cellSize;
                Gizmos.DrawLine(new Vector3(origin.x, py), new Vector3(origin.x + columns * cellSize, py));
            }
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Gizmos.color = color;

            var bottomLeft = new Vector3(rect.xMin, rect.yMin);
            var bottomRight = new Vector3(rect.xMax, rect.yMin);
            var topRight = new Vector3(rect.xMax, rect.yMax);
            var topLeft = new Vector3(rect.xMin, rect.yMax);

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
#endif
    }
}

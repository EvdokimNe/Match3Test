using UnityEngine;

namespace Match3.Board.View
{
    public sealed class BoardLayout
    {
        private const int DepthStep = 8;

        private Camera _camera;
        private int _columns;
        private int _rows;
        private Vector2 _origin;

        public float CellSize { get; private set; } = 1f;
        public Vector2 Origin => _origin;
        public int Columns => _columns;
        public int Rows => _rows;
        public int DestroyLift => (_columns + _rows) * DepthStep;

        public void Configure(Camera camera, Rect worldArea, float maxCellSize, int columns, int rows)
        {
            _camera = camera;
            _columns = columns;
            _rows = rows;

            if (columns <= 0 || rows <= 0)
                return;

            CellSize = Mathf.Min(maxCellSize, worldArea.width / columns, worldArea.height / rows);
            _origin = new Vector2(worldArea.center.x - columns * CellSize * 0.5f, worldArea.yMin);
        }

        public Vector3 WorldPos(Vector2Int cell) => WorldPos(cell.x, cell.y);

        public Vector3 WorldPos(float x, float y) => new(
            _origin.x + (x + 0.5f) * CellSize,
            _origin.y + (y + 0.5f) * CellSize,
            0f);

        public int SortingOrder(float x, float y) => Mathf.RoundToInt((x + y) * DepthStep);

        public bool TryScreenToCell(Vector2 screenPosition, out Vector2Int cell)
        {
            cell = default;

            if (_camera == null || _columns <= 0 || _rows <= 0)
                return false;

            var world = _camera.ScreenToWorldPoint(screenPosition);
            var local = (new Vector2(world.x, world.y) - _origin) / CellSize;

            var x = Mathf.FloorToInt(local.x);
            var y = Mathf.FloorToInt(local.y);

            if (x < 0 || x >= _columns || y < 0 || y >= _rows)
                return false;

            cell = new Vector2Int(x, y);
            return true;
        }
    }
}

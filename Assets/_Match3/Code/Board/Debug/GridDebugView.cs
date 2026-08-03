using System.Collections.Generic;
using Match3.Board.Data;
using Match3.Board.Flow;
using Match3.Board.Input;
using Match3.Board.Operations;
using Match3.Board.View;
using Match3.Scene;
using Match3.Session;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Match3.Board.Debugging
{
    public sealed class GridDebugView : MonoBehaviour
    {
#if UNITY_EDITOR
        private static readonly Color OccupiedColor = new(0.6f, 0.6f, 0.6f, 0.9f);
        private static readonly Color BlockedSideColor = new(1f, 0.55f, 0.15f, 1f);
        private static readonly Color DirtyColor = new(1f, 0.95f, 0.25f, 1f);

        private struct DirtyMark
        {
            public Vector2Int Cell;
            public float ShownUntil;
        }

        [Tooltip("Оверлей поля: рамки клеток, закрытые стороны, операции, порядок отрисовки")]
        [SerializeField] private Key _overlayKey = Key.F1;
        [Tooltip("Панель целиком. Выключенная не рисует ничего и не стоит ни одного дроукола")]
        [SerializeField] private Key _panelKey = Key.F2;
        [SerializeField] private bool _panelVisible;
        [SerializeField] private bool _overlayVisible;
        [SerializeField] private float _dirtyMarkSeconds = 0.6f;

        private readonly List<DirtyMark> _dirtyMarks = new(32);

        private BoardState _board;
        private OperationsLayer _operations;
        private BoardLayout _layout;
        private BoardSimulation _simulation;
        private InputRouter _input;
        private SessionController _session;
        private LevelProgress _progress;
        private MainCameraSceneProvider _cameraProvider;
        private GridView _view;

        private Texture2D _pixel;
        private GUIStyle _labelStyle;
        private GUIStyle _orderStyle;

        [Inject]
        public void Construct(BoardState board, OperationsLayer operations, BoardLayout layout,
            BoardSimulation simulation, InputRouter input, SessionController session, LevelProgress progress,
            MainCameraSceneProvider cameraProvider, GridView view)
        {
            _board = board;
            _operations = operations;
            _layout = layout;
            _simulation = simulation;
            _input = input;
            _session = session;
            _progress = progress;
            _cameraProvider = cameraProvider;
            _view = view;

            _board.DirtyMarked += OnDirtyMarked;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard[_panelKey].wasPressedThisFrame)
                    _panelVisible = !_panelVisible;

                if (keyboard[_overlayKey].wasPressedThisFrame)
                    _overlayVisible = !_overlayVisible;
            }

            PruneDirtyMarks();
        }

        private void OnDestroy()
        {
            if (_board != null)
                _board.DirtyMarked -= OnDirtyMarked;

            if (_pixel != null)
                Destroy(_pixel);
        }

        private void OnDirtyMarked(Vector2Int cell) =>
            _dirtyMarks.Add(new DirtyMark { Cell = cell, ShownUntil = Time.time + _dirtyMarkSeconds });

        private void PruneDirtyMarks()
        {
            for (var i = _dirtyMarks.Count - 1; i >= 0; i--)
            {
                if (_dirtyMarks[i].ShownUntil <= Time.time)
                    _dirtyMarks.RemoveAt(i);
            }
        }

        private void OnGUI()
        {
            if (!_panelVisible || _board == null)
                return;

            EnsureResources();
            DrawHud();

            if (_overlayVisible)
                DrawOverlay();
        }

        private void EnsureResources()
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                _pixel.SetPixel(0, 0, Color.white);
                _pixel.Apply();
            }

            _labelStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 14 };

            _orderStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.LowerCenter,
            };
        }

        private void DrawHud()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, 300f, 220f));

            GUILayout.Label($"Уровень: {_progress.Index}", _labelStyle);
            GUILayout.Label($"Ходов: {_simulation.Moves}", _labelStyle);
            GUILayout.Label($"Блоков на поле: {_board.BlockCount}", _labelStyle);
            GUILayout.Label($"Операций: {_operations.Active.Count}", _labelStyle);
            GUILayout.Label($"Ввод: {_input.Active.Name}", _labelStyle);
            GUILayout.Label($"Оверлей ({_overlayKey}): {(_overlayVisible ? "вкл" : "выкл")}", _labelStyle);
            GUILayout.Label($"Панель ({_panelKey}): скрыть", _labelStyle);

            if (_input.ModeCount > 1 && GUILayout.Button("Сменить режим ввода"))
                _input.SwitchToNextMode();

            if (GUILayout.Button("Перезапустить уровень"))
                _session.RestartLevel();

            if (GUILayout.Button("Сбросить сейв"))
                _session.ResetProgress();

            GUILayout.EndArea();
        }

        private void DrawOverlay()
        {
            for (var y = 0; y < _board.Height; y++)
            {
                for (var x = 0; x < _board.Width; x++)
                {
                    var pos = new Vector2Int(x, y);
                    var cell = _board.Get(pos);
                    var rect = CellRect(pos);

                    if (!cell.IsEmpty)
                        DrawFrame(rect, OccupiedColor, 1f);

                    DrawBlockedSides(rect, cell.BlockedSides);

                    if (!cell.IsEmpty && _operations.HasOperation(cell.BlockId) &&
                        _operations.TryGetOp(cell.BlockId, out var op))
                        DrawLabel(rect, op.Type.ToString(), Color.white);

                    if (!cell.IsEmpty)
                        DrawSortingOrder(rect, _view.SortingOrderOf(cell.BlockId));
                }
            }

            for (var i = 0; i < _dirtyMarks.Count; i++)
                DrawDirtyDot(CellRect(_dirtyMarks[i].Cell));
        }

        private void DrawDirtyDot(Rect rect)
        {
            var size = Mathf.Min(rect.width, rect.height) * 0.22f;
            var inset = size * 0.4f;
            var dot = new Rect(rect.xMax - size - inset, rect.yMin + inset, size, size);

            var previous = GUI.color;
            GUI.color = DirtyColor;
            GUI.DrawTexture(dot, _pixel);
            GUI.color = previous;
        }

        private Rect CellRect(Vector2Int cell)
        {
            var world = _layout.WorldPos(cell);
            var half = _layout.CellSize * 0.5f;

            var min = _cameraProvider.Camera.WorldToScreenPoint(new Vector3(world.x - half, world.y - half, 0f));
            var max = _cameraProvider.Camera.WorldToScreenPoint(new Vector3(world.x + half, world.y + half, 0f));

            var left = Mathf.Min(min.x, max.x);
            var right = Mathf.Max(min.x, max.x);
            var top = Screen.height - Mathf.Max(min.y, max.y);
            var bottom = Screen.height - Mathf.Min(min.y, max.y);

            return new Rect(left, top, right - left, bottom - top);
        }

        private void DrawBlockedSides(Rect rect, EntrySide sides)
        {
            const float thickness = 3f;

            if ((sides & EntrySide.Left) != EntrySide.None)
                DrawEdge(new Rect(rect.xMin, rect.yMin, thickness, rect.height));

            if ((sides & EntrySide.Right) != EntrySide.None)
                DrawEdge(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height));

            if ((sides & EntrySide.Top) != EntrySide.None)
                DrawEdge(new Rect(rect.xMin, rect.yMin, rect.width, thickness));
        }

        private void DrawEdge(Rect edge)
        {
            var previous = GUI.color;
            GUI.color = BlockedSideColor;
            GUI.DrawTexture(edge, _pixel);
            GUI.color = previous;
        }

        private void DrawFrame(Rect rect, Color color, float thickness)
        {
            var previous = GUI.color;
            GUI.color = color;

            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), _pixel);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), _pixel);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), _pixel);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), _pixel);

            GUI.color = previous;
        }

        private void DrawSortingOrder(Rect rect, int order)
        {
            var previous = GUI.color;
            GUI.color = Color.black;
            GUI.Label(rect, $"SO:{order}", _orderStyle);
            GUI.color = previous;
        }

        private void DrawLabel(Rect rect, string text, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.Label(rect, text, _labelStyle);
            GUI.color = previous;
        }
#endif
    }
}

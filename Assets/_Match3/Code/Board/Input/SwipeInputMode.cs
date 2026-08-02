using Match3.Board.Data;
using Match3.Board.View;
using Match3.Configs;
using UnityEngine;

namespace Match3.Board.Input
{
    public sealed class SwipeInputMode : IInputMode
    {
        private readonly GameplayConfig _gameplay;

        private bool _tracking;
        private Vector2 _startPosition;
        private Vector2Int _startCell;

        public SwipeInputMode(GameplayConfig gameplay) => _gameplay = gameplay;

        public string Name => "Свайп";

        public void Cancel() => _tracking = false;

        public bool TryRead(BoardLayout layout, out InputGesture gesture)
        {
            gesture = default;

            if (!PointerReader.TryGetPosition(out var position))
                return false;

            if (PointerReader.PressedThisFrame())
            {
                _tracking = layout.TryScreenToCell(position, out _startCell);
                _startPosition = position;
                return false;
            }

            if (!PointerReader.ReleasedThisFrame())
                return false;

            if (!_tracking)
                return false;

            _tracking = false;

            var delta = position - _startPosition;
            if (delta.magnitude < _gameplay.SwipeThresholdPixels)
                return false;

            var direction = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? delta.x > 0f ? MoveDirection.Right : MoveDirection.Left
                : delta.y > 0f ? MoveDirection.Up : MoveDirection.Down;

            gesture = new InputGesture(_startCell, direction);
            return true;
        }
    }
}

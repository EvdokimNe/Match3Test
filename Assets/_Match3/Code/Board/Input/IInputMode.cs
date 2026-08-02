using Match3.Board.Data;
using Match3.Board.View;
using UnityEngine;

namespace Match3.Board.Input
{
    public readonly struct InputGesture
    {
        public readonly Vector2Int From;
        public readonly MoveDirection Direction;

        public InputGesture(Vector2Int from, MoveDirection direction)
        {
            From = from;
            Direction = direction;
        }
    }

    public interface IInputMode
    {
        string Name { get; }

        bool TryRead(BoardLayout layout, out InputGesture gesture);

        void Cancel();
    }
}

using UnityEngine;

namespace Match3.Board.Data
{
    public enum MoveDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    public static class DirectionExtensions
    {
        public static Vector2Int ToOffset(this MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up: return new Vector2Int(0, 1);
                case MoveDirection.Down: return new Vector2Int(0, -1);
                case MoveDirection.Left: return new Vector2Int(-1, 0);
                default: return new Vector2Int(1, 0);
            }
        }

        public static MoveDirection Opposite(this MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up: return MoveDirection.Down;
                case MoveDirection.Down: return MoveDirection.Up;
                case MoveDirection.Left: return MoveDirection.Right;
                default: return MoveDirection.Left;
            }
        }

        public static EntrySide EntrySideFrom(this MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Right: return EntrySide.Left;
                case MoveDirection.Left: return EntrySide.Right;
                case MoveDirection.Down: return EntrySide.Top;
                default: return EntrySide.None;
            }
        }

        public static bool IsHorizontal(this MoveDirection direction) =>
            direction == MoveDirection.Left || direction == MoveDirection.Right;

        public static bool TryFromOffset(Vector2Int offset, out MoveDirection direction)
        {
            if (offset == new Vector2Int(0, 1)) { direction = MoveDirection.Up; return true; }
            if (offset == new Vector2Int(0, -1)) { direction = MoveDirection.Down; return true; }
            if (offset == new Vector2Int(-1, 0)) { direction = MoveDirection.Left; return true; }
            if (offset == new Vector2Int(1, 0)) { direction = MoveDirection.Right; return true; }

            direction = MoveDirection.Up;
            return false;
        }
    }
}

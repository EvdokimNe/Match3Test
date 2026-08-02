using Match3.Board.Data;
using UnityEngine;

namespace Match3.Board.Flow
{
    public readonly struct RejectedMove
    {
        public readonly bool Occurred;
        public readonly Vector2Int Cell;
        public readonly MoveDirection Direction;

        public RejectedMove(Vector2Int cell, MoveDirection direction)
        {
            Occurred = true;
            Cell = cell;
            Direction = direction;
        }
    }
}

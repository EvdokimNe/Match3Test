using Match3.Board.Data;
using Match3.Board.Operations;
using UnityEngine;

namespace Match3.Board.Flow
{
    public enum GameEventKind
    {
        OpCompleted = 0,
        CellFreed = 1,
    }

    public readonly struct GameEvent
    {
        public readonly GameEventKind Kind;
        public readonly Vector2Int Cell;
        public readonly PendingOp Op;

        private GameEvent(GameEventKind kind, Vector2Int cell, PendingOp op)
        {
            Kind = kind;
            Cell = cell;
            Op = op;
        }

        public static GameEvent OpCompleted(PendingOp op) =>
            new(GameEventKind.OpCompleted, default, op);

        public static GameEvent CellFreed(Vector2Int cell) =>
            new(GameEventKind.CellFreed, cell, null);
    }
}

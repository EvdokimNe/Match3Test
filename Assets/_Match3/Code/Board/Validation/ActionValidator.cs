using Match3.Board.Data;
using Match3.Board.Operations;
using UnityEngine;

namespace Match3.Board.Validation
{
    public enum MoveKind
    {
        None = 0,
        Move = 1,
        Swap = 2,
    }

    public readonly struct MoveResolution
    {
        public readonly MoveKind Kind;
        public readonly Vector2Int From;
        public readonly Vector2Int To;
        public readonly MoveDirection Direction;
        public readonly int BlockId;
        public readonly int OtherBlockId;

        public MoveResolution(MoveKind kind, Vector2Int from, Vector2Int to, MoveDirection direction,
            int blockId, int otherBlockId)
        {
            Kind = kind;
            From = from;
            To = to;
            Direction = direction;
            BlockId = blockId;
            OtherBlockId = otherBlockId;
        }

        public static MoveResolution None => new(MoveKind.None, default, default, default, Cell.NoBlock, Cell.NoBlock);
    }

    public sealed class ActionValidator
    {
        private readonly BoardState _board;
        private readonly BlockRegistry _blocks;
        private readonly OperationsLayer _operations;

        public ActionValidator(BoardState board, BlockRegistry blocks, OperationsLayer operations)
        {
            _board = board;
            _blocks = blocks;
            _operations = operations;
        }

        public bool CanBeMatched(int blockId)
        {
            if (!_blocks.IsAlive(blockId))
                return false;

            if (_operations.HasOperation(blockId))
                return false;

            return _board.HasPermanentSupport(_board.FindCell(blockId));
        }

        public bool CanMoveToVoid(int blockId)
        {
            if (!_blocks.IsAlive(blockId))
                return false;

            return !_operations.HasOperation(blockId);
        }

        public bool CanSwap(int blockId)
        {
            if (!_blocks.IsAlive(blockId))
                return false;

            return !_operations.HasOperation(blockId);
        }

        public bool TryResolve(Vector2Int from, MoveDirection direction, out MoveResolution resolution)
        {
            resolution = MoveResolution.None;

            if (!_board.InBounds(from))
                return false;

            var source = _board.Get(from);
            if (source.IsEmpty)
                return false;

            var to = from + direction.ToOffset();
            if (!_board.InBounds(to))
                return false;

            var target = _board.Get(to);

            if (target.IsEmpty)
                return TryResolveToVoid(from, to, direction, source.BlockId, target, out resolution);

            return TryResolveSwap(from, to, direction, source.BlockId, target.BlockId, out resolution);
        }

        private bool TryResolveToVoid(Vector2Int from, Vector2Int to, MoveDirection direction, int blockId,
            Cell target, out MoveResolution resolution)
        {
            resolution = MoveResolution.None;

            if (!direction.IsHorizontal())
                return false;

            if (target.IsEntryBlocked(direction.EntrySideFrom()))
                return false;

            if (!CanMoveToVoid(blockId))
                return false;

            resolution = new MoveResolution(MoveKind.Move, from, to, direction, blockId, Cell.NoBlock);
            return true;
        }

        private bool TryResolveSwap(Vector2Int from, Vector2Int to, MoveDirection direction, int blockId,
            int otherBlockId, out MoveResolution resolution)
        {
            resolution = MoveResolution.None;

            if (_operations.TryGetOp(otherBlockId, out var otherOp) &&
                (otherOp.Type == OpType.Fall || otherOp.Type == OpType.Destroy))
                return false;

            if (!CanSwap(blockId) || !CanSwap(otherBlockId))
                return false;

            resolution = new MoveResolution(MoveKind.Swap, from, to, direction, blockId, otherBlockId);
            return true;
        }
    }
}

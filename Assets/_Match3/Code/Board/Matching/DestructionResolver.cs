using System.Collections.Generic;
using Match3.Board.Data;
using Match3.Board.Operations;
using Match3.Configs;
using UnityEngine;

namespace Match3.Board.Matching
{
    public sealed class DestructionResolver
    {
        private readonly BoardState _board;
        private readonly OperationsLayer _operations;
        private readonly MatchContext _context;
        private readonly GameplayConfig _gameplay;

        private readonly List<Vector2Int> _seed = new(16);
        private readonly HashSet<Vector2Int> _cellsToDestroy = new();

        public DestructionResolver(BoardState board, OperationsLayer operations,
            MatchContext context, GameplayConfig gameplay)
        {
            _board = board;
            _operations = operations;
            _context = context;
            _gameplay = gameplay;
        }

        public bool ResolveDirty()
        {
            var dirty = _board.DirtyCells;
            if (dirty.Count == 0)
                return false;

            _cellsToDestroy.Clear();

            for (var i = 0; i < dirty.Count; i++)
            {
                var cell = dirty[i];

                if (!_context.TryGetMatchable(cell, out var block))
                    continue;

                var type = block.Type;
                if (type == null || type.Trigger == null || type.Expansion == null)
                    continue;

                _seed.Clear();
                if (!type.Trigger.TryTrigger(_context, cell, _seed))
                    continue;

                type.Expansion.Expand(_context, _seed, _cellsToDestroy);
            }

            _board.ClearDirty();

            if (_cellsToDestroy.Count == 0)
                return false;

            var op = _operations.Rent(OpType.Destroy);
            op.Duration = _gameplay.DestroyTime;

            foreach (var cell in _cellsToDestroy)
            {
                var content = _board.Get(cell);
                if (content.IsEmpty)
                    continue;

                op.AddBlock(content.BlockId, cell, cell);
            }

            if (op.BlockIds.Count == 0)
            {
                _operations.Release(op);
                return false;
            }

            _operations.Add(op);
            return true;
        }
    }
}

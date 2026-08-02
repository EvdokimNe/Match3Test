using System.Collections.Generic;
using Match3.Board.Data;
using UnityEngine;

namespace Match3.Board.Matching
{
    public sealed class LineMatcher
    {
        private const int MinLineLength = 3;

        private readonly MatchContext _context;
        private readonly List<Vector2Int> _line = new(16);

        public LineMatcher(MatchContext context) => _context = context;

        public bool TryFindLines(Vector2Int origin, List<Vector2Int> result)
        {
            if (!_context.TryGetMatchable(origin, out var block))
                return false;

            var found = false;
            found |= CollectLine(origin, block, MoveDirection.Left, MoveDirection.Right, result);
            found |= CollectLine(origin, block, MoveDirection.Down, MoveDirection.Up, result);
            return found;
        }

        private bool CollectLine(Vector2Int origin, Block originBlock, MoveDirection back, MoveDirection forward,
            List<Vector2Int> result)
        {
            _line.Clear();
            _line.Add(origin);

            Extend(origin, originBlock, back);
            Extend(origin, originBlock, forward);

            if (_line.Count < MinLineLength)
                return false;

            for (var i = 0; i < _line.Count; i++)
            {
                if (!result.Contains(_line[i]))
                    result.Add(_line[i]);
            }

            return true;
        }

        private void Extend(Vector2Int origin, Block previous, MoveDirection direction)
        {
            var offset = direction.ToOffset();
            var cursor = origin + offset;

            while (_context.TryGetMatchable(cursor, out var next))
            {
                if (!MergeRules.CanMerge(previous, next, direction))
                    return;

                _line.Add(cursor);
                previous = next;
                cursor += offset;
            }
        }
    }
}

using Match3.Board.Data;
using Match3.Board.Validation;
using UnityEngine;

namespace Match3.Board.Matching
{
    public sealed class MatchContext
    {
        public MatchContext(BoardState board, BlockRegistry blocks, ActionValidator validator)
        {
            Board = board;
            Blocks = blocks;
            Validator = validator;
            Lines = new LineMatcher(this);
        }

        public BoardState Board { get; }
        public BlockRegistry Blocks { get; }
        public ActionValidator Validator { get; }
        public LineMatcher Lines { get; }

        public bool TryGetMatchable(Vector2Int cell, out Block block)
        {
            block = null;

            if (!Board.InBounds(cell))
                return false;

            var content = Board.Get(cell);
            if (content.IsEmpty)
                return false;

            if (!Validator.CanBeMatched(content.BlockId))
                return false;

            block = Blocks.Get(content.BlockId);
            return block != null;
        }
    }
}

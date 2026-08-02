using Match3.Board.Data;
using Match3.Configs;

namespace Match3.Board.Matching
{
    public static class MergeRules
    {
        public static bool CanMerge(Block a, Block b, MoveDirection direction)
        {
            if (!TypesMatch(a.Type, b.Type))
                return false;

            if (!RulesAllow(a, direction))
                return false;

            return RulesAllow(b, direction.Opposite());
        }

        private static bool TypesMatch(BlockTypeConfig a, BlockTypeConfig b) =>
            a.MatchLogic.Matches(a, b) || b.MatchLogic.Matches(b, a);

        private static bool RulesAllow(Block block, MoveDirection direction)
        {
            var rules = block.Rules;
            for (var i = 0; i < rules.Count; i++)
            {
                if (!rules[i].AllowsMerge(direction))
                    return false;
            }

            return true;
        }
    }
}

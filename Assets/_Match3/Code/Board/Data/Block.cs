using System.Collections.Generic;
using Match3.Configs;

namespace Match3.Board.Data
{
    public sealed class Block
    {
        private static readonly List<IBlockRule> EmptyRules = new();

        private BlockTypeConfig _type;
        private BlockConfig _config;
        private List<IBlockRule> _rules = EmptyRules;

        public Block(int id) => Id = id;

        public int Id { get; }
        public BlockTypeConfig Type => _type;
        public BlockConfig Config => _config;
        public List<IBlockRule> Rules => _rules;

        public void Setup(BlockConfig config)
        {
            _config = config;
            _type = config.Type;
            _rules = config.Rules ?? EmptyRules;
        }

        public void Reset()
        {
            _config = null;
            _type = null;
            _rules = EmptyRules;
        }
    }
}

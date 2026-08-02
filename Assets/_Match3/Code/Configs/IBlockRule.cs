using System;
using Match3.Board.Data;

namespace Match3.Configs
{
    public interface IBlockRule
    {
        bool AllowsMerge(MoveDirection direction);
    }

    [Serializable]
    public sealed class CapRule : IBlockRule
    {
        public MoveDirection BlockedDirection = MoveDirection.Up;

        public bool AllowsMerge(MoveDirection direction) => direction != BlockedDirection;
    }
}

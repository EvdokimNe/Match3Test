using System;

namespace Match3.Board.Data
{
    [Flags]
    public enum EntrySide
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Top = 1 << 2,
    }
}

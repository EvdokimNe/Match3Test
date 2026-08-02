namespace Match3.Board.Data
{
    public struct Cell
    {
        public const int NoBlock = -1;

        public int BlockId;

        public int FirstClaimBlockId;
        public EntrySide FirstClaimSides;
        public int SecondClaimBlockId;
        public EntrySide SecondClaimSides;

        public bool IsEmpty => BlockId == NoBlock;

        public bool HasClaims => FirstClaimBlockId != NoBlock || SecondClaimBlockId != NoBlock;

        public EntrySide BlockedSides => FirstClaimSides | SecondClaimSides;

        public bool IsEntryBlocked(EntrySide side) => (BlockedSides & side) != EntrySide.None;

        public static Cell Empty => new()
        {
            BlockId = NoBlock,
            FirstClaimBlockId = NoBlock,
            FirstClaimSides = EntrySide.None,
            SecondClaimBlockId = NoBlock,
            SecondClaimSides = EntrySide.None,
        };
    }
}

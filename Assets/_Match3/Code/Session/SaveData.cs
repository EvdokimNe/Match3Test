using System.Collections.Generic;

namespace Match3.Session
{
    public struct BlockRecord
    {
        public string ConfigId;
        public int X;
        public int Y;
    }

    public sealed class SaveData
    {
        public int Level;
        public int Moves;
        public List<BlockRecord> Blocks = new();
    }
}

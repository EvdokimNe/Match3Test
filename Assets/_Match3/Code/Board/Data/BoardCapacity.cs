using UnityEngine;

namespace Match3.Board.Data
{
    public readonly struct BoardCapacity
    {
        public readonly int Width;
        public readonly int Height;

        public BoardCapacity(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
        }

        public int Cells => Width * Height;
    }
}

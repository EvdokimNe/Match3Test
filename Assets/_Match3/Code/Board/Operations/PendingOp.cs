using System.Collections.Generic;
using UnityEngine;

namespace Match3.Board.Operations
{
    public sealed class PendingOp
    {
        public const int NoRow = -1;

        public readonly List<int> BlockIds = new(8);
        public readonly List<Vector2Int> FromCells = new(8);
        public readonly List<Vector2Int> ToCells = new(8);

        public OpType Type;
        public int Sequence;
        public bool Completed;

        public float Elapsed;
        public float Duration;

        public int Column;
        public float CurrentY;
        public float Velocity;
        public float Accel;
        public float MaxSpeed;
        public int ReservedLowRow = NoRow;
        public int ReservedHighRow = NoRow;

        public float Progress => Duration <= 0f ? 1f : Mathf.Clamp01(Elapsed / Duration);

        public int AnchorRow => Mathf.FloorToInt(CurrentY);

        public void Reset()
        {
            BlockIds.Clear();
            FromCells.Clear();
            ToCells.Clear();

            Completed = false;
            Elapsed = 0f;
            Duration = 0f;

            Column = 0;
            CurrentY = 0f;
            Velocity = 0f;
            Accel = 0f;
            MaxSpeed = 0f;
            ReservedLowRow = NoRow;
            ReservedHighRow = NoRow;
        }

        public void AddBlock(int blockId, Vector2Int from, Vector2Int to)
        {
            BlockIds.Add(blockId);
            FromCells.Add(from);
            ToCells.Add(to);
        }
    }
}

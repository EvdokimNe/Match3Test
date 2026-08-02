using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3.Board.Data
{
    public sealed class BoardState
    {
        private static readonly Vector2Int Nowhere = new(-1, -1);

        private readonly List<Vector2Int> _dirtyCells = new(32);

        private Cell[] _cells = Array.Empty<Cell>();
        private Vector2Int[] _blockCells = Array.Empty<Vector2Int>();
        private int _temporarySupportCount;

        public int MaxWidth { get; private set; }
        public int MaxHeight { get; private set; }
        public int Capacity => MaxWidth * MaxHeight;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int BlockCount { get; private set; }

        public IReadOnlyList<Vector2Int> DirtyCells => _dirtyCells;

        public void Initialize(int maxWidth, int maxHeight)
        {
            MaxWidth = Mathf.Max(1, maxWidth);
            MaxHeight = Mathf.Max(1, maxHeight);

            _cells = new Cell[Capacity];
            _blockCells = new Vector2Int[Capacity];

            for (var i = 0; i < _blockCells.Length; i++)
                _blockCells[i] = Nowhere;

            SetBounds(MaxWidth, MaxHeight);
        }

        public void SetBounds(int width, int height)
        {
            if (width > MaxWidth || height > MaxHeight)
            {
                Debug.LogError($"[Match3] Поле {width}x{height} больше максимального {MaxWidth}x{MaxHeight}");
                width = Mathf.Min(width, MaxWidth);
                height = Mathf.Min(height, MaxHeight);
            }

            Width = width;
            Height = height;
            BlockCount = 0;
            _temporarySupportCount = 0;
            _dirtyCells.Clear();

            for (var i = 0; i < _cells.Length; i++)
                _cells[i] = Cell.Empty;
        }

        public bool InBounds(Vector2Int pos) =>
            pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;

        public Cell Get(Vector2Int pos) => InBounds(pos) ? _cells[Index(pos)] : Cell.Empty;

        public Cell Get(int x, int y) => Get(new Vector2Int(x, y));

        public bool IsEmpty(Vector2Int pos) => InBounds(pos) && _cells[Index(pos)].IsEmpty;

        public bool IsFree(Vector2Int pos)
        {
            if (!InBounds(pos))
                return false;

            var cell = _cells[Index(pos)];
            return cell.IsEmpty && !cell.HasClaims;
        }

        public Vector2Int FindCell(int blockId) =>
            blockId >= 0 && blockId < _blockCells.Length ? _blockCells[blockId] : Nowhere;

        public void PlaceBlock(int blockId, Vector2Int pos)
        {
            if (!InBounds(pos))
            {
                Debug.LogError($"[Match3] PlaceBlock вне поля: {pos}");
                return;
            }

            ref var cell = ref _cells[Index(pos)];
            if (!cell.IsEmpty)
            {
                Debug.LogError($"[Match3] PlaceBlock в занятую клетку {pos}, там блок {cell.BlockId}");
                return;
            }

            cell.BlockId = blockId;
            _blockCells[blockId] = pos;
            BlockCount++;
        }

        public void RemoveBlock(Vector2Int pos)
        {
            if (!InBounds(pos))
                return;

            ref var cell = ref _cells[Index(pos)];
            if (cell.IsEmpty)
                return;

            _blockCells[cell.BlockId] = Nowhere;
            cell.BlockId = Cell.NoBlock;
            BlockCount--;
        }

        public void MoveBlock(Vector2Int from, Vector2Int to)
        {
            if (from == to)
                return;

            if (!InBounds(from) || !InBounds(to))
            {
                Debug.LogError($"[Match3] MoveBlock вне поля: {from} -> {to}");
                return;
            }

            ref var source = ref _cells[Index(from)];
            ref var target = ref _cells[Index(to)];

            if (source.IsEmpty || !target.IsEmpty)
            {
                Debug.LogError($"[Match3] MoveBlock {from} -> {to}: источник пуст или цель занята");
                return;
            }

            var blockId = source.BlockId;
            source.BlockId = Cell.NoBlock;
            target.BlockId = blockId;
            _blockCells[blockId] = to;
        }

        public void SwapBlocks(Vector2Int a, Vector2Int b)
        {
            if (a == b)
                return;

            if (!InBounds(a) || !InBounds(b))
            {
                Debug.LogError($"[Match3] SwapBlocks вне поля: {a} <-> {b}");
                return;
            }

            ref var cellA = ref _cells[Index(a)];
            ref var cellB = ref _cells[Index(b)];

            (cellA.BlockId, cellB.BlockId) = (cellB.BlockId, cellA.BlockId);

            if (cellA.BlockId != Cell.NoBlock) _blockCells[cellA.BlockId] = a;
            if (cellB.BlockId != Cell.NoBlock) _blockCells[cellB.BlockId] = b;
        }

        public void Claim(Vector2Int pos, int blockId, EntrySide sides)
        {
            if (!InBounds(pos))
                return;

            ref var cell = ref _cells[Index(pos)];
            var hadTop = cell.IsEntryBlocked(EntrySide.Top);

            if (cell.FirstClaimBlockId == blockId || cell.FirstClaimBlockId == Cell.NoBlock)
            {
                cell.FirstClaimBlockId = blockId;
                cell.FirstClaimSides = sides;
            }
            else if (cell.SecondClaimBlockId == blockId || cell.SecondClaimBlockId == Cell.NoBlock)
            {
                cell.SecondClaimBlockId = blockId;
                cell.SecondClaimSides = sides;
            }
            else
            {
                Debug.LogError($"[Match3] Третий захват клетки {pos} блоком {blockId}: " +
                               $"уже держат {cell.FirstClaimBlockId} и {cell.SecondClaimBlockId}.");
                return;
            }

            UpdateTemporarySupports(hadTop, cell.IsEntryBlocked(EntrySide.Top));
        }

        public void ReleaseClaim(Vector2Int pos, int blockId)
        {
            if (!InBounds(pos))
                return;

            ref var cell = ref _cells[Index(pos)];
            var hadTop = cell.IsEntryBlocked(EntrySide.Top);

            if (cell.FirstClaimBlockId == blockId)
            {
                cell.FirstClaimBlockId = Cell.NoBlock;
                cell.FirstClaimSides = EntrySide.None;
            }

            if (cell.SecondClaimBlockId == blockId)
            {
                cell.SecondClaimBlockId = Cell.NoBlock;
                cell.SecondClaimSides = EntrySide.None;
            }

            UpdateTemporarySupports(hadTop, cell.IsEntryBlocked(EntrySide.Top));
        }

        public bool HasPermanentSupport(Vector2Int pos)
        {
            if (_temporarySupportCount == 0)
                return true;

            for (var y = pos.y - 1; y >= 0; y--)
            {
                var cell = _cells[Index(new Vector2Int(pos.x, y))];

                if (cell.IsEmpty && cell.IsEntryBlocked(EntrySide.Top))
                    return false;
            }

            return true;
        }

        private void UpdateTemporarySupports(bool hadTop, bool hasTop)
        {
            if (hadTop == hasTop)
                return;

            _temporarySupportCount += hasTop ? 1 : -1;
        }

        public event Action<Vector2Int> DirtyMarked;

        public void MarkDirty(Vector2Int pos)
        {
            if (!InBounds(pos) || _dirtyCells.Contains(pos))
                return;

            _dirtyCells.Add(pos);
            DirtyMarked?.Invoke(pos);
        }

        public void ClearDirty() => _dirtyCells.Clear();

        private int Index(Vector2Int pos) => pos.y * MaxWidth + pos.x;
    }
}

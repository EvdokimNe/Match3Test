using System;
using System.Collections.Generic;
using Match3.Board.Data;
using Match3.Configs;
using UnityEngine;

namespace Match3.Board.Operations
{
    public sealed class OperationsLayer
    {
        private const float RowEpsilon = 0.0001f;
        private const EntrySide FallerSides = EntrySide.Left | EntrySide.Right;

        private readonly BoardState _board;
        private readonly GameplayConfig _gameplay;
        private readonly List<PendingOp> _active = new(64);
        private readonly List<PendingOp> _falls = new(64);
        private readonly Stack<PendingOp> _pool = new();

        private PendingOp[] _opByBlock = Array.Empty<PendingOp>();
        private int _sequenceCounter;

        public OperationsLayer(BoardState board, GameplayConfig gameplay)
        {
            _board = board;
            _gameplay = gameplay;
        }

        public event Action<PendingOp> Started;

        public IReadOnlyList<PendingOp> Active => _active;

        public void Initialize(int blockCapacity) => _opByBlock = new PendingOp[blockCapacity];

        public void Clear()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
                Release(_active[i]);

            _active.Clear();
            Array.Clear(_opByBlock, 0, _opByBlock.Length);
        }

        public bool HasAny() => _active.Count > 0;

        public bool HasOperation(int blockId) =>
            blockId >= 0 && blockId < _opByBlock.Length && _opByBlock[blockId] != null;

        public bool TryGetOp(int blockId, out PendingOp op)
        {
            if (blockId < 0 || blockId >= _opByBlock.Length)
            {
                op = null;
                return false;
            }

            op = _opByBlock[blockId];
            return op != null;
        }

        public PendingOp Rent(OpType type)
        {
            var op = _pool.Count > 0 ? _pool.Pop() : new PendingOp();
            op.Reset();
            op.Type = type;
            op.Sequence = ++_sequenceCounter;
            return op;
        }

        public void Add(PendingOp op)
        {
            _active.Add(op);

            for (var i = 0; i < op.BlockIds.Count; i++)
                _opByBlock[op.BlockIds[i]] = op;

            Started?.Invoke(op);
        }

        public void Release(PendingOp op)
        {
            op.Sequence = ++_sequenceCounter;
            op.Reset();
            _pool.Push(op);
        }

        public void Tick(float dt, List<PendingOp> completed)
        {
            TickFalls(dt);

            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var op = _active[i];

                if (op.Type != OpType.Fall)
                    TickTimed(op, dt);

                if (!op.Completed)
                    continue;

                _active.RemoveAt(i);

                for (var b = 0; b < op.BlockIds.Count; b++)
                    _opByBlock[op.BlockIds[b]] = null;

                completed.Add(op);
            }
        }

        public PendingOp CreateFall(int blockId, Vector2Int cell)
        {
            var op = Rent(OpType.Fall);
            op.AddBlock(blockId, cell, cell);
            op.Column = cell.x;
            op.CurrentY = cell.y;
            op.Velocity = _gameplay.FallSpeed;
            op.Accel = _gameplay.FallAccel;
            op.MaxSpeed = _gameplay.MaxFallSpeed;
            op.ReservedLowRow = cell.y;
            op.ReservedHighRow = cell.y;
            _board.Claim(cell, blockId, FallerSides);
            return op;
        }
        
        public bool HasFalls()
        {
            for (var i = 0; i < _active.Count; i++)
            {
                if (_active[i].Type == OpType.Fall)
                    return true;
            }
        
            return false;
        }

        public bool HasStaticObstacle(Vector2Int pos)
        {
            if (!_board.InBounds(pos))
                return true;

            var cell = _board.Get(pos);

            if (!cell.IsEmpty)
                return !IsFalling(cell.BlockId);

            return cell.IsEntryBlocked(EntrySide.Top);
        }

        public bool TryGetFallerAt(Vector2Int pos, out PendingOp fallOp)
        {
            fallOp = null;

            if (!_board.InBounds(pos))
                return false;

            var cell = _board.Get(pos);

            TakeLowerFaller(cell.BlockId, ref fallOp);
            TakeLowerFaller(cell.FirstClaimBlockId, ref fallOp);
            TakeLowerFaller(cell.SecondClaimBlockId, ref fallOp);

            return fallOp != null;
        }

        private void TakeLowerFaller(int blockId, ref PendingOp lowest)
        {
            if (!TryGetOp(blockId, out var op) || op.Type != OpType.Fall)
                return;

            if (lowest == null || op.CurrentY < lowest.CurrentY)
                lowest = op;
        }

        private bool IsFalling(int blockId) =>
            TryGetOp(blockId, out var op) && op.Type == OpType.Fall;

        private void TickFalls(float dt)
        {
            _falls.Clear();

            for (var i = 0; i < _active.Count; i++)
            {
                var op = _active[i];
                if (op.Type != OpType.Fall)
                    continue;

                var index = _falls.Count;
                while (index > 0 && _falls[index - 1].CurrentY > op.CurrentY)
                    index--;

                _falls.Insert(index, op);
            }

            for (var i = 0; i < _falls.Count; i++)
                TickFall(_falls[i], dt);
        }

        private static void TickTimed(PendingOp op, float dt)
        {
            op.Elapsed += dt;
            if (op.Elapsed >= op.Duration)
            {
                op.Elapsed = op.Duration;
                op.Completed = true;
            }
        }

        private void TickFall(PendingOp op, float dt)
        {
            op.Velocity = Mathf.Min(op.Velocity + op.Accel * dt, op.MaxSpeed);

            var anchor = op.AnchorRow;
            var newY = op.CurrentY - op.Velocity * dt;
            var limitY = FindLimit(op, anchor, out var limitIsStatic);

            var landed = false;
            if (newY <= limitY)
            {
                newY = limitY;
                landed = limitIsStatic;
            }

            op.CurrentY = newY;

            var newAnchor = Mathf.FloorToInt(newY);
            if (newAnchor != anchor)
            {
                ClearReservation(op);
                _board.MoveBlock(new Vector2Int(op.Column, anchor), new Vector2Int(op.Column, newAnchor));
            }

            var low = newAnchor;
            var high = newY > newAnchor + RowEpsilon ? Mathf.Min(newAnchor + 1, _board.Height - 1) : newAnchor;
            SetReservation(op, low, high);

            if (!landed)
                return;

            op.ToCells[0] = new Vector2Int(op.Column, newAnchor);
            op.Completed = true;
        }

        private float FindLimit(PendingOp op, int anchor, out bool limitIsStatic)
        {
            for (var row = anchor - 1; row >= 0; row--)
            {
                var pos = new Vector2Int(op.Column, row);

                if (TryGetFallerAt(pos, out var leader))
                {
                    limitIsStatic = false;
                    return leader.CurrentY + 1f;
                }

                if (HasStaticObstacle(pos))
                {
                    limitIsStatic = true;
                    return row + 1;
                }
            }

            limitIsStatic = true;
            return 0f;
        }

        private void SetReservation(PendingOp op, int low, int high)
        {
            if (op.ReservedLowRow == low && op.ReservedHighRow == high)
                return;

            ClearReservation(op);

            var blockId = op.BlockIds[0];
            for (var row = low; row <= high; row++)
                _board.Claim(new Vector2Int(op.Column, row), blockId, FallerSides);

            op.ReservedLowRow = low;
            op.ReservedHighRow = high;
        }

        private void ClearReservation(PendingOp op)
        {
            if (op.ReservedLowRow == PendingOp.NoRow)
                return;

            var blockId = op.BlockIds[0];
            for (var row = op.ReservedLowRow; row <= op.ReservedHighRow; row++)
                _board.ReleaseClaim(new Vector2Int(op.Column, row), blockId);

            op.ReservedLowRow = PendingOp.NoRow;
            op.ReservedHighRow = PendingOp.NoRow;
        }

        public void ReleaseFallClaims(PendingOp op) => ClearReservation(op);
    }
}

using System;
using System.Collections.Generic;
using Match3.Board.Data;
using Match3.Board.Matching;
using Match3.Board.Operations;
using Match3.Board.Validation;
using Match3.Configs;
using UnityEngine;

namespace Match3.Board.Flow
{
    public sealed class BoardSimulation
    {
        private readonly BoardState _board;
        private readonly BlockRegistry _blocks;
        private readonly OperationsLayer _operations;
        private readonly ActionValidator _validator;
        private readonly DestructionResolver _destruction;
        private readonly GameplayConfig _gameplay;

        private readonly Queue<GameEvent> _events = new(64);
        private readonly List<PendingOp> _completed = new(16);

        private bool _hasRequestedMove;
        private Vector2Int _requestedFrom;
        private MoveDirection _requestedDirection;

        public BoardSimulation(BoardState board, BlockRegistry blocks, OperationsLayer operations,
            ActionValidator validator, DestructionResolver destruction, GameplayConfig gameplay)
        {
            _board = board;
            _blocks = blocks;
            _operations = operations;
            _validator = validator;
            _destruction = destruction;
            _gameplay = gameplay;
        }

        public int Moves { get; private set; }

        public RejectedMove Rejection { get; private set; }

        public void ResetSession(int moves)
        {
            _events.Clear();
            _completed.Clear();
            _board.ClearDirty();
            _hasRequestedMove = false;
            Moves = moves;
        }

        public void RequestMove(Vector2Int from, MoveDirection direction)
        {
            _hasRequestedMove = true;
            _requestedFrom = from;
            _requestedDirection = direction;
        }

        public BoardTickResult Tick(float dt)
        {
            Rejection = default;

            var hadPendingOperations = HasPendingOperations;

            _completed.Clear();
            _operations.Tick(dt, _completed);

            for (var i = 0; i < _completed.Count; i++)
                _events.Enqueue(GameEvent.OpCompleted(_completed[i]));

            ProcessQueuedEvents();
            ApplyRequestedMove();
            
            if (_operations.HasFalls())
                return BoardTickResult.InProgress;

            _destruction.ResolveDirty();

            if (HasPendingOperations)
                return BoardTickResult.InProgress;

            return hadPendingOperations ? BoardTickResult.BecameStable : BoardTickResult.StayedStable;
        }

        private bool HasPendingOperations => _operations.HasAny();

        private void ProcessQueuedEvents()
        {
            while (_events.Count > 0)
                Process(_events.Dequeue());
        }

        private void Process(GameEvent gameEvent)
        {
            switch (gameEvent.Kind)
            {
                case GameEventKind.OpCompleted:
                    ProcessOpCompleted(gameEvent.Op);
                    break;
                case GameEventKind.CellFreed:
                    ProcessCellFreed(gameEvent.Cell);
                    break;
            }
        }

        private void ApplyRequestedMove()
        {
            if (!_hasRequestedMove)
                return;

            _hasRequestedMove = false;

            if (!_validator.TryResolve(_requestedFrom, _requestedDirection, out var resolution))
            {
                Rejection = new RejectedMove(_requestedFrom, _requestedDirection);
                return;
            }

            if (resolution.Kind == MoveKind.Move)
                StartMove(resolution);
            else
                StartSwap(resolution);

            Moves++;
        }

        private void StartMove(MoveResolution resolution)
        {
            _board.MoveBlock(resolution.From, resolution.To);
            _board.Claim(resolution.From, resolution.BlockId, EntrySide.Top);

            var op = _operations.Rent(OpType.Move);
            op.Duration = _gameplay.MoveTime;
            op.AddBlock(resolution.BlockId, resolution.From, resolution.To);
            _operations.Add(op);
        }

        private void StartSwap(MoveResolution resolution)
        {
            _board.SwapBlocks(resolution.From, resolution.To);

            var op = _operations.Rent(OpType.Swap);
            op.Duration = _gameplay.MoveTime;
            op.AddBlock(resolution.BlockId, resolution.From, resolution.To);
            op.AddBlock(resolution.OtherBlockId, resolution.To, resolution.From);
            _operations.Add(op);
        }

        private void ProcessOpCompleted(PendingOp op)
        {
            switch (op.Type)
            {
                case OpType.Move:
                {
                    var source = op.FromCells[0];
                    _board.ReleaseClaim(source, op.BlockIds[0]);
                    _events.Enqueue(GameEvent.CellFreed(source));
                    OnBlockOperationEnd(op.ToCells[0]);
                    break;
                }
                case OpType.Swap:
                {
                    for (var i = 0; i < op.ToCells.Count; i++)
                        OnBlockOperationEnd(op.ToCells[i]);
                    break;
                }
                case OpType.Fall:
                {
                    _operations.ReleaseFallClaims(op);
                    _board.MarkDirty(op.ToCells[0]);
                    break;
                }
                case OpType.Destroy:
                {
                    ProcessDestroyCompleted(op);
                    break;
                }
            }

            _operations.Release(op);
        }

        private void ProcessDestroyCompleted(PendingOp op)
        {
            for (var i = 0; i < op.BlockIds.Count; i++)
            {
                var cell = op.ToCells[i];
                var blockId = op.BlockIds[i];

                _board.RemoveBlock(cell);
                _blocks.Despawn(blockId);
            }

            for (var i = 0; i < op.ToCells.Count; i++)
                _events.Enqueue(GameEvent.CellFreed(op.ToCells[i]));
        }

        private void OnBlockOperationEnd(Vector2Int cell)
        {
            var content = _board.Get(cell);
            if (content.IsEmpty)
                return;

            if (_operations.HasStaticObstacle(cell + Vector2Int.down))
                _board.MarkDirty(cell);
            else
                StartFall(content.BlockId, cell);
        }

        private void ProcessCellFreed(Vector2Int cell)
        {
            if (_operations.HasStaticObstacle(cell))
            {
                MarkBlocksAboveDirty(cell);
                return;
            }

            for (var y = cell.y + 1; y < _board.Height; y++)
            {
                var probe = new Vector2Int(cell.x, y);
                var content = _board.Get(probe);

                if (content.IsEmpty || _operations.HasOperation(content.BlockId))
                {
                    if (_operations.HasStaticObstacle(probe))
                        return;

                    continue;
                }

                StartFall(content.BlockId, probe);
            }
        }

        private void MarkBlocksAboveDirty(Vector2Int cell)
        {
            for (var y = cell.y + 1; y < _board.Height; y++)
            {
                var probe = new Vector2Int(cell.x, y);
                var content = _board.Get(probe);

                if (content.IsEmpty || _operations.HasOperation(content.BlockId))
                    return;

                _board.MarkDirty(probe);
            }
        }

        private void StartFall(int blockId, Vector2Int cell)
        {
            var op = _operations.CreateFall(blockId, cell);
            _operations.Add(op);

            _events.Enqueue(GameEvent.CellFreed(cell));
        }
    }
}

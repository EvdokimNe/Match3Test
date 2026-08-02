using System.Collections.Generic;
using Match3.Board.Data;
using Match3.Board.Operations;
using UnityEngine;

namespace Match3.Board.View
{
    public sealed class BlockSlot : MonoBehaviour
    {
        private enum CommandKind
        {
            Move = 0,
            Fall = 1,
            Destroy = 2,
        }

        private struct ViewCommand
        {
            public CommandKind Kind;
            public Vector2Int From;
            public Vector2Int To;
            public PendingOp Op;
            public int Sequence;
            public int Offset;
        }

        private readonly Queue<ViewCommand> _commands = new(4);

        private BoardLayout _layout;
        private AnimationCurve _moveEasing;
        private BlockVisual _visual;
        private int _sortingOrder;

        public bool HasVisual => _visual != null;
        public int SortingOrder => _sortingOrder;

        public void Initialize(BoardLayout layout, AnimationCurve moveEasing)
        {
            _layout = layout;
            _moveEasing = moveEasing;
        }

        public void Attach(BlockVisual visual, Vector2Int cell, float animationPhase)
        {
            _commands.Clear();
            _visual = visual;

            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            visual.Show(animationPhase);

            SnapTo(cell);
        }

        public BlockVisual Detach()
        {
            var visual = _visual;

            _commands.Clear();
            _visual = null;

            return visual;
        }

        public void SnapTo(Vector2Int cell)
        {
            if (cell.x < 0)
                return;

            transform.position = _layout.WorldPos(cell);
            transform.localScale = Vector3.one * _layout.CellSize;

            ApplySorting(cell.x, cell.y, 0);
        }

        public void PlayRejected(MoveDirection direction) => _visual.PlayRejected(direction);

        public void EnqueueMove(Vector2Int from, Vector2Int to, PendingOp op, int offset)
        {
            _commands.Enqueue(new ViewCommand
            {
                Kind = CommandKind.Move, From = from, To = to, Op = op, Sequence = op.Sequence, Offset = offset,
            });
        }

        public void EnqueueFall(PendingOp op)
        {
            _commands.Enqueue(new ViewCommand
            {
                Kind = CommandKind.Fall, From = op.FromCells[0], To = op.FromCells[0], Op = op, Sequence = op.Sequence,
            });
        }

        public void EnqueueDestroy(PendingOp op, Vector2Int cell)
        {
            _commands.Enqueue(new ViewCommand
            {
                Kind = CommandKind.Destroy, From = cell, To = cell, Op = op, Sequence = op.Sequence,
            });
        }

        public bool Tick(float deltaTime)
        {
            while (_commands.Count > 0)
            {
                var command = _commands.Peek();
                var op = command.Op;
                var finished = op == null || op.Sequence != command.Sequence || op.Completed;

                if (!finished)
                {
                    Apply(command, deltaTime);
                    return true;
                }

                Complete(command);
                _commands.Dequeue();
            }

            _visual.Advance(deltaTime);
            return false;
        }

        private void Apply(ViewCommand command, float deltaTime)
        {
            var op = command.Op;

            switch (command.Kind)
            {
                case CommandKind.Move:
                    var position = Vector2.Lerp(command.From, command.To, _moveEasing.Evaluate(op.Progress));
                    transform.position = _layout.WorldPos(position.x, position.y);
                    ApplySorting(position.x, position.y, command.Offset);
                    _visual.Advance(deltaTime);
                    break;
                case CommandKind.Fall:
                    transform.position = _layout.WorldPos(op.Column, op.CurrentY);
                    ApplySorting(op.Column, op.CurrentY, 0);
                    _visual.Advance(deltaTime);
                    break;
                case CommandKind.Destroy:
                    transform.position = _layout.WorldPos(command.To);
                    ApplySorting(command.To.x, command.To.y, _layout.DestroyLift);
                    _visual.SampleDestroy(op.Progress);
                    break;
            }
        }

        private void Complete(ViewCommand command)
        {
            if (command.Kind == CommandKind.Move)
                transform.position = _layout.WorldPos(command.To);
        }

        private void ApplySorting(float x, float y, int offset)
        {
            var order = _layout.SortingOrder(x, y) + offset;

            _sortingOrder = order;
            _visual.SortingOrder = order;
        }
    }
}

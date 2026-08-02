using System;
using System.Collections.Generic;
using Match3.Board.Data;
using Match3.Board.Flow;
using Match3.Board.Operations;
using Match3.Configs;
using Match3.Scene;
using UnityEngine;
using uPools;

namespace Match3.Board.View
{
    public sealed class GridView : IDisposable
    {
        private readonly BoardState _board;
        private readonly BlockRegistry _blocks;
        private readonly OperationsLayer _operations;
        private readonly BoardSimulation _simulation;
        private readonly BoardLayout _layout;
        private readonly BoardAreaAuthoring _area;
        private readonly MainCameraSceneProvider _cameraProvider;
        private readonly BlockVisualCatalog _catalog;
        private readonly EasingConfig _easing;

        private readonly Dictionary<BlockConfig, BlockVisualCatalog.Entry> _visuals = new();

        private BlockSlot[] _slots = Array.Empty<BlockSlot>();
        private Vector2Int _screenSize;

        public GridView(BoardState board, BlockRegistry blocks, OperationsLayer operations,
            BoardSimulation simulation, BoardLayout layout, BoardAreaAuthoring area,
            MainCameraSceneProvider cameraProvider, BlockVisualCatalog catalog, EasingConfig easing)
        {
            _board = board;
            _blocks = blocks;
            _operations = operations;
            _simulation = simulation;
            _layout = layout;
            _area = area;
            _cameraProvider = cameraProvider;
            _catalog = catalog;
            _easing = easing;

            _operations.Started += OnOperationStarted;
        }

        public void Dispose() => _operations.Started -= OnOperationStarted;

        public void Initialize()
        {
            if (_slots.Length > 0)
                return;

            BuildVisualLookup();

            _slots = new BlockSlot[_area.Capacity.Cells];
            for (var i = 0; i < _slots.Length; i++)
                _slots[i] = CreateSlot(i);
        }

        public void Rebuild()
        {
            ConfigureLayout();

            for (var i = 0; i < _slots.Length; i++)
                ReleaseVisual(_slots[i]);

            var alive = _blocks.AliveIds;
            for (var i = 0; i < alive.Count; i++)
            {
                var id = alive[i];
                var entry = _visuals[_blocks.Get(id).Config];

                var visual = SharedGameObjectPool.Rent(entry.Prefab, _slots[id].transform);
                _slots[id].Attach(visual, _board.FindCell(id), UnityEngine.Random.value);
            }
        }

        public int SortingOrderOf(int blockId) => _slots[blockId].SortingOrder;

        public void Draw(float deltaTime)
        {
            if (_screenSize.x != Screen.width || _screenSize.y != Screen.height)
                ConfigureLayout();

            for (var id = 0; id < _slots.Length; id++)
            {
                var slot = _slots[id];
                if (!slot.HasVisual)
                    continue;

                if (!_blocks.IsAlive(id))
                {
                    ReleaseVisual(slot);
                    continue;
                }

                if (!slot.Tick(deltaTime))
                    slot.SnapTo(_board.FindCell(id));
            }

            PlayRejection();
        }

        private void PlayRejection()
        {
            var rejection = _simulation.Rejection;
            if (!rejection.Occurred)
                return;

            var content = _board.Get(rejection.Cell);
            if (content.IsEmpty)
                return;

            _slots[content.BlockId].PlayRejected(rejection.Direction);
        }

        private void OnOperationStarted(PendingOp op)
        {
            for (var i = 0; i < op.BlockIds.Count; i++)
            {
                var slot = _slots[op.BlockIds[i]];

                switch (op.Type)
                {
                    case OpType.Move:
                    case OpType.Swap:
                        slot.EnqueueMove(op.FromCells[i], op.ToCells[i], op, SwapOffset(op, i));
                        break;
                    case OpType.Fall:
                        slot.EnqueueFall(op);
                        break;
                    case OpType.Destroy:
                        slot.EnqueueDestroy(op, op.ToCells[i]);
                        break;
                }
            }
        }

        private int SwapOffset(PendingOp op, int index)
        {
            if (op.Type != OpType.Swap)
                return 0;

            var from = op.FromCells[index];
            var to = op.ToCells[index];

            return _layout.SortingOrder(from.x, from.y) > _layout.SortingOrder(to.x, to.y) ? 1 : 0;
        }

        private void ReleaseVisual(BlockSlot slot)
        {
            if (!slot.HasVisual)
                return;

            var visual = slot.Detach();
            visual.ResetForPool();
            SharedGameObjectPool.Return(visual.gameObject);
        }

        private void ConfigureLayout()
        {
            _screenSize = new Vector2Int(Screen.width, Screen.height);
            _layout.Configure(_cameraProvider.Camera, _area.EffectiveWorldArea, _area.MaxCellSize,
                _board.Width, _board.Height);
        }

        private void BuildVisualLookup()
        {
            var entries = _catalog.Entries;
            for (var i = 0; i < entries.Count; i++)
                _visuals[entries[i].Block] = entries[i];
        }

        private BlockSlot CreateSlot(int index)
        {
            var go = new GameObject($"Block_{index}");
            go.transform.SetParent(_area.GridRoot, false);

            var slot = go.AddComponent<BlockSlot>();
            slot.Initialize(_layout, _easing.BlockMove);
            return slot;
        }
    }
}

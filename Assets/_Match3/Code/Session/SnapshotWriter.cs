using System;
using Match3.Board.Data;
using Match3.Board.Flow;

namespace Match3.Session
{
    public sealed class LevelProgress
    {
        private int _index;

        public event Action<int> Changed;

        public int Index
        {
            get => _index;
            set
            {
                if (_index == value)
                    return;

                _index = value;
                Changed?.Invoke(value);
            }
        }
    }

    public sealed class SnapshotWriter
    {
        private readonly BoardState _board;
        private readonly BlockRegistry _blocks;
        private readonly BoardSimulation _simulation;
        private readonly SaveStorage _storage;
        private readonly LevelProgress _progress;
        
        private readonly SaveData _buffer = new();

        public SnapshotWriter(BoardState board, BlockRegistry blocks, BoardSimulation simulation, SaveStorage storage,
            LevelProgress progress)
        {
            _board = board;
            _blocks = blocks;
            _simulation = simulation;
            _storage = storage;
            _progress = progress;
        }

        public void Write()
        {
            _buffer.Level = _progress.Index;
            _buffer.Moves = _simulation.Moves;
            _buffer.Blocks.Clear();

            var alive = _blocks.AliveIds;
            for (var i = 0; i < alive.Count; i++)
            {
                var id = alive[i];
                var block = _blocks.Get(id);
                if (block?.Config == null)
                    continue;

                var cell = _board.FindCell(id);
                if (cell.x < 0)
                    continue;

                _buffer.Blocks.Add(new BlockRecord
                {
                    ConfigId = block.Config.ConfigId,
                    X = cell.x,
                    Y = cell.y,
                });
            }

            _storage.Save(_buffer);
        }
    }
}

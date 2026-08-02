using System.Collections.Generic;
using Match3.Board.Data;
using Match3.Board.Operations;
using Match3.Configs;
using UnityEngine;

namespace Match3.Session
{
    public sealed class BoardBuilder
    {
        private readonly BoardState _board;
        private readonly BlockRegistry _blocks;
        private readonly OperationsLayer _operations;
        private readonly LevelSetConfig _levelSet;

        private readonly Dictionary<string, BlockConfig> _catalog = new();

        public BoardBuilder(BoardState board, BlockRegistry blocks, OperationsLayer operations,
            LevelSetConfig levelSet)
        {
            _board = board;
            _blocks = blocks;
            _operations = operations;
            _levelSet = levelSet;

            BuildCatalog();
        }

        public event System.Action Built;

        public void Initialize(BoardCapacity capacity)
        {
            _board.Initialize(capacity.Width, capacity.Height);
            _blocks.Initialize(capacity.Cells);
            _operations.Initialize(capacity.Cells);

            ValidateLevelSizes(capacity);
        }

        public void BuildFromLevel(LevelConfig level)
        {
            if (level == null)
            {
                Debug.LogError("[Match3] BoardBuilder: уровень не задан.");
                return;
            }

            Prepare(level.Width, level.Height);

            var entries = level.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.Block == null)
                    continue;

                Place(entry.Block, entry.Pos);
            }

            Finish();
        }

        public bool TryBuildFromSave(SaveData data)
        {
            var level = _levelSet.Get(data.Level);
            if (level == null)
            {
                Debug.LogError($"[Match3] В сейве уровень {data.Level}, которого нет в наборе.");
                return false;
            }

            if (!IsSaveValid(data, level))
                return false;

            Prepare(level.Width, level.Height);

            for (var i = 0; i < data.Blocks.Count; i++)
            {
                var record = data.Blocks[i];
                Place(_catalog[record.ConfigId], new Vector2Int(record.X, record.Y));
            }

            Finish();
            return true;
        }

        private bool IsSaveValid(SaveData data, LevelConfig level)
        {
            for (var i = 0; i < data.Blocks.Count; i++)
            {
                var record = data.Blocks[i];

                if (!_catalog.ContainsKey(record.ConfigId))
                {
                    Debug.LogError($"[Match3] В сейве блок '{record.ConfigId}', которого нет в каталоге уровней.");
                    return false;
                }

                if (record.X < 0 || record.X >= level.Width || record.Y < 0 || record.Y >= level.Height)
                {
                    Debug.LogError($"[Match3] В сейве блок '{record.ConfigId}' в {record.X}:{record.Y}, " +
                                   $"а уровень {level.Width}x{level.Height}.");
                    return false;
                }
            }

            return true;
        }

        private void Prepare(int width, int height)
        {
            _operations.Clear();
            _blocks.Clear();

            _board.SetBounds(width, height);
        }

        private void ValidateLevelSizes(BoardCapacity capacity)
        {
            var levels = _levelSet.Levels;
            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                if (level == null)
                    continue;

                if (level.Width > capacity.Width || level.Height > capacity.Height)
                    Debug.LogError($"[Match3] Уровень '{level.name}' — {level.Width}x{level.Height}, " +
                                   $"это больше MaxBoardSize {capacity.Width}x{capacity.Height} " +
                                   "на объекте BoardArea в сцене.");
            }
        }

        private void Place(BlockConfig config, Vector2Int pos)
        {
            if (!_board.InBounds(pos))
            {
                Debug.LogError($"[Match3] Блок '{config.ConfigId}' в позиции {pos} вне поля.");
                return;
            }

            var block = _blocks.Spawn(config);
            if (block == null)
                return;

            _board.PlaceBlock(block.Id, pos);
        }

        private void Finish() => Built?.Invoke();

        private void BuildCatalog()
        {
            var levels = _levelSet.Levels;
            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                if (level == null)
                    continue;

                var entries = level.Entries;
                for (var e = 0; e < entries.Count; e++)
                {
                    var config = entries[e].Block;
                    if (config == null)
                        continue;

                    if (_catalog.TryGetValue(config.ConfigId, out var existing) && existing != config)
                    {
                        Debug.LogError($"[Match3] Каталог блоков: ConfigId '{config.ConfigId}' занят двумя ассетами " +
                                       $"('{existing.name}' и '{config.name}').");
                        continue;
                    }

                    _catalog[config.ConfigId] = config;
                }
            }
        }
    }
}

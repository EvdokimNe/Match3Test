using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3.Configs
{
    [CreateAssetMenu(fileName = "Level", menuName = "Match3/Level")]
    public sealed class LevelConfig : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public Vector2Int Pos;
            public BlockConfig Block;
        }

        [SerializeField] private int _width = 6;
        [SerializeField] private int _height = 8;
        [SerializeField] private List<Entry> _entries = new();

        public int Width => _width;
        public int Height => _height;
        public IReadOnlyList<Entry> Entries => _entries;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_width <= 0 || _height <= 0)
                Debug.LogError($"[Match3] LevelConfig '{name}': размер поля должен быть положительным.", this);

            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];

                if (entry.Block == null)
                {
                    Debug.LogError($"[Match3] LevelConfig '{name}': запись #{i} без BlockConfig.", this);
                    continue;
                }

                if (entry.Pos.x < 0 || entry.Pos.x >= _width || entry.Pos.y < 0 || entry.Pos.y >= _height)
                    Debug.LogError($"[Match3] LevelConfig '{name}': запись #{i} с позицией {entry.Pos} вне поля {_width}x{_height}.", this);

                for (var j = i + 1; j < _entries.Count; j++)
                {
                    if (_entries[j].Pos == entry.Pos)
                        Debug.LogError($"[Match3] LevelConfig '{name}': дубль позиции {entry.Pos} в записях #{i} и #{j}.", this);
                }
            }
        }
#endif
    }
}

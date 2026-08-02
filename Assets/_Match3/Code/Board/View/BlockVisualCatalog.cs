using System;
using System.Collections.Generic;
using Match3.Configs;
using UnityEngine;

namespace Match3.Board.View
{
    [CreateAssetMenu(fileName = "BlockVisualCatalog", menuName = "Match3/Block Visual Catalog")]
    public sealed class BlockVisualCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public BlockConfig Block;
            public BlockVisual Prefab;
            public Color DebugColor;
        }

        [SerializeField] private List<Entry> _entries = new();

        public IReadOnlyList<Entry> Entries => _entries;

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];

                if (entry.Block == null)
                    Debug.LogError($"[Match3] BlockVisualCatalog '{name}': в записи #{i} не назначен BlockConfig.", this);

                if (entry.Prefab == null)
                    Debug.LogError($"[Match3] BlockVisualCatalog '{name}': в записи #{i} не назначен префаб визуала.", this);

                for (var j = i + 1; j < _entries.Count; j++)
                {
                    if (_entries[j].Block == entry.Block && entry.Block != null)
                        Debug.LogError($"[Match3] BlockVisualCatalog '{name}': блок '{entry.Block.name}' указан дважды.", this);
                }
            }
        }
#endif
    }
}

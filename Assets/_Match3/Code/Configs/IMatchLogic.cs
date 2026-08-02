using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3.Configs
{
    public interface IMatchLogic
    {
        bool Matches(BlockTypeConfig self, BlockTypeConfig other);
    }

    [Serializable]
    public sealed class EqualsTypeMatchLogic : IMatchLogic
    {
        public bool Matches(BlockTypeConfig self, BlockTypeConfig other) => self == other;
    }

    [Serializable]
    public sealed class CustomTypeMatchLogic : IMatchLogic
    {
        [Tooltip("С какими чужими типами этот тип тоже сливается. Свой тип совпадает всегда")]
        [SerializeField] private List<BlockTypeConfig> _alsoMatches = new();

        public bool Matches(BlockTypeConfig self, BlockTypeConfig other) =>
            self == other || _alsoMatches.Contains(other);
    }
}

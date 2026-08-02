using System.Collections.Generic;
using UnityEngine;

namespace Match3.Configs
{
    [CreateAssetMenu(fileName = "LevelSet", menuName = "Match3/Level Set")]
    public sealed class LevelSetConfig : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> _levels = new();

        public IReadOnlyList<LevelConfig> Levels => _levels;
        public int Count => _levels.Count;

        public LevelConfig Get(int index) =>
            index >= 0 && index < _levels.Count ? _levels[index] : null;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_levels.Count == 0)
                Debug.LogError($"[Match3] LevelSetConfig '{name}': список уровней пуст.", this);
        }
#endif
    }
}

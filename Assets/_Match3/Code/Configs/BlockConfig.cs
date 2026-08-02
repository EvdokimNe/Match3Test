using System.Collections.Generic;
using UnityEngine;

namespace Match3.Configs
{
    [CreateAssetMenu(fileName = "Block", menuName = "Match3/Block")]
    public sealed class BlockConfig : ScriptableObject
    {
        [SerializeField] private string _configId = "fire";
        [SerializeField] private BlockTypeConfig _type;
        [SerializeReference] private List<IBlockRule> _rules = new();

        public string ConfigId => _configId;
        public BlockTypeConfig Type => _type;
        public List<IBlockRule> Rules => _rules;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_type == null)
                Debug.LogError($"[Match3] BlockConfig '{name}': не назначен Type (BlockTypeConfig).", this);

            if (string.IsNullOrWhiteSpace(_configId))
                Debug.LogError($"[Match3] BlockConfig '{name}': пустой ConfigId — по нему идёт восстановление сейва.", this);

            for (var i = 0; i < _rules.Count; i++)
            {
                if (_rules[i] == null)
                    Debug.LogError($"[Match3] BlockConfig '{name}': в правиле #{i} не выбран тип.", this);
            }
        }
#endif
    }
}

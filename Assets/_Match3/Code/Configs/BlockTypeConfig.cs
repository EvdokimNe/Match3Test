using Match3.Board.Matching;
using UnityEngine;

namespace Match3.Configs
{
    [CreateAssetMenu(fileName = "BlockType", menuName = "Match3/Block Type")]
    public sealed class BlockTypeConfig : ScriptableObject
    {
        [SerializeReference] private IMatchLogic _matchLogic = new EqualsTypeMatchLogic();
        [SerializeReference] private IDestructionTrigger _trigger = new LineTrigger();
        [SerializeReference] private IDestructionExpansion _expansion = new FirstOrderNeighbors();

        public IMatchLogic MatchLogic => _matchLogic;
        public IDestructionTrigger Trigger => _trigger;
        public IDestructionExpansion Expansion => _expansion;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_matchLogic == null)
                Debug.LogError($"[Match3] BlockTypeConfig '{name}': не выбрана MatchLogic.", this);

            if (_trigger == null)
                Debug.LogError($"[Match3] BlockTypeConfig '{name}': не выбран Trigger.", this);
        }
#endif
    }
}

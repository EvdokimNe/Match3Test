using System.Collections.Generic;
using UnityEngine;

namespace Match3.Board.Matching
{
    public interface IDestructionExpansion
    {
        void Expand(MatchContext context, List<Vector2Int> seed, HashSet<Vector2Int> result);
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Match3.Board.Matching
{
    public interface IDestructionTrigger
    {
        bool TryTrigger(MatchContext context, Vector2Int cell, List<Vector2Int> seed);
    }
}

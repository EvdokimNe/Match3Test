using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3.Board.Matching
{
    [Serializable]
    public sealed class LineTrigger : IDestructionTrigger
    {
        public bool TryTrigger(MatchContext context, Vector2Int cell, List<Vector2Int> seed) =>
            context.Lines.TryFindLines(cell, seed);
    }
}

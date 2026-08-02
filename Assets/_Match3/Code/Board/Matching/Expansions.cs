using System;
using System.Collections.Generic;
using Match3.Board.Data;
using UnityEngine;

namespace Match3.Board.Matching
{
    [Serializable]
    public sealed class NoExpansion : IDestructionExpansion
    {
        public void Expand(MatchContext context, List<Vector2Int> seed, HashSet<Vector2Int> result)
        {
            for (var i = 0; i < seed.Count; i++)
                result.Add(seed[i]);
        }
    }

    [Serializable]
    public sealed class FirstOrderNeighbors : IDestructionExpansion
    {
        private static readonly MoveDirection[] Directions =
        {
            MoveDirection.Up, MoveDirection.Down, MoveDirection.Left, MoveDirection.Right,
        };

        public void Expand(MatchContext context, List<Vector2Int> seed, HashSet<Vector2Int> result)
        {
            for (var i = 0; i < seed.Count; i++)
                result.Add(seed[i]);

            for (var i = 0; i < seed.Count; i++)
            {
                var cell = seed[i];
                if (!context.TryGetMatchable(cell, out var block))
                    continue;

                for (var d = 0; d < Directions.Length; d++)
                {
                    var direction = Directions[d];
                    var neighbourCell = cell + direction.ToOffset();

                    if (!context.TryGetMatchable(neighbourCell, out var neighbour))
                        continue;

                    if (MergeRules.CanMerge(block, neighbour, direction))
                        result.Add(neighbourCell);
                }
            }
        }
    }

    [Serializable]
    public sealed class ConnectedAreaExpansion : IDestructionExpansion
    {
        private static readonly MoveDirection[] Directions =
        {
            MoveDirection.Up, MoveDirection.Down, MoveDirection.Left, MoveDirection.Right,
        };

        private readonly Queue<Vector2Int> _frontier = new();

        public void Expand(MatchContext context, List<Vector2Int> seed, HashSet<Vector2Int> result)
        {
            _frontier.Clear();

            for (var i = 0; i < seed.Count; i++)
            {
                if (result.Add(seed[i]))
                    _frontier.Enqueue(seed[i]);
            }

            while (_frontier.Count > 0)
            {
                var cell = _frontier.Dequeue();
                if (!context.TryGetMatchable(cell, out var block))
                    continue;

                for (var d = 0; d < Directions.Length; d++)
                {
                    var direction = Directions[d];
                    var neighbourCell = cell + direction.ToOffset();

                    if (result.Contains(neighbourCell))
                        continue;

                    if (!context.TryGetMatchable(neighbourCell, out var neighbour))
                        continue;

                    if (!MergeRules.CanMerge(block, neighbour, direction))
                        continue;

                    result.Add(neighbourCell);
                    _frontier.Enqueue(neighbourCell);
                }
            }
        }
    }
}

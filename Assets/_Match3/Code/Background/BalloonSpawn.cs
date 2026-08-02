using UnityEngine;

namespace Match3.Background
{
    public readonly struct BalloonSpawn
    {
        public readonly Vector2 Origin;
        public readonly float DirectionX;
        public readonly float Variation;

        public BalloonSpawn(Vector2 origin, float directionX, float variation)
        {
            Origin = origin;
            DirectionX = directionX;
            Variation = variation;
        }
    }
}

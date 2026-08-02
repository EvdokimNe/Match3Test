using UnityEngine;

namespace Match3.Background
{
    public interface IBalloonMotion
    {
        Vector2 Evaluate(in BalloonSpawn spawn, float time);
    }
}

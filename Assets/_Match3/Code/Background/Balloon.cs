namespace Match3.Background
{
    public sealed class Balloon
    {
        public BalloonView View;
        public BalloonSpawn Spawn;
        public IBalloonMotion Motion;
        public float Elapsed;
    }
}

using System;
using UnityEngine;

namespace Match3.Background
{
    [Serializable]
    public sealed class SineMotion : IBalloonMotion
    {
        [Tooltip("Горизонтальная скорость, мин-макс")]
        [SerializeField] private Vector2 _speed = new(1.2f, 2.8f);
        [Tooltip("Размах по вертикали в мировых единицах, мин-макс")]
        [SerializeField] private Vector2 _amplitude = new(0.4f, 1.1f);
        [Tooltip("Частота колебаний, мин-макс")]
        [SerializeField] private Vector2 _frequency = new(0.7f, 1.6f);

        public Vector2 Evaluate(in BalloonSpawn spawn, float time)
        {
            var speed = Mathf.Lerp(_speed.x, _speed.y, spawn.Variation);
            var amplitude = Mathf.Lerp(_amplitude.x, _amplitude.y, spawn.Variation);
            var frequency = Mathf.Lerp(_frequency.x, _frequency.y, spawn.Variation);

            return new Vector2(
                spawn.DirectionX * speed * time,
                amplitude * Mathf.Sin(frequency * time));
        }
    }

    [Serializable]
    public sealed class DiagonalSineMotion : IBalloonMotion
    {
        [Tooltip("Горизонтальная скорость, мин-макс")]
        [SerializeField] private Vector2 _speed = new(1f, 2.2f);
        [Tooltip("Наклон полёта: доля от горизонтальной скорости, мин-макс")]
        [SerializeField] private Vector2 _slope = new(-0.3f, 0.3f);
        [Tooltip("Размах по вертикали в мировых единицах, мин-макс")]
        [SerializeField] private Vector2 _amplitude = new(0.2f, 0.6f);
        [Tooltip("Частота колебаний, мин-макс")]
        [SerializeField] private Vector2 _frequency = new(0.9f, 2f);

        public Vector2 Evaluate(in BalloonSpawn spawn, float time)
        {
            var speed = Mathf.Lerp(_speed.x, _speed.y, spawn.Variation);
            var slope = Mathf.Lerp(_slope.x, _slope.y, spawn.Variation);
            var amplitude = Mathf.Lerp(_amplitude.x, _amplitude.y, spawn.Variation);
            var frequency = Mathf.Lerp(_frequency.x, _frequency.y, spawn.Variation);

            var travelled = speed * time;

            return new Vector2(
                spawn.DirectionX * travelled,
                slope * travelled + amplitude * Mathf.Sin(frequency * time));
        }
    }
}

using UnityEngine;

namespace Match3.Configs
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Match3/Gameplay Config")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [Header("Тайминги логики")]
        [Tooltip("Длительность Move и Swap, сек")]
        [SerializeField] private float _moveTime = 0.15f;
        [Tooltip("Длительность Destroy, сек")]
        [SerializeField] private float _destroyTime = 0.25f;
        [Tooltip("Пауза между очисткой поля и стартом следующего уровня, сек")]
        [SerializeField] private float _levelTransitionSeconds = 0.5f;

        [Header("Падение по умолчанию")]
        [Tooltip("Стартовая скорость падения, клеток/с")]
        [SerializeField] private float _fallSpeed = 6f;
        [Tooltip("Ускорение падения, клеток/с². Ноль — равномерное падение")]
        [SerializeField] private float _fallAccel = 20f;
        [Tooltip("Потолок скорости падения, клеток/с")]
        [SerializeField] private float _maxFallSpeed = 18f;

        [Header("Ввод")]
        [Tooltip("Порог свайпа в пикселях")]
        [SerializeField] private float _swipeThresholdPixels = 24f;

        public float MoveTime => _moveTime;
        public float DestroyTime => _destroyTime;
        public float LevelTransitionSeconds => _levelTransitionSeconds;
        public float FallSpeed => _fallSpeed;
        public float FallAccel => _fallAccel;
        public float MaxFallSpeed => _maxFallSpeed;
        public float SwipeThresholdPixels => _swipeThresholdPixels;
    }
}

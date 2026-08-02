using UnityEngine;

namespace Match3.Configs
{
    [CreateAssetMenu(fileName = "EasingConfig", menuName = "Match3/Easing Config")]
    public sealed class EasingConfig : ScriptableObject
    {
        [Tooltip("Переезд и обмен. Логическую длительность задаёт GameplayConfig, здесь только форма движения")]
        [SerializeField] private AnimationCurve _blockMove = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public AnimationCurve BlockMove => _blockMove;
    }
}

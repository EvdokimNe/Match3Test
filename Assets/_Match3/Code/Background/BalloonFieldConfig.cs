using System.Collections.Generic;
using UnityEngine;

namespace Match3.Background
{
    [CreateAssetMenu(fileName = "BalloonFieldConfig", menuName = "Match3/Balloon Field Config")]
    public sealed class BalloonFieldConfig : ScriptableObject
    {
        [Tooltip("Сколько шаров одновременно в кадре")]
        [SerializeField] private int _count = 3;
        [Tooltip("Насколько далеко за краем экрана шар появляется и исчезает")]
        [SerializeField] private float _offscreenMargin = 1.5f;
        [Tooltip("Порядок отрисовки — должен быть ниже, чем у блоков")]
        [SerializeField] private int _sortingOrder = -100;

        [SerializeReference] private List<IBalloonMotion> _motions = new()
        {
            new SineMotion(),
            new DiagonalSineMotion(),
        };

        public int Count => _count;
        public float OffscreenMargin => _offscreenMargin;
        public int SortingOrder => _sortingOrder;
        public IReadOnlyList<IBalloonMotion> Motions => _motions;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_count <= 0)
                Debug.LogError($"[Match3] BalloonFieldConfig '{name}': количество шаров должно быть положительным.", this);

            if (_offscreenMargin <= 0f)
                Debug.LogError($"[Match3] BalloonFieldConfig '{name}': запас за краем экрана должен быть положительным.", this);

            if (_motions.Count == 0)
                Debug.LogError($"[Match3] BalloonFieldConfig '{name}': не задано ни одного типа движения.", this);

            for (var i = 0; i < _motions.Count; i++)
            {
                if (_motions[i] == null)
                    Debug.LogError($"[Match3] BalloonFieldConfig '{name}': в движении #{i} не выбран тип.", this);
            }
        }
#endif
    }
}

using UnityEngine;

namespace Match3.Board.View
{
    [CreateAssetMenu(fileName = "SpriteAnimation", menuName = "Match3/Sprite Animation")]
    public sealed class SpriteAnimation : ScriptableObject
    {
        [SerializeField] private Sprite[] _frames;

        public int FrameCount => _frames.Length;

        public Sprite FrameAt(int index) => _frames[index];

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_frames.Length == 0)
                Debug.LogError($"[Match3] SpriteAnimation '{name}': не задано ни одного кадра.", this);
        }
#endif
    }
}

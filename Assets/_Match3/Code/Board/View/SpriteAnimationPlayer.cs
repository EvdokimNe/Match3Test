using UnityEngine;

namespace Match3.Board.View
{
    public sealed class SpriteAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Скорость зациклённых анимаций. Разрушение сюда не смотрит — оно идёт по прогрессу операции")]
        [SerializeField] private float _loopFps = 12f;

        private SpriteAnimation _animation;
        private float _time;
        private int _frameIndex = -1;

        public void PlayLoop(SpriteAnimation animation, float startTime)
        {
            SetAnimation(animation);
            _time = startTime;
            ApplyFrame(LoopFrameIndex());
        }

        public void Advance(float deltaTime)
        {
            _time += deltaTime;
            ApplyFrame(LoopFrameIndex());
        }

        public void Sample(SpriteAnimation animation, float progress)
        {
            SetAnimation(animation);

            var lastIndex = animation.FrameCount - 1;
            ApplyFrame(Mathf.Clamp(Mathf.FloorToInt(progress * animation.FrameCount), 0, lastIndex));
        }

        private void SetAnimation(SpriteAnimation animation)
        {
            if (_animation == animation)
                return;

            _animation = animation;
            _frameIndex = -1;
        }

        private int LoopFrameIndex() =>
            Mathf.FloorToInt(_time * _loopFps) % _animation.FrameCount;

        private void ApplyFrame(int index)
        {
            if (index == _frameIndex)
                return;

            _frameIndex = index;
            _renderer.sprite = _animation.FrameAt(index);
        }

#if UNITY_EDITOR
        private void Reset() => _renderer = GetComponent<SpriteRenderer>();
#endif
    }
}

using LitMotion;
using LitMotion.Extensions;
using Match3.Board.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Match3.Board.View
{
    public sealed class BlockVisual : MonoBehaviour
    {
        [Tooltip("Порядок блока на поле. Внутри блока рендереры сортируются своими sortingOrder")]
        [SerializeField] private SortingGroup _group;
        [Tooltip("Сюда бьют твины. Мировую позицию задаёт слот, этот трансформ только локальный")]
        [SerializeField] private Transform _animationRoot;
        [SerializeField] private SpriteAnimationPlayer _player;

        [Header("Анимации")]
        [SerializeField] private SpriteAnimation _idle;
        [SerializeField] private SpriteAnimation _destruction;

        [Header("Отдача при отклонённом ходе")]
        [SerializeField] private float _rejectStrength = 0.15f;
        [SerializeField] private float _rejectDuration = 0.25f;

        private MotionHandle _rejectHandle;
        private bool _destroying;

        public int SortingOrder
        {
            set => _group.sortingOrder = value;
        }

        public void Show(float animationPhase)
        {
            _destroying = false;
            ResetAnimationRoot();
            _player.PlayLoop(_idle, animationPhase);
        }

        public void Advance(float deltaTime)
        {
            if (_destroying)
                return;

            _player.Advance(deltaTime);
        }

        public void SampleDestroy(float progress)
        {
            _destroying = true;
            _player.Sample(_destruction, progress);
        }

        public void PlayRejected(MoveDirection direction)
        {
            _rejectHandle.TryCancel();

            var strength = (Vector3)(Vector2)direction.ToOffset() * _rejectStrength;

            _rejectHandle = LMotion.Punch.Create(Vector3.zero, strength, _rejectDuration)
                .BindToLocalPosition(_animationRoot);
        }

        public void ResetForPool()
        {
            _rejectHandle.TryCancel();
            _destroying = false;
            ResetAnimationRoot();
        }

        private void ResetAnimationRoot()
        {
            _animationRoot.localPosition = Vector3.zero;
            _animationRoot.localRotation = Quaternion.identity;
            _animationRoot.localScale = Vector3.one;
        }
    }
}

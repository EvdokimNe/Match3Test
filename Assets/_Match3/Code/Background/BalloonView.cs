using UnityEngine;

namespace Match3.Background
{
    public sealed class BalloonView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;

        public void SetSortingOrder(int sortingOrder) => _renderer.sortingOrder = sortingOrder;

        public void SetPosition(Vector2 position) => transform.position = position;

#if UNITY_EDITOR
        private void Reset() => _renderer = GetComponentInChildren<SpriteRenderer>();
#endif
    }
}

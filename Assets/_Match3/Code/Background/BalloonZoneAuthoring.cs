using System.Collections.Generic;
using Match3.Scene;
using UnityEngine;

namespace Match3.Background
{
    public sealed class BalloonZoneAuthoring : MonoBehaviour
    {
#if UNITY_EDITOR
        private static readonly Color ZoneGizmoColor = new(0.4f, 0.8f, 1f, 1f);

        [Tooltip("Нужен только гизмо: границы в рантайме считает BalloonField, он берёт камеру из контейнера")]
        [SerializeField] private MainCameraSceneProvider _cameraProvider;
#endif

        [Header("Зона")]
        [Tooltip("Высота полосы, в которой рождаются шары. Центр по вертикали — позиция этого объекта")]
        [SerializeField] private float _height = 4f;

        [Header("Содержимое")]
        [Tooltip("Виды шаров. Раздаются по кругу, поэтому в небе гарантированно есть каждый")]
        [SerializeField] private List<BalloonView> _prefabs = new();
        [SerializeField] private BalloonFieldConfig _config;

        public IReadOnlyList<BalloonView> Prefabs => _prefabs;
        public BalloonFieldConfig Config => _config;

        public float BottomY => transform.position.y - _height * 0.5f;
        public float TopY => transform.position.y + _height * 0.5f;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_cameraProvider == null || _cameraProvider.Camera == null)
                return;

            var camera = _cameraProvider.Camera;
            var halfWidth = camera.orthographicSize * camera.aspect;
            var centerX = camera.transform.position.x;

            var left = centerX - halfWidth;
            var right = centerX + halfWidth;

            var bottomLeft = new Vector3(left, BottomY);
            var bottomRight = new Vector3(right, BottomY);
            var topRight = new Vector3(right, TopY);
            var topLeft = new Vector3(left, TopY);

            Gizmos.color = ZoneGizmoColor;

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
#endif
    }
}

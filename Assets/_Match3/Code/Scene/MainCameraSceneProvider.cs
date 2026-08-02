using UnityEngine;

namespace Match3.Scene
{
    public sealed class MainCameraSceneProvider : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public Camera Camera => _camera;

#if UNITY_EDITOR
        private void Reset() => _camera = Camera.main;
#endif
    }
}

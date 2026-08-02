using Match3.Background;
using Match3.Board.Debugging;
using Match3.Board.View;
using Match3.Scene;
using Match3.Session;
using UnityEngine;

namespace Match3.Composition
{
    public sealed class Match3SceneContext : MonoBehaviour
    {
        [SerializeField] private MainCameraSceneProvider _cameraProvider;
        [SerializeField] private BoardAreaAuthoring _boardArea;
        [SerializeField] private BalloonZoneAuthoring _balloonZone;
        [SerializeField] private GridDebugView _debugView;
        [SerializeField] private LevelSwitchView _levelSwitch;

        public MainCameraSceneProvider CameraProvider => _cameraProvider;
        public BoardAreaAuthoring BoardArea => _boardArea;
        public BalloonZoneAuthoring BalloonZone => _balloonZone;
        public GridDebugView DebugView => _debugView;
        public LevelSwitchView LevelSwitch => _levelSwitch;

#if UNITY_EDITOR
        private void Reset()
        {
            _cameraProvider = FindAnyObjectByType<MainCameraSceneProvider>();
            _boardArea = FindAnyObjectByType<BoardAreaAuthoring>();
            _balloonZone = FindAnyObjectByType<BalloonZoneAuthoring>();
            _debugView = FindAnyObjectByType<GridDebugView>();
            _levelSwitch = FindAnyObjectByType<LevelSwitchView>();
        }
#endif
    }
}

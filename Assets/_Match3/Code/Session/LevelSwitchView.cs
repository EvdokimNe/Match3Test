using Match3.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Match3.Session
{
    public sealed class LevelSwitchView : MonoBehaviour
    {
        [SerializeField] private Button _previous;
        [SerializeField] private Button _next;
        [SerializeField] private Button _restart;
        [SerializeField] private TMP_Text _label;

        private SessionController _session;
        private LevelProgress _progress;
        private LevelSetConfig _levelSet;

        [Inject]
        public void Construct(SessionController session, LevelProgress progress, LevelSetConfig levelSet)
        {
            _session = session;
            _progress = progress;
            _levelSet = levelSet;

            _previous.onClick.AddListener(_session.GoToPreviousLevel);
            _next.onClick.AddListener(_session.GoToNextLevel);
            _restart.onClick.AddListener(_session.RestartLevel);

            _progress.Changed += Redraw;
            Redraw(_progress.Index);
        }

        private void OnDestroy() => _progress.Changed -= Redraw;

        private void Redraw(int index) => _label.text = $"{index + 1} / {_levelSet.Count}";
    }
}

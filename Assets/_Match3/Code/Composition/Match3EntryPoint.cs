using Match3.Board.View;
using Match3.Session;
using VContainer.Unity;

namespace Match3.Composition
{
    public sealed class Match3EntryPoint : IStartable
    {
        private readonly SessionController _session;
        private readonly BoardBuilder _boardBuilder;
        private readonly GridView _gridView;
        private readonly BoardAreaAuthoring _area;

        public Match3EntryPoint(SessionController session, BoardBuilder boardBuilder, GridView gridView,
            BoardAreaAuthoring area)
        {
            _session = session;
            _boardBuilder = boardBuilder;
            _gridView = gridView;
            _area = area;
        }

        public void Start()
        {
            _boardBuilder.Initialize(_area.Capacity);
            _gridView.Initialize();
            _boardBuilder.Built += _gridView.Rebuild;
            _session.StartFromSaveOrFirstLevel();
        }
    }
}

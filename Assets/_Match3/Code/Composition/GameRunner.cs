using Match3.Board.Flow;
using Match3.Board.Input;
using Match3.Board.View;
using Match3.Session;
using UnityEngine;
using VContainer.Unity;

namespace Match3.Composition
{
    public sealed class GameRunner : ITickable
    {
        private readonly InputRouter _input;
        private readonly BoardSimulation _simulation;
        private readonly GridView _gridView;
        private readonly SessionController _session;

        public GameRunner(InputRouter input, BoardSimulation simulation, GridView gridView, SessionController session)
        {
            _input = input;
            _simulation = simulation;
            _gridView = gridView;
            _session = session;
        }

        public void Tick()
        {
            var deltaTime = Time.deltaTime;

            _input.Read();
            var result = _simulation.Tick(deltaTime);
            _gridView.Draw(deltaTime);

            if (result == BoardTickResult.BecameStable)
                _session.OnBoardBecameStable();
        }
    }
}

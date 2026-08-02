using System.Collections.Generic;
using Match3.Board.Flow;
using Match3.Board.View;

namespace Match3.Board.Input
{
    public sealed class InputRouter
    {
        private readonly BoardSimulation _simulation;
        private readonly BoardLayout _layout;
        private readonly IReadOnlyList<IInputMode> _modes;

        private int _activeIndex;

        public InputRouter(BoardSimulation simulation, BoardLayout layout, IReadOnlyList<IInputMode> modes)
        {
            _simulation = simulation;
            _layout = layout;
            _modes = modes;
        }

        public IInputMode Active => _modes[_activeIndex];
        public int ModeCount => _modes.Count;

        public void SwitchToNextMode()
        {
            Active.Cancel();
            _activeIndex = (_activeIndex + 1) % _modes.Count;
        }

        public void Read()
        {
            if (Active.TryRead(_layout, out var gesture))
                _simulation.RequestMove(gesture.From, gesture.Direction);
        }
    }
}

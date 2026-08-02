using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Match3.Board.Data;
using Match3.Board.Flow;
using Match3.Configs;
using UnityEngine;

namespace Match3.Session
{
    public sealed class SessionController : IDisposable
    {
        private readonly BoardState _board;
        private readonly BoardSimulation _simulation;
        private readonly BoardBuilder _builder;
        private readonly SaveStorage _storage;
        private readonly LevelSetConfig _levelSet;
        private readonly GameplayConfig _gameplay;
        private readonly LevelProgress _progress;
        private readonly SnapshotWriter _snapshotWriter;

        private CancellationTokenSource _transition;

        public SessionController(BoardState board, BoardSimulation simulation, BoardBuilder builder,
            SaveStorage storage, LevelSetConfig levelSet, GameplayConfig gameplay, LevelProgress progress,
            SnapshotWriter snapshotWriter)
        {
            _board = board;
            _simulation = simulation;
            _builder = builder;
            _storage = storage;
            _levelSet = levelSet;
            _gameplay = gameplay;
            _progress = progress;
            _snapshotWriter = snapshotWriter;
        }


        public void Dispose() => CancelTransition();

        public void OnBoardBecameStable()
        {
            if (_board.BlockCount == 0)
                AdvanceLevelAsync().Forget();
            else
                _snapshotWriter.Write();
        }

        public void StartFromSaveOrFirstLevel()
        {
            if (_storage.TryLoad(out var data) && _builder.TryBuildFromSave(data))
            {
                _progress.Index = data.Level;
                _simulation.ResetSession(data.Moves);
                return;
            }

            StartLevel(0);
        }

        public async UniTask AdvanceLevelAsync()
        {
            CancelTransition();
            _transition = new CancellationTokenSource();

            var delay = TimeSpan.FromSeconds(_gameplay.LevelTransitionSeconds);
            await UniTask.Delay(delay, cancellationToken: _transition.Token);

            StartLevel(NextLevelIndex());
        }

        public void StartLevel(int index)
        {
            var level = _levelSet.Get(index);
            if (level == null)
            {
                Debug.LogError($"[Match3] Уровень #{index} отсутствует в LevelSetConfig.");
                return;
            }

            CancelTransition();

            _progress.Index = index;
            _builder.BuildFromLevel(level);
            _simulation.ResetSession(0);
            _snapshotWriter.Write();
        }

        public void RestartLevel() => StartLevel(_progress.Index);

        public void GoToNextLevel() => StartLevel(NextLevelIndex());

        public void GoToPreviousLevel() => StartLevel(PreviousLevelIndex());

        public void ResetProgress()
        {
            _storage.Delete();
            StartLevel(0);
        }

        private int NextLevelIndex()
        {
            var next = _progress.Index + 1;
            return next >= _levelSet.Count ? 0 : next;
        }

        private int PreviousLevelIndex()
        {
            var previous = _progress.Index - 1;
            return previous < 0 ? _levelSet.Count - 1 : previous;
        }

        private void CancelTransition()
        {
            if (_transition == null)
                return;

            _transition.Cancel();
            _transition.Dispose();
            _transition = null;
        }
    }
}

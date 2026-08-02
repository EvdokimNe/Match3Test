using Match3.Background;
using Match3.Board.Data;
using Match3.Board.Flow;
using Match3.Board.Input;
using Match3.Board.Matching;
using Match3.Board.Operations;
using Match3.Board.Validation;
using Match3.Board.View;
using Match3.Configs;
using Match3.Session;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Match3.Composition
{
    public sealed class Match3Scope : LifetimeScope
    {
        [Header("Сцена")]
        [SerializeField] private Match3SceneContext _sceneContext;

        [Header("Конфиги")]
        [SerializeField] private GameplayConfig _gameplayConfig;
        [SerializeField] private LevelSetConfig _levelSet;
        [SerializeField] private BlockVisualCatalog _blockVisuals;
        [SerializeField] private EasingConfig _easing;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterScene(builder);
            RegisterConfigs(builder);
            RegisterLogic(builder);
            RegisterView(builder);
            RegisterBackground(builder);

            builder.RegisterEntryPoint<Match3EntryPoint>();
        }

        private void RegisterScene(IContainerBuilder builder)
        {
            builder.RegisterComponent(_sceneContext.CameraProvider);
            builder.RegisterComponent(_sceneContext.BoardArea);
            builder.RegisterComponent(_sceneContext.BalloonZone);
            builder.RegisterComponent(_sceneContext.DebugView);
            builder.RegisterComponent(_sceneContext.LevelSwitch);
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameplayConfig);
            builder.RegisterInstance(_levelSet);
            builder.RegisterInstance(_blockVisuals);
            builder.RegisterInstance(_easing);
        }

        private static void RegisterLogic(IContainerBuilder builder)
        {
            builder.Register<BoardState>(Lifetime.Singleton);
            builder.Register<BlockRegistry>(Lifetime.Singleton);
            builder.Register<OperationsLayer>(Lifetime.Singleton);
            builder.Register<ActionValidator>(Lifetime.Singleton);
            builder.Register<MatchContext>(Lifetime.Singleton);
            builder.Register<DestructionResolver>(Lifetime.Singleton);
            builder.Register<BoardSimulation>(Lifetime.Singleton);

            builder.Register<SaveStorage>(Lifetime.Singleton);
            builder.Register<LevelProgress>(Lifetime.Singleton);
            builder.Register<BoardBuilder>(Lifetime.Singleton);
            builder.Register<SnapshotWriter>(Lifetime.Singleton);
            builder.Register<SessionController>(Lifetime.Singleton);
        }

        private static void RegisterView(IContainerBuilder builder)
        {
            builder.Register<BoardLayout>(Lifetime.Singleton);
            builder.Register<GridView>(Lifetime.Singleton);

            builder.Register<SwipeInputMode>(Lifetime.Singleton);
            builder.Register(resolver => new InputRouter(
                    resolver.Resolve<BoardSimulation>(),
                    resolver.Resolve<BoardLayout>(),
                    new IInputMode[] { resolver.Resolve<SwipeInputMode>() }),
                Lifetime.Singleton);

            builder.RegisterEntryPoint<GameRunner>();
        }

        private static void RegisterBackground(IContainerBuilder builder) =>
            builder.RegisterEntryPoint<BalloonField>();
    }
}

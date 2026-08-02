using System.Collections.Generic;
using Match3.Scene;
using UnityEngine;
using VContainer.Unity;

namespace Match3.Background
{
    public sealed class BalloonField : IStartable, ITickable
    {
        private readonly BalloonZoneAuthoring _zone;
        private readonly MainCameraSceneProvider _cameraProvider;
        private readonly List<Balloon> _balloons = new(4);

        private float _leftEdge;
        private float _rightEdge;

        public BalloonField(BalloonZoneAuthoring zone, MainCameraSceneProvider cameraProvider)
        {
            _zone = zone;
            _cameraProvider = cameraProvider;
        }

        public void Start()
        {
            var camera = _cameraProvider.Camera;
            var halfWidth = camera.orthographicSize * camera.aspect;
            var centerX = camera.transform.position.x;

            _leftEdge = centerX - halfWidth;
            _rightEdge = centerX + halfWidth;

            var config = _zone.Config;
            var prefabs = _zone.Prefabs;

            for (var i = 0; i < config.Count; i++)
            {
                var view = Object.Instantiate(prefabs[i % prefabs.Count], _zone.transform);
                view.SetSortingOrder(config.SortingOrder);

                var balloon = new Balloon { View = view };
                _balloons.Add(balloon);
                Respawn(balloon);
            }
        }

        public void Tick()
        {
            var deltaTime = Time.deltaTime;
            var margin = _zone.Config.OffscreenMargin;

            for (var i = 0; i < _balloons.Count; i++)
            {
                var balloon = _balloons[i];
                balloon.Elapsed += deltaTime;

                var position = balloon.Spawn.Origin + balloon.Motion.Evaluate(balloon.Spawn, balloon.Elapsed);
                balloon.View.SetPosition(position);

                if (IsFullyOffscreen(position.x, balloon.Spawn.DirectionX, margin))
                    Respawn(balloon);
            }
        }

        private void Respawn(Balloon balloon)
        {
            var config = _zone.Config;
            var motions = config.Motions;

            var directionX = Random.value < 0.5f ? -1f : 1f;
            var margin = config.OffscreenMargin;
            var originX = directionX > 0f ? _leftEdge - margin : _rightEdge + margin;
            var originY = Random.Range(_zone.BottomY, _zone.TopY);

            balloon.Spawn = new BalloonSpawn(new Vector2(originX, originY), directionX, Random.value);

            balloon.Motion = motions[Random.Range(0, motions.Count)];
            balloon.Elapsed = 0f;
            balloon.View.SetPosition(balloon.Spawn.Origin);
        }

        private bool IsFullyOffscreen(float x, float directionX, float margin) =>
            directionX > 0f ? x > _rightEdge + margin : x < _leftEdge - margin;
    }
}

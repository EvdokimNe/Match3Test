using System;
using System.Collections.Generic;
using Match3.Configs;
using UnityEngine;

namespace Match3.Board.Data
{
    public sealed class BlockRegistry
    {
        private Block[] _slots = Array.Empty<Block>();
        private bool[] _isAlive = Array.Empty<bool>();
        private Stack<int> _free = new();
        private List<int> _alive = new();

        public int Capacity => _slots.Length;
        public IReadOnlyList<int> AliveIds => _alive;

        public void Initialize(int capacity)
        {
            _slots = new Block[capacity];
            _isAlive = new bool[capacity];
            _free = new Stack<int>(capacity);
            _alive = new List<int>(capacity);

            for (var id = 0; id < capacity; id++)
                _slots[id] = new Block(id);

            for (var id = capacity - 1; id >= 0; id--)
                _free.Push(id);
        }

        public void Clear()
        {
            for (var i = 0; i < _alive.Count; i++)
            {
                var id = _alive[i];
                _slots[id].Reset();
                _isAlive[id] = false;
                _free.Push(id);
            }

            _alive.Clear();
        }

        public Block Spawn(BlockConfig config)
        {
            if (_free.Count == 0)
            {
                Debug.LogError($"[Match3] BlockRegistry: пул на {Capacity} блоков исчерпан — " +
                               "блоков больше, чем клеток на поле максимального размера.");
                return null;
            }

            var id = _free.Pop();
            var block = _slots[id];
            block.Setup(config);
            _isAlive[id] = true;
            _alive.Add(id);
            return block;
        }

        public void Despawn(int id)
        {
            if (!IsAlive(id))
                return;

            _slots[id].Reset();
            _isAlive[id] = false;
            _alive.Remove(id);
            _free.Push(id);
        }

        public bool IsAlive(int id) => id >= 0 && id < _isAlive.Length && _isAlive[id];

        public Block Get(int id) => IsAlive(id) ? _slots[id] : null;
    }
}

using System.Collections.Generic;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Spawn
{
    internal class PositionGenerator
    {
        private float2[] _bakedPositions;
        private int _bakedSeed;
        
        private int _radius;
        private int _amount;
        private int _seed;
        
        public PositionGenerator()
        {
            _bakedPositions = new float2[0];
        }

        public void SetSeed(int seed)
        {
            _seed = seed;
        }

        public IReadOnlyList<float2> IteratePositions(int radius, int amount)
        {
            if ((_amount != amount) || (radius != _radius) || (_bakedSeed != _seed))
            {
                _radius = radius;
                _amount = amount;
                _bakedSeed = _seed;
                
                GeneratePositions(radius, amount, _bakedSeed);
            }

            return _bakedPositions;
        }

        private void GeneratePositions(float radius, int amount, int seed)
        {
            if (_bakedPositions.Length < amount)
            {
                _bakedPositions = new float2[amount];
            }
            
            UnityEngine.Random.InitState(seed);

            for (var i = 0; i < amount; i++)
            {
                var pos = UnityEngine.Random.insideUnitCircle * radius;
                _bakedPositions[i] = new float2(pos.x, pos.y);
            }
        }
    }
}
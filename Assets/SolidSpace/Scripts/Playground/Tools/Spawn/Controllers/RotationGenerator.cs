using System.Collections;
using System.Collections.Generic;
using SolidSpace.Mathematics;

namespace SolidSpace.Playground.Tools.Spawn
{
    public class RotationGenerator
    {
        private float[] _bakedRotations;
        private int _bakedSeed;
        
        private int _seed;
        private int _amount;

        public RotationGenerator()
        {
            _bakedRotations = new float[0];
        }

        public void SetSeed(int seed)
        {
            _seed = seed;
        }

        public IReadOnlyList<float> IterateRotations(int amount)
        {
            if ((_amount != amount) || (_bakedSeed != _seed))
            {
                _amount = amount;
                _bakedSeed = _seed;

                GenerateRotations(_seed, _amount);
            }

            return _bakedRotations;
        }

        private void GenerateRotations(int seed, int amount)
        {
            if (_bakedRotations.Length < amount)
            {
                _bakedRotations = new float[amount];
            }

            UnityEngine.Random.InitState(seed);
            
            for (var i = 0; i < amount; i++)
            {
                _bakedRotations[i] = UnityEngine.Random.value * FloatMath.TwoPI;
            }
        }
    }
}
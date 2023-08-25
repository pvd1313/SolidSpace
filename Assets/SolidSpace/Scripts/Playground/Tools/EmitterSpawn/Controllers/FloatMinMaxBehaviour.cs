using System.Globalization;
using SolidSpace.UI.Factory;

namespace SolidSpace.Playground.Tools.EmitterSpawn
{
    public class FloatMinMaxBehaviour : IStringFieldCorrectionBehaviour
    {
        private readonly float _min;
        private readonly float _max;

        public FloatMinMaxBehaviour(float min, float max)
        {
            _min = min;
            _max = max;
        }
        
        public string TryFixString(string value, out bool wasFixed)
        {
            wasFixed = true;

            if (!float.TryParse(value, out var parsedFloat))
            {
                return _min.ToString(CultureInfo.InvariantCulture);
            }

            if (parsedFloat < _min)
            {
                return _min.ToString(CultureInfo.InvariantCulture);
            }

            if (parsedFloat > _max)
            {
                return _max.ToString(CultureInfo.InvariantCulture);
            }

            wasFixed = false;

            return default;
        }
    }
}
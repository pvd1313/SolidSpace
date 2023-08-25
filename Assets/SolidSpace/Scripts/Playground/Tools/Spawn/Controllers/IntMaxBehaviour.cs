using SolidSpace.UI.Factory;

namespace SolidSpace.Playground.Tools.Spawn
{
    public class IntMaxBehaviour : IStringFieldCorrectionBehaviour
    {
        private readonly int _min;

        public IntMaxBehaviour(int min)
        {
            _min = min;
        }
        
        public string TryFixString(string value, out bool wasFixed)
        {
            wasFixed = true;

            if (!int.TryParse(value, out var parsedInt))
            {
                return _min.ToString();
            }

            if (parsedInt < _min)
            {
                return _min.ToString();
            }

            wasFixed = false;
            
            return default;
        }
    }
}
using System.Collections.Generic;

namespace SolidSpace.Debugging
{
    public static class SpaceDebug
    {
        public static IReadOnlyDictionary<string, int> IntStates => _intStates;
        public static IReadOnlyDictionary<string, float> FloatStates => _floatStates;

        private static readonly Dictionary<string, int> _intStates;
        private static readonly Dictionary<string, float> _floatStates;
        
        static SpaceDebug()
        {
            _intStates = new Dictionary<string, int>();
            _floatStates = new Dictionary<string, float>();
        }
        
        public static void LogState(string id, int value)
        {
            _intStates[id] = value;
        }
        
        public static void LogState(string id, float value)
        {
            _floatStates[id] = value;
        }
    }
}
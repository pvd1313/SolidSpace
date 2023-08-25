using UnityEngine;

namespace SolidSpace.Profiling
{
    [System.Serializable]
    public class ProfilingConfig
    {
        public bool EnableSolidProfiling => _enableSolidProfiling;
        public bool EnableUnityProfiling => _enableUnityProfiling;
        public int MaxRecordCount => _maxRecordCount;
        public int StackSize => _stackSize;
        
        [SerializeField] private bool _enableSolidProfiling;
        [SerializeField] private bool _enableUnityProfiling;
        [SerializeField] private int _maxRecordCount;
        [SerializeField] private int _stackSize;

        public ProfilingConfig(bool enableSolidProfiling, bool enableUnityProfiling, int maxRecordCount, int stackSize)
        {
            _enableSolidProfiling = enableSolidProfiling;
            _enableUnityProfiling = enableUnityProfiling;
            _maxRecordCount = maxRecordCount;
            _stackSize = stackSize;
        }
    }
}
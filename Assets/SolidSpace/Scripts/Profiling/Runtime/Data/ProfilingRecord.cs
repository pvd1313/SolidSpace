using System.Runtime.CompilerServices;

namespace SolidSpace.Profiling
{
    internal struct ProfilingRecord
    {
        private const uint TimeSamplesMask = 0x7FFFFFFF;
        private const uint CommandTypeMask = 0x80000000;
        
        private uint _timeAndCommand;
        private int _nameHash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int timeSamples, bool isBeginSampleCommand, int nameHash)
        {
            _timeAndCommand = (uint) (timeSamples & TimeSamplesMask) | (isBeginSampleCommand ? CommandTypeMask : 0);
            _nameHash = nameHash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out int timeSamples, out bool isBeginSampleCommand, out int nameHash)
        {
            isBeginSampleCommand = (_timeAndCommand & CommandTypeMask) != 0;
            timeSamples = (int) (_timeAndCommand & TimeSamplesMask);
            nameHash = _nameHash;
        }
    }
}
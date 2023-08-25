namespace SolidSpace.Profiling
{
    public readonly struct ProfilingHandle
    {
        private readonly ProfilingManager _manager;

        internal ProfilingHandle(ProfilingManager manager)
        {
            _manager = manager;
        }

        public void BeginSample(string name)
        {
            _manager.OnBeginSample(name);
        }

        public void EndSample(string name)
        {
            _manager.OnEndSample(name);
        }
    }
}
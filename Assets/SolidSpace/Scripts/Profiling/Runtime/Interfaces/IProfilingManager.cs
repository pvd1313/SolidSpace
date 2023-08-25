namespace SolidSpace.Profiling
{
    public interface IProfilingManager
    {
        ProfilingTreeReader Reader { get; }
        
        ProfilingHandle GetHandle(object owner);
    }
}
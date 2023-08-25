namespace SolidSpace.Profiling
{
    public interface IProfilingProcessor
    {
        void Initialize();
        void Update();
        void FinalizeObject();
    }
}
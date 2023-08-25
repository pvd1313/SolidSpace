namespace SolidSpace.Playground.Core
{
    public interface IPlaygroundTool
    {
        void OnInitialize();
        void OnUpdate();
        void OnActivate(bool isActive);
        void OnFinalize();
    }
}
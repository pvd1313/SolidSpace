using Unity.Entities;

namespace SolidSpace.Playground.Tools.Capture
{
    public interface ICaptureTool
    {
        void OnUpdate();

        void OnActivate(bool isActive);

        EntityQuery CreateQueryFromCurrentFilter();
    }
}
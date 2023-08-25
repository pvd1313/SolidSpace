using SolidSpace.Entities.ParentHandle;
using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct ChildComponent : IComponentData
    {
        public ParentHandleInfo parentHandle;
    }
}
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Components
{
    public struct RectSizeComponent : IComponentData
    {
        public half2 value;
    }
}
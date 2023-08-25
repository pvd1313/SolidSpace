using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct SpriteRenderComponent : IComponentData
    {
        public AtlasIndex16 colorIndex;
        public AtlasIndex64 frameIndex;
    }
}
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.EntitySearch
{
    public interface IEntitySearchSystem
    {
        EntitySearchResult Result { get; }

        void SetSearchPosition(float2 position);
        
        void SetEnabled(bool enabled);

        void SetQuery(EntityQueryDesc queryDesc);
        void SetSearchRadius(float radius);
    }
}
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.EntitySearch
{
    public struct EntitySearchResult
    {
        public Entity nearestEntity;
        public float2 nearestPosition;
        public NativeSlice<Entity> inRadiusEntities;
        public NativeSlice<float2> inRadiusPositions;
        public int inRadiusCount;
        public bool isValid;
    }
}
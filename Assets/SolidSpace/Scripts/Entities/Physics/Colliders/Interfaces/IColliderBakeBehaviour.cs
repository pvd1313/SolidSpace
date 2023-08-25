using Unity.Entities;

namespace SolidSpace.Entities.Physics.Colliders
{
    public interface IColliderBakeBehaviour
    {
        void OnInitialize(int colliderCount);

        void OnProcessChunk(ArchetypeChunk chunk);

        void OnProcessChunkEntity(int chunkEntityIndex, int colliderIndex);
    }
}
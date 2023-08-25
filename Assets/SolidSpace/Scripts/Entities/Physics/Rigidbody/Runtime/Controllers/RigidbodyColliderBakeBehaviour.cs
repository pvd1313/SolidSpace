using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    public struct RigidbodyColliderBakeBehaviour : IColliderBakeBehaviour
    {
        public ComponentTypeHandle<RigidbodyComponent> rigidbodyHandle;

        [NativeDisableContainerSafetyRestriction] private NativeArray<RigidbodyComponent> _chunkRigidbodies;
        
        public void OnInitialize(int colliderCount)
        {
            
        }

        public void OnProcessChunk(ArchetypeChunk chunk)
        {
            _chunkRigidbodies = chunk.GetNativeArray(rigidbodyHandle);
        }

        public void OnProcessChunkEntity(int chunkEntityIndex, int colliderIndex)
        {
            _chunkRigidbodies[chunkEntityIndex] = new RigidbodyComponent
            {
                colliderIndex = (ushort) colliderIndex
            };
        }
    }
}
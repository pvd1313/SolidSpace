using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.JobUtilities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace SolidSpace.Entities.Bullets
{
    public struct BulletColliderBakeBehaviour : IColliderBakeBehaviour
    {
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<HealthComponent> outHealthComponents;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<SpriteRenderComponent> outSpriteComponents;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Entity> outEntities;
        
        [ReadOnly] public ComponentTypeHandle<SpriteRenderComponent> spriteHandle;
        [ReadOnly] public ComponentTypeHandle<HealthComponent> healthHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;
        
        [NativeDisableContainerSafetyRestriction] private NativeArray<SpriteRenderComponent> _chunkSprites;
        [NativeDisableContainerSafetyRestriction] private NativeArray<HealthComponent> _chunkHealth;
        [NativeDisableContainerSafetyRestriction] private NativeArray<Entity> _chunkEntities;

        public void OnInitialize(int colliderCount)
        {
            outHealthComponents = NativeMemory.CreateTempArray<HealthComponent>(colliderCount);
            outSpriteComponents = NativeMemory.CreateTempArray<SpriteRenderComponent>(colliderCount);
            outEntities = NativeMemory.CreateTempArray<Entity>(colliderCount);
        }

        public void OnProcessChunk(ArchetypeChunk chunk)
        {
            _chunkSprites = chunk.GetNativeArray(spriteHandle);
            _chunkHealth = chunk.GetNativeArray(healthHandle);
            _chunkEntities = chunk.GetNativeArray(entityHandle);
        }

        public void OnProcessChunkEntity(int chunkEntityIndex, int colliderIndex)
        {
            outHealthComponents[colliderIndex] = _chunkHealth[chunkEntityIndex];
            outSpriteComponents[colliderIndex] = _chunkSprites[chunkEntityIndex];
            outEntities[colliderIndex] = _chunkEntities[chunkEntityIndex];
        }
        
        public void Dispose()
        {
            outHealthComponents.Dispose();
            outSpriteComponents.Dispose();
            outEntities.Dispose();
        }
    }
}
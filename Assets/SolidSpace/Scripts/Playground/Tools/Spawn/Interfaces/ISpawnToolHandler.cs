using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Spawn
{
    public interface ISpawnToolHandler
    {
        void OnSpawnEvent(SpawnEventData eventData);

        void OnDrawSpawnCircle(float2 position, float radius);
    }
}
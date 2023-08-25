using Unity.Mathematics;

namespace SolidSpace.Entities.Actors
{
    public interface IActorControlSystem
    {
        void SetActorsTargetPosition(float2 position);
    }
}
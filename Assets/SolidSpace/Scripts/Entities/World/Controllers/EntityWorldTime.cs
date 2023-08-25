using SolidSpace.GameCycle;

namespace SolidSpace.Entities.World
{
    internal class EntityWorldTime : IEntityWorldTime, IUpdatable
    {
        public double ElapsedTime { get; private set; }
        
        public float DeltaTime { get; private set; }
        
        public void OnUpdate()
        {
            var deltaTime = UnityEngine.Time.deltaTime;
            ElapsedTime += deltaTime;
            DeltaTime = deltaTime;
        }
    }
}
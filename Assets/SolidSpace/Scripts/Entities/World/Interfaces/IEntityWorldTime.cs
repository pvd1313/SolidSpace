namespace SolidSpace.Entities.World
{
    public interface IEntityWorldTime
    {
        public double ElapsedTime { get; }
        
        public float DeltaTime { get; }
    }
}
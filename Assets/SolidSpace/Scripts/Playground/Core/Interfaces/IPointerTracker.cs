using Unity.Mathematics;

namespace SolidSpace.Playground.Core
{
    public interface IPointerTracker
    {
        public float2 Position { get; }

        public bool ClickedThisFrame { get; }
        
        public bool IsHeldThisFrame { get; }
    }
}
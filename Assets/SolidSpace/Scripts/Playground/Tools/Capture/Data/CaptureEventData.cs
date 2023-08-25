using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Capture
{
    public struct CaptureEventData
    {
        public ECaptureEventType eventType;
        public Entity entity;
        public float2 entityPosition;
        public float2 startPointer;
        public float2 currentPointer;
    }
}
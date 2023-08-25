using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Capture
{
    public interface ICaptureToolHandler
    {
        void OnCaptureEvent(CaptureEventData eventData);

        void OnDrawSelectionCircle(float2 position, float radius);
    }
}
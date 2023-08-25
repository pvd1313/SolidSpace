using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SolidSpace.Entities.Rendering.Pixels
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PixelVertexData
    {
        public float2 position;
    }
}
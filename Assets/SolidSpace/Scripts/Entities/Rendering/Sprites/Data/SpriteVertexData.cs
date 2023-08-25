using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SpriteVertexData
    {
        public float2 position;
        public half2 colorUV;
        public half2 frameUV;
        public float frameZValue;
    }
}
using SolidSpace.Entities.Health;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Prefabs
{
    [BurstCompile]
    public struct BlitHealthToFrameJob : IJob
    {
        [ReadOnly] public int3 inFrameAtlasOffset;
        [ReadOnly] public int2 inFrameAtlasSize;
        [ReadOnly] public int2 inHealthSize;
        [ReadOnly] public NativeSlice<byte> inHealth;

        public NativeArray<float> inOutFrameAtlas;

        public void Execute()
        {
            for (var y = 0; y < inHealthSize.y; y++)
            {
                for (var x = 0; x < inHealthSize.x; x++)
                {
                    var framePoint = (inFrameAtlasOffset.y + y) * inFrameAtlasSize.x + inFrameAtlasOffset.x + x;
                    var frameValue = (int) inOutFrameAtlas[framePoint];
                    
                    if (HealthUtil.HasBit(inHealth, inHealthSize, new int2(x, y)))
                    {
                        frameValue |= 1 << inFrameAtlasOffset.z;
                    }
                    else
                    {
                        frameValue &= ~(1 << inFrameAtlasOffset.z);
                    }

                    inOutFrameAtlas[framePoint] = (ushort) frameValue;
                }
            }
        }
    }
}
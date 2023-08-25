using SolidSpace.Entities.Atlases;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;

namespace SolidSpace.Entities.Health
{
    internal class HealthAtlasSystem : IHealthAtlasSystem, IInitializable
    {
        public NativeArray<byte> Data => _data;
        public NativeSlice<AtlasChunk1D> Chunks => _indexManager.Chunks;
        
        private readonly Atlas1DConfig _config;
        
        private AtlasIndexManager1D16 _indexManager;
        private NativeArray<byte> _data;

        public HealthAtlasSystem(Atlas1DConfig config)
        {
            _config = config;
        }
        
        public void OnInitialize()
        {
            _data = NativeMemory.CreatePermArray<byte>(_config.AtlasSize);
            _indexManager = new AtlasIndexManager1D16(_config);
        }

        public AtlasIndex16 Allocate(int2 size)
        {
            var byteCount = HealthUtil.GetRequiredByteCount(size);
            return _indexManager.Allocate(byteCount);
        }

        public void Release(AtlasIndex16 index)
        {
            _indexManager.Release(index);
        }

        public void OnFinalize()
        {
            _indexManager.Dispose();
            Data.Dispose();
        }
    }
}
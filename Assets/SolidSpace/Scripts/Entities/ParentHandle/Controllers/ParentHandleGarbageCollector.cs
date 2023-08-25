using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.ParentHandle
{
    public class ParentHandleGarbageCollector : IInitializable, IUpdatable
    {
        private readonly IParentHandleManager _handleManager;
        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private ProfilingHandle _profiler;

        public ParentHandleGarbageCollector(IParentHandleManager handleManager, IEntityManager entityManager, 
            IProfilingManager profilingManager)
        {
            _handleManager = handleManager;
            _entityManager = entityManager;
            _profilingManager = profilingManager;
        }

        public void OnInitialize()
        {
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(ParentComponent)
            });
            _profiler = _profilingManager.GetHandle(this);
        }
        
        public void OnUpdate()
        {
            var archetypeChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var handles = _handleManager.Handles;
            
            _profiler.BeginSample("Create byte mask");
            var maskSize = handles.Length;
            var occupationMask = NativeMemory.CreateTempArray<byte>(handles.Length);
            var jobCount = (int) Math.Ceiling(maskSize / 128f);
            new FillNativeArrayJob<byte>
            {
                inValue = 0,
                inTotalItem = maskSize,
                inItemPerJob = 128,
                outNativeArray = occupationMask
            }.Schedule(jobCount, 4).Complete();
            _profiler.EndSample("Create byte mask");
            
            _profiler.BeginSample("Fill byte mask");
            new FillByteMaskWithParentHandleJob
            {
                inArchetypeChunks = archetypeChunks,
                parentHandle = _entityManager.GetComponentTypeHandle<ParentComponent>(true),
                outMask = occupationMask,
            }.Schedule(archetypeChunks.Length, 4).Complete();
            _profiler.EndSample("Fill byte mask");
            
            _profiler.BeginSample("Compare mask");
            jobCount = (int) Math.Ceiling(maskSize / 32f);
            var compareJob = new CompareParentHandleMaskJob
            {
                inByteMask = occupationMask,
                inItemPerJob = 32,
                inParentHandles = _handleManager.Handles,
                outCounts = NativeMemory.CreateTempArray<int>(jobCount),
                outWastedHandles = NativeMemory.CreateTempArray<ushort>(maskSize)
            };
            compareJob.Schedule(jobCount, 4).Complete();
            _profiler.EndSample("Compare mask");

            _profiler.BeginSample("Collect results");
            var handleCollectJob = new DataCollectJob<ushort>
            {
                inCounts = compareJob.outCounts,
                inOffset = 32,
                inOutData = compareJob.outWastedHandles,
                outCount = NativeMemory.CreateTempJobReference<int>()
            };
            handleCollectJob.Run();
            _profiler.EndSample("Collect results");
            
            _profiler.BeginSample("Release handles");
            var totalCount = handleCollectJob.outCount.Value;
            for (var i = 0; i < totalCount; i++)
            {
                _handleManager.ReleaseHandle(compareJob.outWastedHandles[i]);
            }
            _profiler.EndSample("Release handles");

            _profiler.BeginSample("Disposal");
            archetypeChunks.Dispose();
            occupationMask.Dispose();
            compareJob.outCounts.Dispose();
            compareJob.outWastedHandles.Dispose();
            handleCollectJob.outCount.Dispose();
            _profiler.EndSample("Disposal");
        }

        public void OnFinalize()
        {

        }
    }
}
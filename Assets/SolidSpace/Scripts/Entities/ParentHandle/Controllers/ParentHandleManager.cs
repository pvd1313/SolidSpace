using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using Unity.Collections;

namespace SolidSpace.Entities.ParentHandle
{
    public class ParentHandleManager : IInitializable, IParentHandleManager
    {
        public NativeSlice<HandleState> Handles => new NativeSlice<HandleState>(_handles, 0, _handleCount);
        
        private NativeArray<HandleState> _handles;
        private int _handleCount;
        private List<ushort> _freeHandles;
        
        public void OnInitialize()
        {
            _freeHandles = new List<ushort>();
            _handles = NativeMemory.CreatePermArray<HandleState>(0);
        }
        
        public ParentHandleInfo AllocateHandle()
        {
            var lastItem = _freeHandles.Count - 1;
            if (lastItem >= 0)
            {
                var handleIndex = _freeHandles[lastItem];
                _freeHandles.RemoveAt(lastItem);
                var handleState = _handles[handleIndex];
                handleState.isOccupied = true;
                handleState.version++;
                _handles[handleIndex] = handleState;

                return new ParentHandleInfo
                {
                    index = handleIndex,
                    version = handleState.version
                };
            }

            if (_handleCount >= ushort.MaxValue)
            {
                throw new OutOfMemoryException($"Too many handles ({ushort.MaxValue}) max");
            }
            
            NativeMemory.MaintainPersistentArrayLength(ref _handles, new ArrayMaintenanceData
            {
                itemPerAllocation = 128,
                requiredCapacity = _handleCount + 1,
                copyOnResize = true
            });
            
            _handles[_handleCount] = new HandleState
            {
                isOccupied = true,
                version = 0
            };

            return new ParentHandleInfo
            {
                index = (ushort) _handleCount++,
                version = 0
            };
        }

        public void ReleaseHandle(ushort handleIndex)
        {
            if (handleIndex >= _handleCount)
            {
                throw new InvalidOperationException($"Handle ({handleIndex}) is not occupied yet (index out of range)");
            }

            var handleState = _handles[handleIndex];
            if (!handleState.isOccupied)
            {
                throw new InvalidOperationException($"Handle ({handleIndex}) is not occupied yet");
            }

            handleState.version++;
            handleState.isOccupied = false;
            _handles[handleIndex] = handleState;
            _freeHandles.Add(handleIndex);
        }

        public void OnFinalize()
        {
            _handles.Dispose();
        }
    }
}
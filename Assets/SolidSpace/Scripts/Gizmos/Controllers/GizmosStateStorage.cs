using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    public class GizmosStateStorage : IInitializable, IGizmosStateStorage
    {
        public int Version { get; private set; }
        public int HandleCount => _lastHandleId;

        private Dictionary<string, ushort> _handleToId;
        private ushort _lastHandleId;
        private NativeArray<int> _activityChunks;
        private NativeArray<Color> _colors;
        private List<string> _names;

        public void OnInitialize()
        {
            _handleToId = new Dictionary<string, ushort>();
            _lastHandleId = 0;
            _activityChunks = NativeMemory.CreatePermArray<int>(0);
            _colors = NativeMemory.CreatePermArray<Color>(0);
            _names = new List<string>();
        }

        public ushort GetOrCreateHandleId(string name, Color defaultColor)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{nameof(name)} is null or empty");
            }
            
            if (_handleToId.TryGetValue(name, out var handleId))
            {
                return handleId;
            }
            
            if (_lastHandleId >= ushort.MaxValue)
            {
                throw new OutOfMemoryException("Too many handles");
            }
                
            handleId = _lastHandleId++;
            _handleToId[name] = handleId;
            var rule = new ArrayMaintenanceData
            {
                copyOnResize = true,
                itemPerAllocation = 32,
                requiredCapacity = _lastHandleId
            };
            NativeMemory.MaintainPersistentArrayLength(ref _activityChunks, rule);
            NativeMemory.MaintainPersistentArrayLength(ref _colors, rule);
            var enabled = PlayerPrefs.GetInt(name, 1) > 0;
            SetChunkActivityBit(handleId, enabled);
            _names.Add(name);
            _colors[handleId] = defaultColor;
            Version++;

            return handleId;
        }

        public bool GetHandleEnabled(ushort handleId)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            return GetChunkActivityBit(handleId);
        }

        public void SetHandleEnabled(ushort handleId, bool enabled)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            if (GetChunkActivityBit(handleId) == enabled)
            {
                return;
            }

            PlayerPrefs.SetInt(_names[handleId], enabled ? 1 : 0);
            SetChunkActivityBit(handleId, enabled);
            Version++;
        }

        public Color GetHandleColor(ushort handleId)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            return _colors[handleId];
        }

        public void SetHandleColor(ushort handleId, Color color)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            if (_colors[handleId] == color)
            {
                return;
            }

            _colors[handleId] = color;
            Version++;
        }

        public string GetHandleName(ushort handleId)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            return _names[handleId];
        }

        private void SetChunkActivityBit(ushort handleId, bool active)
        {
            if (active)
            {
                _activityChunks[handleId / 32] |= 1 << (handleId % 32);
            }
            else
            {
                _activityChunks[handleId / 32] &= ~(1 << handleId % 32);
            }
        }

        private bool GetChunkActivityBit(ushort handleId)
        {
            return (_activityChunks[handleId / 32] & (1 << handleId % 32)) > 0;
        }

        public void OnFinalize()
        {
            _activityChunks.Dispose();
            _colors.Dispose();
        }
    }
}
using UnityEngine;

namespace SolidSpace.Gizmos
{
    public interface IGizmosStateStorage
    {
        int Version { get; }
        int HandleCount { get; }
        ushort GetOrCreateHandleId(string name, Color defaultColor);
        bool GetHandleEnabled(ushort handleId);
        void SetHandleEnabled(ushort handleId, bool enabled);
        Color GetHandleColor(ushort handleId);
        void SetHandleColor(ushort handleId, Color color);
        string GetHandleName(ushort handleId);
    }
}
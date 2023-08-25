using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Prefabs
{
    public interface IPrefabSystem
    {
        public IReadOnlyList<ComponentType> PrefabComponents { get; }

        public ushort Create(Texture2D texture);

        public void Instantiate(ushort prefabIndex, float2 position, float rotation);
        
        public void ScheduleRelease(ushort prefabIndex);

        public void ScheduleReplication(PrefabReplicationData replicationData);
    }
}
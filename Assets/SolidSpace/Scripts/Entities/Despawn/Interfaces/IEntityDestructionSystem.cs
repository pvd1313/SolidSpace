using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Despawn
{
    public interface IEntityDestructionSystem
    {
        void ScheduleDestroy(NativeSlice<Entity> entities);

        void ScheduleDestroy(Entity entity);
        void ScheduleDestroy(EntityQuery query);
    }
}
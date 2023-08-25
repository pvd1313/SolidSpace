namespace SolidSpace.Entities.Splitting
{
    public interface ISplittingCommandSystem
    {
        void ScheduleSplittingCheck(SplittingEntityData entityData);
    }
}
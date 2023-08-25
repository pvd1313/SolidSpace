namespace SolidSpace.GameCycle
{
    public interface IInitializable
    {
        void OnInitialize();

        void OnFinalize();
    }
}
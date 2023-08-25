namespace SolidSpace.Playground.Tools.Spawn
{
    public interface ISpawnToolFactory
    {
        ISpawnTool Create(ISpawnToolHandler handler);
    }
}
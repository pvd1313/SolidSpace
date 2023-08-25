using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.Spawn
{
    public class SpawnToolInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<SpawnToolFactory>();
        }
    }
}
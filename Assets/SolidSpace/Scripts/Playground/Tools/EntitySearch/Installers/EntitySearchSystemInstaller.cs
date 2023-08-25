using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.EntitySearch
{
    public class EntitySearchSystemInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<EntitySearchSystem>();
        }
    }
}
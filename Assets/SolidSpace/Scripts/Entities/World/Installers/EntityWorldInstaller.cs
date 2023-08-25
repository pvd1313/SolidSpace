using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.World
{
    internal class EntityWorldInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<EntityManager>();
            container.Bind<EntityWorldTime>();
        }
    }
}
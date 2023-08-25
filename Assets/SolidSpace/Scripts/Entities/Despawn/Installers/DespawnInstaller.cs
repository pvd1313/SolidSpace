using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Despawn
{
    internal class DespawnInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<TimeDespawnComputeSystem>();
            container.Bind<EntityDestructionSystem>();
        }
    }
}
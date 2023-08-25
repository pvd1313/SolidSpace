using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Physics.Velocity
{
    internal class VelocityInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<VelocitySystem>();
        }
    }
}
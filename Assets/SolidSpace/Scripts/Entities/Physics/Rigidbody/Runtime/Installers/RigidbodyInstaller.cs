using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    public class RigidbodyInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<RigidbodyComputeSystem>();
        }
    }
}
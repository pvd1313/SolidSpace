using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.ParentHandle
{
    public class ParentHandleInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ParentHandleManager>();
            container.Bind<ParentHandleGarbageCollector>();
        }
    }
}
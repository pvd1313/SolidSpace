using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Splitting
{
    public class SplittingInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<SplittingController>();
        }
    }
}
using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Randomization
{
    internal class RandomizationInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<RandomValueSystem>();
        }
    }
}
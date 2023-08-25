using Zenject;

namespace SolidSpace.DependencyInjection
{
    public class ZenjectInitializationWrapper : IInitializable
    {
        private readonly IApplicationBootstrapper _bootstrapper;

        public ZenjectInitializationWrapper(IApplicationBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }
        
        public void Initialize()
        {
            _bootstrapper.Run();
        }
    }
}
using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Actors
{
    internal class ActorsInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ActorControlSystem>();
        }
    }
}
using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.RepeatTimer
{
    public class RepeatTimerInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<RepeatTimerSystem>();
        }
    }
}
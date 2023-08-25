using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.ActorControl
{
    internal class ActorControlInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ActorActivationTool>();
            container.Bind<ActorNavigationTool>();
        }
    }
}
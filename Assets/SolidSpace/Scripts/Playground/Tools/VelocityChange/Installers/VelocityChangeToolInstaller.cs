using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.VelocityChange
{
    public class VelocityChangeToolInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<VelocityChangeTool>();
        }
    }
}
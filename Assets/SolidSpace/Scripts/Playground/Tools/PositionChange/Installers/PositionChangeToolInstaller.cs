using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.PositionChange
{
    public class PositionChangeToolInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<PositionChangeTool>();
        }
    }
}
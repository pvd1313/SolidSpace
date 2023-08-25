using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.Eraser
{
    public class EraserToolInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<EraserTool>();
        }
    }
}
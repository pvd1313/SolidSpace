using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.Capture
{
    internal class CaptureToolInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<CaptureToolFactory>();
        }
    }
}
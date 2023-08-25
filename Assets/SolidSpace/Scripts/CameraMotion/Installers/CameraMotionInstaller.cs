using SolidSpace.DependencyInjection;

namespace SolidSpace.CameraMotion
{
    public class CameraMotionInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<CameraMotionController>();
        }
    }
}
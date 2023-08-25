using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Bullets
{
    internal class BulletsInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<BulletComputeSystem>();
        }
    }
}
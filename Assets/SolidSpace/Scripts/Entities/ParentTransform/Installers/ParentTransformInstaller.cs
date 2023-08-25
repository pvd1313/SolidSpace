using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.ParentTransform
{
    public class ParentTransformInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ParentTransformSystem>();
        }
    }
}
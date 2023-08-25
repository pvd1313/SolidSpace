using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public class ComponentFilterToolInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ComponentFilterFactoryConfig _factoryConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ComponentFilterFactory>(_factoryConfig);
        }
    }
}
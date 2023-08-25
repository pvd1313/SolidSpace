using SolidSpace.DependencyInjection;
using SolidSpace.Entities.Atlases;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    internal class HealthInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private Atlas1DConfig _healthAtlasConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<HealthAtlasSystem>(_healthAtlasConfig);
            container.Bind<HealthIndexDisposeSystem>();
        }
    }
}
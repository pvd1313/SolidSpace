using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Entities.Prefabs
{
    internal class PrefabsInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PrefabSystemConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<PrefabSystem>(_config);
        }
    }
}
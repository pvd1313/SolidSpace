using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    internal class GizmosInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private GizmosConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<GizmosManager>(_config);
            container.Bind<GizmosDrawerManager>();
            container.Bind<GizmosStateStorage>();
            container.Bind<GizmosWindow>(_config);
        }
    }
}
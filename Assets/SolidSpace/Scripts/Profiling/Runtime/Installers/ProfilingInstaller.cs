using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Profiling
{
    public class ProfilingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ProfilingConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ProfilingManager>(_config);
        }
    }
}
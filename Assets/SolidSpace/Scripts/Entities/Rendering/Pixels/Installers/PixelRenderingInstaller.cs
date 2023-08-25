using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Pixels
{
    internal class PixelRenderingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PixelMeshSystemConfig _pixelMeshSystemConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<PixelRenderingSystem>(_pixelMeshSystemConfig);
        }
    }
}
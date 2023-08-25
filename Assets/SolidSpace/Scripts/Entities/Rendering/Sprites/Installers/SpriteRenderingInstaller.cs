using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    internal class SpriteRenderingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private SpriteAtlasConfig _colorAtlasConfig;
        [SerializeField] private SpriteAtlasConfig _frameAtlasConfig;
        [SerializeField] private SpriteMeshSystemConfig _meshSystemConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<SpriteColorSystem>(_colorAtlasConfig);
            container.Bind<SpriteFrameSystem>(_frameAtlasConfig);
            container.Bind<SpriteRenderingSystem>(_meshSystemConfig);
            container.Bind<FrameIndexDisposeSystem>();
        }
    }
}
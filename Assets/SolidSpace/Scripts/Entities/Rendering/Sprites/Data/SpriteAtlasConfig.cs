using SolidSpace.Entities.Atlases;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [System.Serializable]
    public class SpriteAtlasConfig
    {
        public Atlas2DConfig AtlasConfig => _atlasConfig;

        [SerializeField] private Atlas2DConfig _atlasConfig;
    }
}
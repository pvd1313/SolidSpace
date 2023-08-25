using System;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [Serializable]
    internal class SpriteMeshSystemConfig
    {
        public Shader Shader => _shader;
        
        [SerializeField] private Shader _shader;
    }
}
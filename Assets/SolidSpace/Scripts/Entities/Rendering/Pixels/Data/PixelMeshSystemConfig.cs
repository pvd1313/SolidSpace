using System;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Pixels
{
    [Serializable]
    internal class PixelMeshSystemConfig
    {
        public Shader Shader => _shader;
        
        [SerializeField] private Shader _shader;
    }
}
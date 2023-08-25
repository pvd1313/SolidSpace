using SolidSpace.Entities.Atlases;
using UnityEngine;

namespace SolidSpace.Entities.Prefabs
{
    [System.Serializable]
    public class PrefabSystemConfig
    {
        public Atlas1DConfig BakedHealthConfig => _bakedHealthConfig;
        
        [SerializeField] private Atlas1DConfig _bakedHealthConfig;
    }
}
using System;
using UnityEngine;

namespace SolidSpace.Entities.Atlases
{
    [Serializable]
    public class Atlas1DConfig
    {
        public int AtlasSize => _atlasSize;
        public int MinItemSize => _minItemSize;
        public int MaxItemSize => _maxItemSize;

        [SerializeField] private int _atlasSize;
        [SerializeField] private int _minItemSize;
        [SerializeField] private int _maxItemSize;
    }
}
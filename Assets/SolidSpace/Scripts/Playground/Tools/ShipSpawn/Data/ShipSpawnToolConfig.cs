using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ShipSpawn
{
    [System.Serializable]
    public class ShipSpawnToolConfig
    {
        public IReadOnlyList<Sprite> Sprites => _sprites;

        [SerializeField] private Sprite[] _sprites;
    }
}
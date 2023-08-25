using System;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.GameCycle
{
    [Serializable]
    public class InitializationOrder
    {
        public IReadOnlyList<ControllerGroup> Groups => _groups;
        
        [SerializeField] private List<ControllerGroup> _groups;
    }
}
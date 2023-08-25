using System;
using System.Collections.Generic;
using SolidSpace.Reflection;
using UnityEngine;

namespace SolidSpace.GameCycle
{
    [Serializable]
    public class ControllerGroup
    {
        public string Name => _name;
        public IReadOnlyList<TypeReference> Controllers => _controllers;

        [SerializeField] private string _name;
        [SerializeField] private List<TypeReference> _controllers;
    }
}
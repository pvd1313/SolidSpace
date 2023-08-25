using System;
using SolidSpace.Reflection;
using UnityEngine;

namespace SolidSpace.Playground.Core
{
    [Serializable]
    public struct ToolIcon
    {
        [SerializeField] public TypeReference toolType;
        [SerializeField] public Sprite icon;
    }
}
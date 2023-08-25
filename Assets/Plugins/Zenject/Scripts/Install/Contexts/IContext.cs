using System.Collections.Generic;
using UnityEngine;

namespace Zenject
{
    public interface IContext
    {
        IEnumerable<GameObject> RootGameObjects { get; }
        
        GameObject GameObject { get; }
        
        Transform Transform { get; }
    }
}
using System;
using UnityEngine;

namespace SolidSpace.Reflection
{
    [Serializable]
    public struct TypeReference
    {
       [SerializeField] public string typeName;

       public bool TryResolve(out Type type)
       {
           type = Type.GetType(typeName);

           return !(type is null);
       }

       public Type Resolve()
       {
           var type = Type.GetType(typeName);
           if (type is null)
           {
               throw new InvalidOperationException($"Failed to resolve type '{typeName}'");
           }

           return type;
       }

       public override string ToString()
       {
           return typeName;
       }
    }
}
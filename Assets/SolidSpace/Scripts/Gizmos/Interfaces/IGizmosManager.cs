using UnityEngine;

namespace SolidSpace.Gizmos
{
    public interface IGizmosManager
    {
        GizmosHandle GetHandle(object owner, string name, Color defaultColor);
        GizmosHandle GetHandle(object owner, Color defaultColor);
    }
}
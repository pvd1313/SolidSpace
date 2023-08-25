using UnityEngine;

namespace SolidSpace.Gizmos
{
    public interface IGizmosDrawer
    {
        public Color GizmosColor { get; }

        void Draw(GizmosHandle handle);
    }
}
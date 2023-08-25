using UnityEngine;

namespace SolidSpace.DependencyInjection
{
    public abstract class ScriptableObjectInstaller : ScriptableObject
    {
        public abstract void InstallBindings(IDependencyContainer container);
    }
}
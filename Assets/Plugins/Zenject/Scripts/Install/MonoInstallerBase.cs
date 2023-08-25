#if !NOT_UNITY3D

using System.Diagnostics;
using UnityEngine;

namespace Zenject
{
    [DebuggerStepThrough]
    public class MonoInstallerBase : MonoBehaviour, IInstaller
    {
        [Inject]
        protected DiContainer Container
        {
            get; set;
        }

        public virtual bool IsEnabled
        {
            get { return enabled; }
        }

        public virtual void Start()
        {
            // Define this method so we expose the enabled check box
        }

        public virtual void InstallBindings()
        {
            
        }
    }
}

#endif


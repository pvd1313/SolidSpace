using System.Collections.Generic;
using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Core
{
    internal class PlaygroundCoreInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PlaygroundCoreConfig _config;
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineEditor]
#endif
        [SerializeField] private List<ScriptableObjectInstaller> _installers;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.BindFromComponentInHierarchy<Camera>();
            
            container.Bind<PlaygroundUIManager>();
            container.Bind<PlaygroundCoreController>(_config);
            container.Bind<MouseTracker>();

            foreach (var installer in _installers)
            {
                installer.InstallBindings(container);
            }
        }
    }
}
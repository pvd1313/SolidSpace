using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SolidSpace.DependencyInjection
{
    public class SceneContext : MonoBehaviour, IContext
    {
        public IEnumerable<GameObject> RootGameObjects => gameObject.scene.GetRootGameObjects();

        public GameObject GameObject => gameObject;

        public Transform Transform => gameObject.transform;
        
        [SerializeField] private List<ScriptableObjectInstaller> _installers;

        private DiContainer _container;

        public void Awake()
        {
            _container = new DiContainer();
            _container.DefaultParent = transform;
            _container.IsInstalling = true;
            
            InstallBindings(_container);

            _container.IsInstalling = false;
            
            _container.ResolveRoots();
        }

        public TContract TryResolve<TContract>() where TContract : class
        {
            return _container.TryResolve<TContract>();
        }

        private void InstallBindings(DiContainer container)
        {
            container.Bind<IContext>().FromInstance(this);
            container.Bind<TickableManager>().AsSingle().NonLazy();
            container.Bind<InitializableManager>().AsSingle().NonLazy();
            container.Bind<DisposableManager>().AsSingle().NonLazy();
            container.BindInterfacesTo<ZenjectInitializationWrapper>().AsSingle();
            container.Bind(typeof(SceneKernel), typeof(MonoKernel)).To<SceneKernel>()
                .FromNewComponentOn(gameObject).AsSingle().NonLazy();

            var wrappedContainer = new ZenjectContainerWrapper(container);
            foreach (var installer in _installers)
            {
                installer.InstallBindings(wrappedContainer);
            }
        }
    }
}

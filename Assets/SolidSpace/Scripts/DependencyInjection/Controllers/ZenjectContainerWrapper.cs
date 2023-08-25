using Zenject;

namespace SolidSpace.DependencyInjection
{
    internal class ZenjectContainerWrapper : IDependencyContainer
    {
        private readonly DiContainer _container;

        public ZenjectContainerWrapper(DiContainer container)
        {
            _container = container;
        }

        public void Bind<T>()
        {
            _container.BindInterfacesTo<T>().AsSingle();
        }

        public void Bind<T>(object parameter)
        {
            _container.BindInterfacesTo<T>().AsSingle().WithArguments(parameter);
        }

        public void BindFromComponentInHierarchy<T>()
        {
            _container.Bind<T>().FromComponentInHierarchy().AsSingle();
        }
    }
}
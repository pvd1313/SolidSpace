namespace SolidSpace.DependencyInjection
{
    public interface IDependencyContainer
    {
        void Bind<T>();
        void Bind<T>(object parameter);
        void BindFromComponentInHierarchy<T>();
    }
}
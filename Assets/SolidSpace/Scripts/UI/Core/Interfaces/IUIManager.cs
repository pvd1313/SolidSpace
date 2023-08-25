namespace SolidSpace.UI.Core
{
    public interface IUIManager
    {
        public bool IsMouseOver { get; }
        
        T Build<T>(UIPrefab<T> prefab) where T : class, IUIElement;
        void AttachToRoot(IUIElement view, string rootContainerName);
        void DetachFromRoot(IUIElement view, string rootContainerName);
    }
}
using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.UI.Factory
{
    internal class UIFactoryInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private UIPrefabs _prefabs;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<UIFactory>(_prefabs);

            container.Bind<ToolButtonBuilder>();
            container.Bind<ToolWindowBuilder>();
            container.Bind<TagLabelBuilder>();
            container.Bind<LayoutGridBuilder>();
            container.Bind<GeneralButtonBuilder>();
            container.Bind<StringFieldBuilder>();
            container.Bind<VerticalFixedItemListBuilder>();
        }
    }
}
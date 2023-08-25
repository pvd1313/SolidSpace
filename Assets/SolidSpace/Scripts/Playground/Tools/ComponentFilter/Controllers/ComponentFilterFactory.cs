using System;
using System.Collections.Generic;
using System.Linq;
using SolidSpace.GameCycle;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Entities;
using UnityEngine.UIElements;


namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public class ComponentFilterFactory : IComponentFilterFactory, IInitializable
    {
        private readonly IUIFactory _uiFactory;
        private readonly ComponentFilterFactoryConfig _config;

        private Dictionary<ComponentType, string> _componentToName;

        public ComponentFilterFactory(IUIFactory uiFactory, ComponentFilterFactoryConfig config)
        {
            _uiFactory = uiFactory;
            _config = config;
        }

        public void OnInitialize()
        {
            _componentToName = new Dictionary<ComponentType, string>();
            foreach (var type in IterateAllComponents())
            {
                _componentToName[type] = _config.NameConverter.Replace(type.Name);
            }
        }

        public IComponentFilter Create(params ComponentType[] readonlyEnabledComponents)
        {
            var rawTypes = IterateAllComponents().ToArray();
            var allComponents = new ComponentType[rawTypes.Length];
            var filter = new FilterState[rawTypes.Length];
            var componentToIndex = new Dictionary<ComponentType, int>();
            for (var i = 0; i < rawTypes.Length; i++)
            {
                var component = (ComponentType) rawTypes[i];
                allComponents[i] = component;
                componentToIndex[component] = i;
                filter[i] = new FilterState
                {
                    isLocked = false,
                    state = ETagLabelState.Neutral
                };
            }

            for (var i = 0; i < readonlyEnabledComponents.Length; i++)
            {
                var componentType = readonlyEnabledComponents[i];
                var index = componentToIndex[componentType];
                filter[index] = new FilterState
                {
                    isLocked = true,
                    state = ETagLabelState.Positive
                };
            }

            var window = _uiFactory.CreateToolWindow();
            window.SetTitle("Components");

            var view = new ComponentFilter();
            view.AllComponents = allComponents;
            view.ComponentToIndex = componentToIndex;
            view.Filter = filter;
            view.Root = window.Root;
            
            var container = _uiFactory.CreateLayoutGrid();
            container.SetFlexDirection(FlexDirection.Row);
            container.SetFlexWrap(Wrap.Wrap);
            window.AttachChild(container);

            var tags = new ITagLabel[allComponents.Length];
            for (var i = 0; i < allComponents.Length; i++)
            {
                var state = filter[i];
                var tag = _uiFactory.CreateTagLabel();
                var labelIndex = i;
                var name = _componentToName[rawTypes[i]];
                tag.SetLabel(name);
                tag.SetState(state.state);
                tag.SetLocked(state.isLocked);
                tag.Clicked += () => view.OnTagClicked(labelIndex);
                container.AttachChild(tag);
                tags[i] = tag;
            }
            view.Tags = tags;

            return view;
        }

        public IUIElement CreateReadonly(IReadOnlyList<ComponentType> components)
        {
            var window = _uiFactory.CreateToolWindow();
            window.SetTitle("Components");

            var container = _uiFactory.CreateLayoutGrid();
            container.SetFlexDirection(FlexDirection.Row);
            container.SetFlexWrap(Wrap.Wrap);
            window.AttachChild(container);

            for (var i = 0; i < components.Count; i++)
            {
                var component = components[i];
                var tag = _uiFactory.CreateTagLabel();
                tag.SetLabel(_componentToName[component]);
                tag.SetLocked(true);
                tag.SetState(ETagLabelState.Positive);
                container.AttachChild(tag);
            }

            return window;
        }

        private IEnumerable<Type> IterateAllComponents()
        {
            var inter = typeof(IComponentData);
            
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => t.IsValueType && inter.IsAssignableFrom(t))
                .Where(t => _config.Filter.IsMatch(t.FullName));
        }

        public void OnFinalize() { }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using SolidSpace.GameCycle;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Core
{
    internal class PlaygroundCoreController : IInitializable, IUpdatable
    {
        private struct ToolInfo
        {
            public int order;
            public Sprite icon;
        }
        
        private static readonly string NoToolText = string.Empty;
        
        private readonly IUIManager _uiManager;
        private readonly List<IPlaygroundTool> _tools;
        private readonly IUIFactory _uiFactory;
        private readonly PlaygroundCoreConfig _config;

        private IToolWindow _window;
        private IToolButton[] _buttons;
        private string[] _names;
        private int _toolIndex;

        public PlaygroundCoreController(IUIManager uiManager, List<IPlaygroundTool> tools, IUIFactory uiFactory,
            PlaygroundCoreConfig config)
        {
            _uiManager = uiManager;
            _tools = tools;
            _uiFactory = uiFactory;
            _config = config;
        }
        
        public void OnInitialize()
        {
            _window = _uiFactory.CreateToolWindow();
            _uiManager.AttachToRoot(_window, "ContainerA");
            _window.SetTitle(NoToolText);

            var grid = _uiFactory.CreateLayoutGrid();
            grid.SetFlexDirection(FlexDirection.Row);
            grid.SetFlexWrap(Wrap.Wrap);
            _window.AttachChild(grid);

            var displayInfo = new Dictionary<Type, ToolInfo>();
            for (var i = 0; i < _config.ToolIcons.Count; i++)
            {
                var toolIcon = _config.ToolIcons[i];
                var toolType = toolIcon.toolType.Resolve();
                displayInfo.Add(toolType, new ToolInfo
                {
                    icon = toolIcon.icon,
                    order = i
                });
            }

            foreach (var toolType in _tools.Select(t => t.GetType()))
            {
                if (!displayInfo.ContainsKey(toolType))
                {
                    throw new InvalidOperationException($"Missing configuration for '{toolType.FullName}'");
                }
            }

            _toolIndex = -1;
            _buttons = new IToolButton[_tools.Count];
            _names = new string[_tools.Count];

            var j = 0;
            foreach(var tool in _tools.OrderBy(t => displayInfo[t.GetType()].order))
            {
                tool.OnInitialize();
                
                var toolType = tool.GetType();
                var toolInfo = displayInfo[toolType];
                var button = _uiFactory.CreateToolButton();
                var buttonIndex = j;
                button.SetSelected(false);
                button.SetIcon(toolInfo.icon);
                grid.AttachChild(button);
                button.Clicked += () => OnToolViewClicked(buttonIndex);
                
                _buttons[j] = button;
                _names[j] = _config.ToolNameConverter.Replace(toolType.Name);

                j++;
            }
        }
        
        public void OnUpdate()
        {
            if (_toolIndex != -1)
            {
                _tools[_toolIndex].OnUpdate();
            }
        }

        private void OnToolViewClicked(int newIndex)
        {
            if (_toolIndex == newIndex)
            {
                _buttons[_toolIndex].SetSelected(false);
                _tools[_toolIndex].OnActivate(false);
                _window.SetTitle(_config.WindowDefaultTitle);
                _toolIndex = -1;
                
                return;
            }

            if (_toolIndex != -1)
            {
                _buttons[_toolIndex].SetSelected(false);
                _tools[_toolIndex].OnActivate(false);
            }

            _toolIndex = newIndex;
            _buttons[_toolIndex].SetSelected(true);
            _tools[_toolIndex].OnActivate(true);
            _window.SetTitle(_names[_toolIndex]);
        }

        public void OnFinalize()
        {
            foreach (var tool in _tools)
            {
                tool.OnFinalize();
            }
        }
    }
}
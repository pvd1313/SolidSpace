using System;
using System.Collections.Generic;
using SolidSpace.Entities.Prefabs;
using SolidSpace.Gizmos;
using SolidSpace.Mathematics;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.Spawn;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Tools.ShipSpawn
{
    internal class ShipSpawnTool : IPlaygroundTool, ISpawnToolHandler
    {
        private struct ShipData
        {
            public ushort prefabIndex;
            public int2 size;
            public string name;
        }
        
        private readonly ISpawnToolFactory _spawnToolFactory;
        private readonly IPlaygroundUIManager _uiManager;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IGizmosManager _gizmosManager;
        private readonly IPrefabSystem _prefabSystem;
        private readonly ShipSpawnToolConfig _config;
        private readonly IUIFactory _uiFactory;

        private ISpawnTool _spawnTool;
        private IUIElement _componentsWindow;
        private GizmosHandle _gizmos;
        private List<ShipData> _ships;
        private IToolWindow _shipsWindow;
        private int _shipIndex = -1;
        private List<IToolButton> _shipButtons;

        public ShipSpawnTool(ISpawnToolFactory spawnToolFactory,
                             IPlaygroundUIManager uiManager,
                             IComponentFilterFactory filterFactory,
                             IGizmosManager gizmosManager,
                             IPrefabSystem prefabSystem,
                             ShipSpawnToolConfig config,
                             IUIFactory uiFactory)
        {
            _spawnToolFactory = spawnToolFactory;
            _uiManager = uiManager;
            _filterFactory = filterFactory;
            _gizmosManager = gizmosManager;
            _prefabSystem = prefabSystem;
            _config = config;
            _uiFactory = uiFactory;
        }

        public void OnInitialize()
        {
            _componentsWindow = _filterFactory.CreateReadonly(_prefabSystem.PrefabComponents);
            _spawnTool = _spawnToolFactory.Create(this);
            _gizmos = _gizmosManager.GetHandle(this, Color.yellow);
            _ships = new List<ShipData>();
            
            _shipsWindow = _uiFactory.CreateToolWindow();
            
            var grid = _uiFactory.CreateLayoutGrid();
            grid.SetFlexDirection(FlexDirection.Row);
            grid.SetFlexWrap(Wrap.Wrap);
            _shipsWindow.AttachChild(grid);
            _shipButtons = new List<IToolButton>();

            for (var shipIndex = 0; shipIndex < _config.Sprites.Count; shipIndex++)
            {
                var sprite = _config.Sprites[shipIndex];
                
                _ships.Add(new ShipData
                {
                    prefabIndex = _prefabSystem.Create(sprite.texture),
                    size = new int2(sprite.texture.width, sprite.texture.height),
                    name = sprite.name
                });

                var button = _uiFactory.CreateToolButton();
                button.SetSelected(false);
                button.SetIcon(sprite);
                _shipButtons.Add(button);
                var index = shipIndex;
                button.Clicked += () => OnShipButtonClicked(index);
                
                grid.AttachChild(button);
            }
        }

        private void OnShipButtonClicked(int newIndex)
        {
            if (_shipIndex == newIndex)
            {
                _shipButtons[_shipIndex].SetSelected(false);
                _shipsWindow.SetTitle("");
                _shipIndex = -1;
                
                return;
            }

            if (_shipIndex != -1)
            {
                _shipButtons[_shipIndex].SetSelected(false);
            }

            _shipIndex = newIndex;
            _shipButtons[_shipIndex].SetSelected(true);
            _shipsWindow.SetTitle(_ships[_shipIndex].name);
        }
        
        public void OnFinalize()
        {
            foreach (var shipData in _ships)
            {
                _prefabSystem.ScheduleRelease(shipData.prefabIndex);
            }
        }
        
        public void OnActivate(bool isActive)
        {
            _spawnTool.OnActivate(isActive);       
            _uiManager.SetElementVisible(_componentsWindow, isActive);
            _uiManager.SetElementVisible(_shipsWindow, isActive);
        }
        
        public void OnUpdate()
        {
            if (_shipIndex != -1)
            {
                _spawnTool.OnUpdate();
            }
        }
        
        public void OnSpawnEvent(SpawnEventData eventData)
        {
            var origin = eventData.origin;
            
            switch (eventData.eventType)
            {
                case ESpawnEventType.Preview:
                    _gizmos.DrawWireRect(origin.position, _ships[_shipIndex].size, origin.rotation);
                    var xAxis = FloatMath.Rotate(_ships[_shipIndex].size.x * 0.5f, origin.rotation);
                    _gizmos.DrawLine(origin.position, origin.position + xAxis);
                    break;
                
                case ESpawnEventType.Place:
                    _prefabSystem.Instantiate(_ships[_shipIndex].prefabIndex, origin.position, origin.rotation);

                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDrawSpawnCircle(float2 position, float radius)
        {
            _gizmos.DrawScreenCircle(position, radius);
        }
    }
}
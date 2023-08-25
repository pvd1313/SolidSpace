using System;
using SolidSpace.GameCycle;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    public class GizmosWindow : IInitializable, IUpdatable
    {
        private readonly IUIFactory _uiFactory;
        private readonly IUIManager _uiManager;
        private readonly GizmosConfig _config;
        private readonly IGizmosStateStorage _gizmosStorage;

        private IToolWindow _window;
        private ITagLabel[] _items;
        private IVerticalFixedItemList _itemList;
        private int _version;
        private int _offset;

        public GizmosWindow(IUIFactory uiFactory, IUIManager uiManager, GizmosConfig config, 
            IGizmosStateStorage gizmosStorage)
        {
            _uiFactory = uiFactory;
            _uiManager = uiManager;
            _config = config;
            _gizmosStorage = gizmosStorage;
        }
        
        public void OnInitialize()
        {
            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Gizmos");
            _uiManager.AttachToRoot(_window, "ContainerB");

            _itemList = _uiFactory.CreateVerticalList();
            _itemList.Scrolled += OnListScroll;
            _window.AttachChild(_itemList);

            _items = new ITagLabel[_config.WindowItemCount];
            for (var i = 0; i < _items.Length; i++)
            {
                var item = _uiFactory.CreateTagLabel();
                _itemList.AttachItem(item);
                _items[i] = item;
                var itemIndex = i;
                item.Clicked += () => OnItemClick(itemIndex);
            }
        }

        private void OnItemClick(int index)
        {
            var handleId = _offset + index;
            if (handleId < 0 || handleId >= _gizmosStorage.HandleCount)
            {
                return;
            }

            var enabled = _gizmosStorage.GetHandleEnabled((ushort) handleId);
            _gizmosStorage.SetHandleEnabled((ushort) handleId, !enabled);
        }

        private void OnListScroll(int delta)
        {
            _offset += delta;
            _version--;
        }
        
        public void OnUpdate()
        {
            if (_version == _gizmosStorage.Version)
            {
                return;
            }

            _version = _gizmosStorage.Version;
            var handleCount = _gizmosStorage.HandleCount;
            _offset = Mathf.Clamp(_offset, 0,  Math.Max(0, handleCount - _items.Length));
            for (var i = 0; i < _items.Length; i++)
            {
                var item = _items[i];
                var handleId = _offset + i;
                if (handleId >= handleCount)
                {
                    item.SetLabel(string.Empty);
                    item.SetState(ETagLabelState.Neutral);
                    continue;
                }

                var handleName = _gizmosStorage.GetHandleName((ushort) handleId);
                handleName = _config.HandleNameConverter.Replace(handleName);
                var handleEnabled = _gizmosStorage.GetHandleEnabled((ushort) handleId);
                
                item.SetLabel(handleName);
                item.SetState(handleEnabled ? ETagLabelState.Positive : ETagLabelState.Negative);
            }

            var sliderMinMax = new int2(0, handleCount);
            var sliderOffset = new int2(_offset, Math.Min(handleCount, _offset + _items.Length));
            _itemList.SetSliderState(sliderMinMax, sliderOffset);
        }

        public void OnFinalize()
        {
            
        }
    }
}
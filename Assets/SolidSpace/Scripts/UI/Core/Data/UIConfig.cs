using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    [Serializable]
    internal class UIConfig
    {
        public IReadOnlyList<string> ContainerNames => _containerNames;
        public VisualTreeAsset RootAsset => _rootAsset;
        public PanelSettings PanelSettings => _panelSettings;

        [SerializeField] private List<string> _containerNames;
        [SerializeField] private VisualTreeAsset _rootAsset;
        [SerializeField] private PanelSettings _panelSettings;
    }
}
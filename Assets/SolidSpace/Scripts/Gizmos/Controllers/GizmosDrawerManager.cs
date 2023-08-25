using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;
using SolidSpace.Profiling;

namespace SolidSpace.Gizmos
{
    public class GizmosDrawerManager : IGizmosDrawerManager, IInitializable, IUpdatable
    {
        private struct DrawerInfo
        {
            public IGizmosDrawer drawer;
            public GizmosHandle handle;
            public string ownerName;
        }
        
        private readonly IProfilingManager _profilingManager;
        private readonly List<IGizmosDrawer> _initialDrawers;
        private readonly IGizmosManager _gizmosHandleFactory;

        private ProfilingHandle _profiler;
        private Dictionary<IGizmosDrawer, DrawerInfo> _drawers;

        public GizmosDrawerManager(IProfilingManager profilingManager, List<IGizmosDrawer> initialDrawers,
            IGizmosManager gizmosHandles)
        {
            _profilingManager = profilingManager;
            _initialDrawers = initialDrawers;
            _gizmosHandleFactory = gizmosHandles;
        }
        
        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _drawers = new Dictionary<IGizmosDrawer, DrawerInfo>();
            
            foreach (var drawer in _initialDrawers)
            {
                AddDrawer(drawer);
            }
        }
        
        public void OnUpdate()
        {
            foreach (var info in _drawers.Values)
            {
                _profiler.BeginSample(info.ownerName);
                info.drawer.Draw(info.handle);
                _profiler.EndSample(info.ownerName);
            }
        }
        
        public void AddDrawer(IGizmosDrawer drawer)
        {
            if (_drawers.ContainsKey(drawer))
            {
                throw new InvalidOperationException($"Drawer ({drawer.GetType()}) was already added");
            }  
            
            _drawers.Add(drawer, new DrawerInfo
            {
                drawer = drawer,
                handle = _gizmosHandleFactory.GetHandle(drawer, drawer.GizmosColor),
                ownerName = drawer.GetType().Name
            });
        }

        public void RemoveDrawer(IGizmosDrawer drawer)
        {
            if (!_drawers.Remove(drawer))
            {
                throw new InvalidOperationException($"Drawer ({drawer.GetType()}) was not added to be removed");
            } 
        }

        public void OnFinalize()
        {
            
        }
    }
}
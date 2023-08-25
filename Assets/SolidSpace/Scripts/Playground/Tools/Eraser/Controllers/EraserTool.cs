using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Despawn;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.Capture;
using SolidSpace.UI.Factory;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal class EraserTool : IPlaygroundTool, ICaptureToolHandler
    {
        private readonly IPlaygroundUIManager _playgroundUIManager;
        private readonly IUIFactory _uiFactory;
        private readonly ICaptureToolFactory _captureToolFactory;
        private readonly IGizmosManager _gizmosManager;
        private readonly IEntityDestructionSystem _destructionSystem;

        private ICaptureTool _captureTool;
        private IToolWindow _window;
        private GizmosHandle _gizmos;

        public EraserTool(IPlaygroundUIManager playgroundUIManager,
                          IUIFactory uiFactory,
                          ICaptureToolFactory captureToolFactory,
                          IGizmosManager gizmosManager,
                          IEntityDestructionSystem destructionSystem)
        {
            _playgroundUIManager = playgroundUIManager;
            _uiFactory = uiFactory;
            _captureToolFactory = captureToolFactory;
            _gizmosManager = gizmosManager;
            _destructionSystem = destructionSystem;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.red);
            
            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Eraser");

            var button = _uiFactory.CreateGeneralButton();
            button.SetLabel("Destroy all");
            button.Clicked += OnDestroyClicked;
            _window.AttachChild(button);

            _captureTool = _captureToolFactory.Create(this, typeof(PositionComponent));
        }
        
        public void OnActivate(bool isActive)
        {
            _captureTool.OnActivate(isActive);
            _playgroundUIManager.SetElementVisible(_window, isActive);
        }

        public void OnUpdate()
        {
            _captureTool.OnUpdate();
        }
        
        public void OnCaptureEvent(CaptureEventData eventData)
        {
            switch (eventData.eventType)
            {
                case ECaptureEventType.SelectionSingle:
                    _gizmos.DrawScreenDot(eventData.entityPosition);
                    _gizmos.DrawLine(eventData.currentPointer, eventData.entityPosition);
                    break;
                
                case ECaptureEventType.SelectionMultiple:
                    _gizmos.DrawScreenDot(eventData.entityPosition);
                    break;
                
                case ECaptureEventType.CaptureStart:
                    _destructionSystem.ScheduleDestroy(eventData.entity);
                    break;
                
                case ECaptureEventType.CaptureUpdate:
                    break;
                
                case ECaptureEventType.CaptureEnd:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDrawSelectionCircle(float2 position, float radius)
        {
            _gizmos.DrawScreenCircle(position, radius);
        }

        private void OnDestroyClicked()
        {
            var query = _captureTool.CreateQueryFromCurrentFilter();
            _destructionSystem.ScheduleDestroy(query);
        }

        public void OnFinalize()
        {
            
        }
    }
}
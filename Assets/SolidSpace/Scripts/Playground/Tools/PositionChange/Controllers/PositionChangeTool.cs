using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.Capture;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.PositionChange
{
    internal class PositionChangeTool : IPlaygroundTool, ICaptureToolHandler
    {
        private readonly IGizmosManager _gizmosManager;
        private readonly IEntityManager _entityManager;
        private readonly ICaptureToolFactory _captureToolFactory;

        private GizmosHandle _gizmos;
        private ICaptureTool _captureTool;

        public PositionChangeTool(IGizmosManager gizmosManager, IEntityManager entityManager, 
            ICaptureToolFactory captureToolFactory)
        {
            _gizmosManager = gizmosManager;
            _entityManager = entityManager;
            _captureToolFactory = captureToolFactory;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.white);
            _captureTool = _captureToolFactory.Create(this, typeof(PositionComponent));
        }
        
        public void OnActivate(bool isActive)
        {
            _captureTool.OnActivate(isActive);
        }

        public void OnUpdate()
        {
            _captureTool.OnUpdate();
        }
        
        public void OnCaptureEvent(CaptureEventData eventData)
        {
            switch (eventData.eventType)
            {
                case ECaptureEventType.CaptureStart:
                    break;
                
                case ECaptureEventType.CaptureUpdate:
                    var delta = eventData.currentPointer - eventData.startPointer;
                    _entityManager.SetComponentData(eventData.entity, new PositionComponent
                    {
                        value = eventData.entityPosition + delta
                    });
                    break;
                
                case ECaptureEventType.CaptureEnd:
                    break;
                
                case ECaptureEventType.SelectionSingle:
                    _gizmos.DrawScreenDot(eventData.entityPosition);
                    _gizmos.DrawLine(eventData.currentPointer, eventData.entityPosition);
                    break;
                
                case ECaptureEventType.SelectionMultiple:
                    _gizmos.DrawScreenDot(eventData.entityPosition);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDrawSelectionCircle(float2 position, float radius)
        {
            _gizmos.DrawScreenCircle(position, radius);
        }

        public void OnFinalize()
        {
            
        }
    }
}
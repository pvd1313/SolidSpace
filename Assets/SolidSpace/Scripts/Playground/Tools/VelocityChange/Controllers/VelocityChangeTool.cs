using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.Capture;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.VelocityChange
{
    internal class VelocityChangeTool : IPlaygroundTool, ICaptureToolHandler
    {
        private readonly IGizmosManager _gizmosManager;
        private readonly IEntityManager _entityManager;
        private readonly ICaptureToolFactory _captureToolFactory;

        private GizmosHandle _gizmos;
        private ICaptureTool _captureTool;

        public VelocityChangeTool(IGizmosManager gizmosManager, IEntityManager entityManager,
            ICaptureToolFactory captureToolFactory)
        {
            _gizmosManager = gizmosManager;
            _entityManager = entityManager;
            _captureToolFactory = captureToolFactory;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.cyan);
            _captureTool = _captureToolFactory.Create(this, typeof(PositionComponent), typeof(VelocityComponent));
        }

        public void OnUpdate()
        {
            _captureTool.OnUpdate();
        }

        public void OnActivate(bool isActive)
        {
           _captureTool.OnActivate(isActive);
        }

        public void OnCaptureEvent(CaptureEventData eventData)
        {
            switch (eventData.eventType)
            {
                case ECaptureEventType.CaptureStart:
                    break;
                
                case ECaptureEventType.CaptureUpdate:
                    var delta = eventData.currentPointer - eventData.startPointer;
                    _gizmos.DrawScreenDot(eventData.entityPosition);
                    _gizmos.DrawLine(eventData.entityPosition, eventData.entityPosition + delta);
                    _entityManager.SetComponentData(eventData.entity, new PositionComponent
                    {
                        value = eventData.entityPosition
                    });
                    break;
                
                case ECaptureEventType.CaptureEnd:
                    _entityManager.SetComponentData(eventData.entity, new VelocityComponent
                    {
                        value = eventData.currentPointer - eventData.startPointer
                    });
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
        
        public void OnFinalize() { }
    }
}
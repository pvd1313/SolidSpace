using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.Capture;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ActorControl
{
    internal class ActorActivationTool : IPlaygroundTool, ICaptureToolHandler
    {
        private readonly ICaptureToolFactory _captureToolFactory;
        private readonly IGizmosManager _gizmosManager;
        private readonly IEntityManager _entityManager;

        private GizmosHandle _gizmos;
        private ICaptureTool _captureTool;

        public ActorActivationTool(ICaptureToolFactory captureToolFactory, IGizmosManager gizmosManager, 
            IEntityManager entityManager)
        {
            _captureToolFactory = captureToolFactory;
            _gizmosManager = gizmosManager;
            _entityManager = entityManager;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.magenta);
            _captureTool = _captureToolFactory.Create(this, typeof(PositionComponent), typeof(ActorComponent));
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
                    var actorData = _entityManager.GetComponentData<ActorComponent>(eventData.entity);
                    actorData.isActive = !actorData.isActive;
                    _entityManager.SetComponentData(eventData.entity, actorData);
                    break;
                
                case ECaptureEventType.CaptureUpdate:
                    break;
                
                case ECaptureEventType.CaptureEnd:
                    break;
                
                case ECaptureEventType.SelectionSingle:
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
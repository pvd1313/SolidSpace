using SolidSpace.Entities.Actors;
using SolidSpace.Entities.Components;
using SolidSpace.Playground.Core;
using SolidSpace.UI.Core;
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.ActorControl
{
    internal class ActorNavigationTool : IPlaygroundTool
    {
        private readonly IPointerTracker _pointer;
        private readonly IUIManager _uiManager;
        private readonly IActorControlSystem _actorControlSystem;

        private float2 _navPoint;
        private EntityQueryDesc _query;
        
        public ActorNavigationTool(IPointerTracker pointer, IUIManager uiManager, IActorControlSystem actorControlSystem)
        {
            _pointer = pointer;
            _uiManager = uiManager;
            _actorControlSystem = actorControlSystem;
        }
        
        public void OnInitialize()
        {
            _query = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PositionComponent),
                    typeof(ActorComponent)
                }
            };
        }

        public void OnUpdate()
        {
            if (!_uiManager.IsMouseOver && _pointer.IsHeldThisFrame)
            {
                _navPoint = _pointer.Position;
                _actorControlSystem.SetActorsTargetPosition(_navPoint);
            }
        }

        public void OnActivate(bool isActive)
        {

        }

        public void OnFinalize()
        {
            
        }
    }
}
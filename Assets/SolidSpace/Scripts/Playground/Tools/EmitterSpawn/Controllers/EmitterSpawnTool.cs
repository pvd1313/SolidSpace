using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Mathematics;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.Spawn;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.EmitterSpawn
{
    public class EmitterSpawnTool : IPlaygroundTool, ISpawnToolHandler
    {
        private readonly IEntityManager _entityManager;
        private readonly ISpawnToolFactory _spawnToolFactory;
        private readonly IPlaygroundUIManager _uiManager;
        private readonly IUIFactory _uiFactory;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IGizmosManager _gizmosManager;

        private EntityArchetype _emitterArchetype;
        private ISpawnTool _spawnTool;
        private IToolWindow _emitterWindow;
        private IUIElement _componentsWindow;
        private IStringField _spawnRateField;
        private IStringField _particleVelocityField;
        private IStringField _spreadAngleField;
        private GizmosHandle _gizmos;

        private float _spawnRate;
        private float _particleVelocity;
        private float _spreadAngle;

        public EmitterSpawnTool(IEntityManager entityManager, IPlaygroundUIManager uiManager,
            ISpawnToolFactory spawnToolFactory, IUIFactory uiFactory, IComponentFilterFactory filterFactory,
            IGizmosManager gizmosManager)
        {
            _entityManager = entityManager;
            _spawnToolFactory = spawnToolFactory;
            _uiManager = uiManager;
            _uiFactory = uiFactory;
            _filterFactory = filterFactory;
            _gizmosManager = gizmosManager;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.yellow);
            
            var emitterComponents = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RotationComponent),
                typeof(ParticleEmitterComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent),
            };
            _emitterArchetype = _entityManager.CreateArchetype(emitterComponents);

            _componentsWindow = _filterFactory.CreateReadonly(emitterComponents);

            _emitterWindow = _uiFactory.CreateToolWindow();
            _emitterWindow.SetTitle("Emitter");

            _spawnTool = _spawnToolFactory.Create(this);

            _spawnRate = 60;
            _spawnRateField = _uiFactory.CreateStringField();
            _spawnRateField.SetValue("60");
            _spawnRateField.SetLabel("Spawn Rate");
            _spawnRateField.SetValueCorrectionBehaviour(new FloatMinMaxBehaviour(1, 60));
            _spawnRateField.ValueChanged += () => _spawnRate = float.Parse(_spawnRateField.Value);
            _emitterWindow.AttachChild(_spawnRateField);

            _particleVelocity = 10;
            _particleVelocityField = _uiFactory.CreateStringField();
            _particleVelocityField.SetValue("10");
            _particleVelocityField.SetLabel("Particle Velocity");
            _particleVelocityField.SetValueCorrectionBehaviour(new FloatMinMaxBehaviour(0, 1000));
            _particleVelocityField.ValueChanged += () => _particleVelocity = float.Parse(_particleVelocityField.Value);
            _emitterWindow.AttachChild(_particleVelocityField);

            _spreadAngle = 360;
            _spreadAngleField = _uiFactory.CreateStringField();
            _spreadAngleField.SetValue("360");
            _spreadAngleField.SetLabel("Spread Angle");
            _spreadAngleField.SetValueCorrectionBehaviour(new FloatMinMaxBehaviour(0, 360));
            _spreadAngleField.ValueChanged += () => _spreadAngle = float.Parse(_spreadAngleField.Value);
            _emitterWindow.AttachChild(_spreadAngleField);
        }
        
        public void OnActivate(bool isActive)
        {
            _spawnTool.OnActivate(isActive);
            _uiManager.SetElementVisible(_componentsWindow, isActive);
            _uiManager.SetElementVisible(_emitterWindow, isActive);
        }

        public void OnUpdate()
        {
            _spawnTool.OnUpdate();
        }
        
        public void OnSpawnEvent(SpawnEventData eventData)
        {
            var origin = eventData.origin;
            
            switch (eventData.eventType)
            {
                case ESpawnEventType.Preview:
                    DrawEmitterDirectionAxis(origin.position, origin.rotation);
                    _gizmos.DrawScreenDot(origin.position);
                    break;
                
                case ESpawnEventType.Place:
                    Spawn(origin.position, origin.rotation, _spawnRate, _particleVelocity, _spreadAngle * FloatMath.Deg2Rad);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawEmitterDirectionAxis(float2 position, float rotation)
        {
            var distance = _particleVelocity * 5f;
            
            if (_spreadAngle < 360)
            {
                var forward = FloatMath.Rotate(distance, rotation);
                _gizmos.DrawLine(position, position + forward);
                DrawArc(position, rotation, _spreadAngle * FloatMath.Deg2Rad, distance, 8);
            }
            else
            {
                _gizmos.DrawWirePolygon(position, distance, 8);
            }
        }

        private void DrawArc(float2 position, float rotation, float arcAngle, float distance, int topology)
        {
            var delta = arcAngle / topology;
            var previousPosition = position;
            var startAngle = rotation - arcAngle * 0.5f;
            for (var i = 0; i <= topology; i++)
            {
                var axis = FloatMath.Rotate(distance, startAngle + delta * i);
                var newPosition = position + axis;
                _gizmos.DrawLine(previousPosition, newPosition);
                previousPosition = newPosition;
            }
            
            _gizmos.DrawLine(previousPosition, position);
        }

        public void OnDrawSpawnCircle(float2 position, float radius)
        {
            _gizmos.DrawScreenCircle(position, radius);
        }

        private void Spawn(float2 position, float rotation, float spawnRate, float particleVelocity, float spreadAngle)
        {
            var entity = _entityManager.CreateEntity(_emitterArchetype);
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = position
            });
            _entityManager.SetComponentData(entity, new RepeatTimerComponent
            {
                delay = 1f / spawnRate
            });
            _entityManager.SetComponentData(entity, new RotationComponent
            {
                value = rotation
            });
            _entityManager.SetComponentData(entity, new ParticleEmitterComponent
            {
                particleVelocity = particleVelocity,
                spreadAngle = spreadAngle
            });
        }

        public void OnFinalize() { }
    }
}
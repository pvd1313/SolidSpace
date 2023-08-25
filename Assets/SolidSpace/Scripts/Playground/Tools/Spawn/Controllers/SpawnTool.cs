using System;
using SolidSpace.Mathematics;
using SolidSpace.Playground.Core;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Spawn
{
    internal class SpawnTool : ISpawnTool
    {
        public IToolWindow Window { get; set; }
        public IUIManager UIManager { get; set; }
        public IPointerTracker Pointer { get; set; }
        public IStringField SpawnRadiusField { get; set; }
        public IStringField SpawnAmountField { get; set; }
        public int SpawnRadius { get; set; }
        public int SpawnAmount { get; set; }
        public ISpawnToolHandler Handler { get; set; }
        public PositionGenerator PositionGenerator { get; set; }
        public RotationGenerator RotationGenerator { get; set; }
        public IPlaygroundUIManager PlaygroundUI { get; set; }
        public bool RotationIsRandom { get; set; }
        public ITagLabel RotationLabel { get; set; }
        public float2 PointerClickPosition { get; set; }
        public float PreviousPointerAngle { get; set; }
        public bool IsPlacingMode { get; set; }
        
        public void OnUpdate()
        {
            if (IsPlacingMode)
            {
                Handler.OnDrawSpawnCircle(PointerClickPosition, SpawnRadius);
                SendEvent(ESpawnEventType.Preview);
                
                if (!Pointer.IsHeldThisFrame)
                {
                    IsPlacingMode = false;
                    SendEvent(ESpawnEventType.Place);

                    var seed = Time.frameCount;
                    PositionGenerator.SetSeed(seed);
                    RotationGenerator.SetSeed(seed);
                }
                
                return;
            }
            
            if (UIManager.IsMouseOver)
            {
                return;
            }

            PointerClickPosition = Pointer.Position;
            Handler.OnDrawSpawnCircle(PointerClickPosition, SpawnRadius);
            SendEvent(ESpawnEventType.Preview);

            if (Pointer.ClickedThisFrame)
            {
                IsPlacingMode = true;
            }
        }

        private void SendEvent(ESpawnEventType eventType)
        {
            var positions = PositionGenerator.IteratePositions(SpawnRadius, SpawnAmount);
            
            if (RotationIsRandom)
            {
                var rotations = RotationGenerator.IterateRotations(SpawnAmount);
                for (var i = 0; i < SpawnAmount; i++)
                {
                    Handler.OnSpawnEvent(new SpawnEventData
                    {
                        eventType = eventType,
                        origin = new SpawnOrigin
                        {
                            position = PointerClickPosition + positions[i],
                            rotation = rotations[i]
                        }
                    });
                }
                
                return;
            }
            
            var mouseDirection = Pointer.Position - PointerClickPosition;
            if (Math.Abs(mouseDirection.x) > float.Epsilon || Math.Abs(mouseDirection.y) > float.Epsilon)
            {
                PreviousPointerAngle = FloatMath.Atan2(mouseDirection);
            }

            for (var i = 0; i < SpawnAmount; i++)
            {
                Handler.OnSpawnEvent(new SpawnEventData
                {
                    eventType = eventType,
                    origin = new SpawnOrigin
                    {
                        position = PointerClickPosition + positions[i],
                        rotation = PreviousPointerAngle
                    }
                });
            }
        }

        public void OnActivate(bool isActive)
        {
            PlaygroundUI.SetElementVisible(Window, isActive);
        }
    }
}
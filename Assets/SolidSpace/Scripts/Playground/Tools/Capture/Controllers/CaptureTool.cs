using System;
using System.Collections.Generic;
using System.Linq;
using SolidSpace.Entities.World;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.EntitySearch;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Capture
{
    internal class CaptureTool : ICaptureTool
    {
        public IEntitySearchSystem SearchSystem { get; set; }
        public IComponentFilter Filter { get; set; }
        public IPlaygroundUIManager PlaygroundUI { get; set; }
        public IUIManager UIManager { get; set; }
        public IPointerTracker Pointer { get; set; }
        public List<Entity> CapturedEntities { get; set; }
        public List<float2> CapturedPositions { get; set; }
        public float2 CapturedPointer { get; set; }
        public int SearchRadius { get; set; }
        public IStringField SearchRadiusField { get; set; }
        public IEntityManager EntityManager { get; set; }
        public ICaptureToolHandler Handler { get; set; }
        public IToolWindow Window { get; set; }

        public void OnActivate(bool isActive)
        {
            if (isActive)
            {
                UpdateSearchSystemQuery();
                CapturedEntities.Clear();
                CapturedPositions.Clear();
                SearchRadius = Math.Max(0, SearchRadius);
                SearchRadiusField.SetValue(SearchRadius.ToString());
                SearchSystem.SetSearchRadius(SearchRadius);
            }

            PlaygroundUI.SetElementVisible(Filter, isActive);
            PlaygroundUI.SetElementVisible(Window, isActive);
            SearchSystem.SetEnabled(isActive);
        }

        public void OnUpdate()
        {
            if (CapturedEntities.Count > 0)
            {
                OnCaptureUpdate();

                if (CapturedEntities.Count != 0)
                {
                    return;
                }
            }
            
            if (UIManager.IsMouseOver)
            {
                return;
            }
            
            SearchSystem.SetSearchPosition(Pointer.Position);
            
            if (SearchRadius == 0)
            {
                OnSingleSearchUpdate();
            }
            else
            {
                OnRadiusSearchUpdate();
            }
        }

        public EntityQuery CreateQueryFromCurrentFilter()
        {
            return EntityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = Filter.GetTagsWithState(ETagLabelState.Positive).ToArray(),
                None = Filter.GetTagsWithState(ETagLabelState.Negative).ToArray()
            });
        }

        private void OnCaptureUpdate()
        {
            var eventData = new CaptureEventData
            {
                eventType = ECaptureEventType.CaptureUpdate,
                currentPointer = Pointer.Position,
                startPointer = CapturedPointer,
            };

            for (var i = CapturedEntities.Count - 1; i >= 0; i--)
            {
                var entity = CapturedEntities[i];
                if (!EntityManager.CheckExists(entity))
                {
                    CapturedEntities.RemoveAt(i);
                    CapturedPositions.RemoveAt(i);
                    continue;
                }

                var entityPosition = CapturedPositions[i];
                eventData.entity = entity;
                eventData.entityPosition = entityPosition;
                Handler.OnCaptureEvent(eventData);
            }

            if (Pointer.IsHeldThisFrame)
            {
                return;
            }

            eventData.eventType = ECaptureEventType.CaptureEnd;
            for (var i = 0; i < CapturedEntities.Count; i++)
            {
                eventData.entity = CapturedEntities[i];
                eventData.entityPosition = CapturedPositions[i];
                Handler.OnCaptureEvent(eventData);
            }
            
            CapturedEntities.Clear();
            CapturedPositions.Clear();
        }

        private void OnSingleSearchUpdate()
        {
            var searchResult = SearchSystem.Result;
            if (!searchResult.isValid)
            {
                return;
            }

            var eventData = new CaptureEventData
            {
                eventType = ECaptureEventType.SelectionSingle,
                entity = searchResult.nearestEntity,
                entityPosition = searchResult.nearestPosition,
                currentPointer = Pointer.Position,
                startPointer = Pointer.Position
            };
            Handler.OnCaptureEvent(eventData);

            if (!Pointer.ClickedThisFrame)
            {
                return;
            }
            
            CapturedEntities.Add(searchResult.nearestEntity);
            CapturedPositions.Add(searchResult.nearestPosition);
            CapturedPointer = Pointer.Position;
            
            eventData.eventType = ECaptureEventType.CaptureStart;
            Handler.OnCaptureEvent(eventData);
        }
        
        private void OnRadiusSearchUpdate()
        {
            Handler.OnDrawSelectionCircle(Pointer.Position, SearchRadius);

            var searchResult = SearchSystem.Result;
            if (!searchResult.isValid)
            {
                return;
            }
            
            var eventData = new CaptureEventData
            {
                eventType = ECaptureEventType.SelectionMultiple,
                startPointer = Pointer.Position,
                currentPointer = Pointer.Position,
            };
            
            for (var i = 0; i < searchResult.inRadiusCount; i++)
            {
                eventData.entity = searchResult.inRadiusEntities[i];
                eventData.entityPosition = searchResult.inRadiusPositions[i];
                Handler.OnCaptureEvent(eventData);
            }

            if (!Pointer.ClickedThisFrame)
            {
                return;
            }

            eventData.eventType = ECaptureEventType.CaptureStart;
            CapturedPointer = Pointer.Position;

            for (var i = 0; i < searchResult.inRadiusCount; i++)
            {
                var entity = searchResult.inRadiusEntities[i];
                var position = searchResult.inRadiusPositions[i];
                
                if (!EntityManager.CheckExists(entity))
                {
                    continue;
                }
                
                CapturedEntities.Add(entity);
                CapturedPositions.Add(position);
                eventData.entity = entity;
                eventData.entityPosition = position;
                Handler.OnCaptureEvent(eventData);
            }
        }

        public void OnSearchRadiusFieldChange()
        {
            SearchRadius = int.Parse(SearchRadiusField.Value);
            SearchSystem.SetSearchRadius(SearchRadius);
        }
        
        public void UpdateSearchSystemQuery()
        {
            SearchSystem.SetQuery(new EntityQueryDesc
            {
                All = Filter.GetTagsWithState(ETagLabelState.Positive).ToArray(),
                None = Filter.GetTagsWithState(ETagLabelState.Negative).ToArray()
            });
        }
    }
}
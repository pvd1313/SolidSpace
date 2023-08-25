using System;
using System.Collections.Generic;
using System.Linq;
using SolidSpace.DependencyInjection;
using SolidSpace.Profiling;
using UnityEngine;

namespace SolidSpace.GameCycle
{
    internal class GameCycleController : IApplicationBootstrapper, IDisposable
    {
        private readonly GameCycleConfig _config;
        private readonly IProfilingManager _profilingManager;
        private readonly IProfilingProcessor _profilingProcessor;

        private readonly List<IInitializable> _initializables;
        private readonly List<IUpdatable> _updatables;
        
        private string[] _gameCycleNames;
        private IUpdatable[] _gameCycle;
        private IInitializable[] _initializationSequence;
        private UpdatingBehaviour _updateBehaviour;
        private ProfilingHandle _profilingHandle;

        public GameCycleController(List<IInitializable> initializables, List<IUpdatable> updatables, GameCycleConfig config,
            IProfilingManager profilingManager, IProfilingProcessor profilingProcessor)
        {
            _initializables = initializables;
            _updatables = updatables;
            _config = config;
            _profilingManager = profilingManager;
            _profilingProcessor = profilingProcessor;
        }
        
        public void Run()
        {
            _profilingProcessor.Initialize();
            _profilingHandle = _profilingManager.GetHandle(this);
            _gameCycle = PrepareSequence(_updatables, _config.UpdateOrder.Groups, "update");
            _initializationSequence = PrepareSequence(_initializables, _config.InitializationOrder.Groups, "initialization");

            foreach (var item in _initializationSequence)
            {
                item.OnInitialize();
            }

            _gameCycleNames = _gameCycle.Select(i => i.GetType().Name).ToArray();
            
            var gameObject = new GameObject(nameof(GameCycleController));
            _updateBehaviour = gameObject.AddComponent<UpdatingBehaviour>();
            _updateBehaviour.OnUpdate += OnUpdate;
        }

        private T[] PrepareSequence<T>(ICollection<T> instances, IReadOnlyCollection<ControllerGroup> order, string orderName)
        {
            var correctOrder = new Dictionary<Type, int>(); 
            var j = 0;
            foreach (var controllerReference in order.SelectMany(g => g.Controllers))
            {
                var type = controllerReference.Resolve();
                correctOrder.Add(type, j++);
            }
            
            var sequence = new List<(T, int)>(instances.Count);
            foreach (var instance in instances)
            {
                var type = instance.GetType();
                if (!correctOrder.TryGetValue(type, out var executionOrder))
                {
                    var message = $"Controller '{type.FullName}' is not defined in {orderName} order";
                    throw new InvalidOperationException(message);
                }

                sequence.Add((instance, executionOrder));
            }

            return sequence.OrderBy(i => i.Item2).Select(i => i.Item1).ToArray();
        }

        private void OnUpdate()
        {
            for (var i = 0; i < _gameCycle.Length; i++)
            {
                _profilingHandle.BeginSample(_gameCycleNames[i]);
                _gameCycle[i].OnUpdate();
                _profilingHandle.EndSample(_gameCycleNames[i]);
            }

            _profilingProcessor.Update();
        }

        public void Dispose()
        {
            _updateBehaviour.OnUpdate -= OnUpdate;

            for (var i = _initializationSequence.Length - 1; i >= 0; i--)
            {
                _initializationSequence[i].OnFinalize();
            }

            _profilingProcessor.FinalizeObject();
        }
    }
}
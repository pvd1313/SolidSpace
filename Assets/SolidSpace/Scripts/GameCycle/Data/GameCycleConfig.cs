using UnityEngine;

namespace SolidSpace.GameCycle
{
    [System.Serializable]
    internal class GameCycleConfig
    {
        public InitializationOrder InitializationOrder => _initializationOrder;
        public UpdateOrder UpdateOrder => _updateOrder;
        
        [SerializeField] private InitializationOrder _initializationOrder;
        [SerializeField] private UpdateOrder _updateOrder;
    }
}
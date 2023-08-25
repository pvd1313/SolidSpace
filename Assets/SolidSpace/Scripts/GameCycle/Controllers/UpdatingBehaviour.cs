using System;
using UnityEngine;

namespace SolidSpace.GameCycle
{
    internal class UpdatingBehaviour : MonoBehaviour
    {
        public event Action OnUpdate;

        private bool _isBroken;

        private void Update()
        {
            if (_isBroken)
            {
                return;
            }
            
            try
            {
                OnUpdate?.Invoke();
            }
            catch
            {
                _isBroken = true;
                
                throw;
            }
        }
    }
}
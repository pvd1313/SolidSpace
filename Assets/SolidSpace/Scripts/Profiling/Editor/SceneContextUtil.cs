using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Profiling.Editor
{
    public static class SceneContextUtil
    {
        private static bool ContextFound;
        private static bool IsPlaying;
        private static SceneContext Context;
        
        public static T TryResolve<T>() where T : class
        {
            if (!Application.isPlaying)
            {
                IsPlaying = false;
                
                return null;
            }
            
            if (!IsPlaying)
            {
                Context = Object.FindObjectOfType<SceneContext>();
                ContextFound = Context != null;
                IsPlaying = true;
            }
            
            if (!ContextFound)
            {
                return null;
            }
            
            return Context.TryResolve<T>();
        }
    }
}
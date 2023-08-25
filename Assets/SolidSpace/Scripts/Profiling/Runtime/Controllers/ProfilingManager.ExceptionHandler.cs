using System;
using System.Collections.Generic;

namespace SolidSpace.Profiling
{
    public partial class ProfilingManager
    {
        private struct ExceptionHandler
        {
            public void HandleJobState(IList<string> names, ProfilingBuildTreeJob job)
            {
                var state = job.outState[0];

                if (state.code == ETreeBuildCode.Success)
                {
                    return;
                }
                
                var stackTrace = BuildPath(names, job, state.stackLast);
                switch (state.code)
                {
                    case ETreeBuildCode.StackIsNotEmptyAfterJobComplete:
                        throw new InvalidOperationException($"EndSample() was not called for {stackTrace}");
                    
                    case ETreeBuildCode.StackOverflow:
                        throw new StackOverflowException($"{stackTrace}{names[state.recordLast + 1]} caused stack overflow");
                    
                    case ETreeBuildCode.StackUnderflow:
                        throw new InvalidOperationException($"BeginSample() is missing for {stackTrace}{names[state.recordLast + 1]}");
                    
                    case ETreeBuildCode.NameMismatch:
                        var newNodePath = BuildPath(names, job, state.stackLast - 1) + names[state.recordLast + 1];
                        throw new InvalidOperationException($"BeginSample({stackTrace}) does not match EndSample({newNodePath})");
                }

                throw new InvalidOperationException($"Error during tree building: {state.code}");
            }
            
            private string BuildPath(IList<string> names, ProfilingBuildTreeJob job, int stackLast)
            {
                var result = string.Empty;

                for (var i = 0; i <= stackLast; i++)
                {
                    result += names[job.outNames[job.parentStack[i]]] + "/";
                }

                return result;
            }
        }
    }
}
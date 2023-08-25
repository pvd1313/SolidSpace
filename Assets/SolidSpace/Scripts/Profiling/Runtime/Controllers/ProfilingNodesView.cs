using System;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.Profiling
{
    public class ProfilingNodesView
    {
        private readonly string[] _fractionText;
        private readonly string[] _exponentText;
        
        public ProfilingNodesView()
        {
            _fractionText = new string[100];
            for (var i = 0; i < 100; i++)
            {
                _fractionText[i] = "." + i.ToString("D2");
            }
            
            _exponentText = new string[100];
            for (var i = 0; i < 100; i++)
            {
                _exponentText[i] = i.ToString("D2");
            }
        }

        public void OnGUI(Rect rect, IEnumerable<ProfilingNode> nodes)
        {
            rect.height = 20;
            var timeRectLeft = new Rect(rect.width - 32, rect.y, 20, 20);
            var timeRectRight = new Rect(rect.width - 19, rect.y, 25, 20);
            
            foreach (var node in nodes)
            {
                rect.x = 20 * node.deep + 3;

                GUI.Label(rect, node.name);
                
                TimeToString(node.time, out var timeTextLeft, out var timeTextRight);
                
                if ((int) node.time > 0)
                {
                    GUI.Label(timeRectLeft, timeTextLeft);
                }
                
                GUI.Label(timeRectRight, timeTextRight);

                rect.y += 20;
                timeRectRight.y += 20;
                timeRectLeft.y += 20;
            }
        }
        
        private void TimeToString(float time, out string left, out string right)
        {
            left = _exponentText[Math.Min(99, (int) time)];
            right = _fractionText[(int) (time % 1 * 100)];
        }
    }
}
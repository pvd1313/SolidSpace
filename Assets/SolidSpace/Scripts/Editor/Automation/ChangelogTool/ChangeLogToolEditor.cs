#if ODIN_INSPECTOR

using System;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.Editor.Automation.ChangelogTool
{
    [CustomEditor(typeof(ChangelogToolAsset))]
    public class ChangeLogToolEditor : OdinEditor
    {
        [SerializeField] private string _inputText;
        [SerializeField] private string _outputText;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _inputText = EditorGUILayout.TextArea(_inputText, GUILayout.MaxHeight(400));

            if (GUILayout.Button("Convert"))
            {
                _outputText = Convert(_inputText, ((ChangelogToolAsset) target).Config);
            }

            _outputText = EditorGUILayout.TextArea(_outputText, GUILayout.MaxHeight(400));
        }

        private string Convert(string source, ChangelogToolConfig config)
        {
            var lines = source.Split(new [] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            var builder = new StringBuilder();
            var blacklist = config.Blacklist;
            var converters = config.Converters;
            
            foreach (var line in lines)
            {
                if (blacklist.Any(bp => bp.IsMatch(line)))
                {
                    continue;
                }

                var converted = line;
                foreach (var converter in converters)
                {
                    converted = converter.Replace(converted);
                }
                
                builder.Append(converted);
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }
    }
}

#endif
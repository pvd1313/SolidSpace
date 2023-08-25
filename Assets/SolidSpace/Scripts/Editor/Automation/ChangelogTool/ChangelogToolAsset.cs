using UnityEngine;

namespace SolidSpace.Editor.Automation.ChangelogTool
{
    public class ChangelogToolAsset : ScriptableObject
    {
        public ChangelogToolConfig Config => _config;
        
        [SerializeField] private ChangelogToolConfig _config;
    }
}
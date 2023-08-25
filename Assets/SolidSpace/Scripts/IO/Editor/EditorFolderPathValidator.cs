using System.IO;
using System.Text.RegularExpressions;
using SolidSpace.DataValidation;

namespace SolidSpace.IO.Editor
{
    [InspectorDataValidator]
    public class EditorFolderPathValidator : IDataValidator<EditorFolderPath>
    {
        private const string BlacklistRegex = @"(\\)|(\/$)|(^\/)|(\/\/)";

        public string Validate(EditorFolderPath data)
        {
            if (data.path is null)
            {
                return $"'{nameof(data.path)}' is null";
            }

            var match = Regex.Match(data.path, BlacklistRegex);
            if (match.Success)
            {
                return $"'{nameof(data.path)}' contains|starts|ends with '{match.Value}'";
            }

            var folder = EditorPath.Combine(EditorPath.ProjectRoot, data.path);
            if (!Directory.Exists(folder))
            {
                return $"Folder '{folder}' does not exist";
            }

            return string.Empty;
        }
    }
}
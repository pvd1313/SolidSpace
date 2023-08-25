#if !NOT_UNITY3D

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Zenject
{
    public static class ZenMenuItems
    {
        [MenuItem("Edit/Zenject/Help...")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/svermeulen/zenject");
        }

        [MenuItem("Assets/Create/Zenject/Default Scene Contract Config", false, 80)]
        public static void CreateDefaultSceneContractConfig()
        {
            var folderPath = ZenUnityEditorUtil.GetCurrentDirectoryAssetPathFromSelection();

            if (!folderPath.EndsWith("/Resources"))
            {
                EditorUtility.DisplayDialog("Error",
                    "ZenjectDefaultSceneContractConfig objects must be placed directly underneath a folder named 'Resources'.  Please try again.", "Ok");
                return;
            }

            var config = ScriptableObject.CreateInstance<DefaultSceneContractConfig>();

            ZenUnityEditorUtil.SaveScriptableObjectAsset(
                Path.Combine(folderPath, DefaultSceneContractConfig.ResourcePath + ".asset"), config);
        }

        [MenuItem("Assets/Create/Zenject/Scriptable Object Installer", false, 1)]
        public static void CreateScriptableObjectInstaller()
        {
            AddCSharpClassTemplate("Scriptable Object Installer", "UntitledInstaller",
                  "using UnityEngine;"
                + "\nusing Zenject;"
                + "\n"
                + "\n[CreateAssetMenu(fileName = \"CLASS_NAME\", menuName = \"Installers/CLASS_NAME\")]"
                + "\npublic class CLASS_NAME : ScriptableObjectInstaller<CLASS_NAME>"
                + "\n{"
                + "\n    public override void InstallBindings()"
                + "\n    {"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Mono Installer", false, 1)]
        public static void CreateMonoInstaller()
        {
            AddCSharpClassTemplate("Mono Installer", "UntitledInstaller",
                  "using UnityEngine;"
                + "\nusing Zenject;"
                + "\n"
                + "\npublic class CLASS_NAME : MonoInstaller"
                + "\n{"
                + "\n    public override void InstallBindings()"
                + "\n    {"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Installer", false, 1)]
        public static void CreateInstaller()
        {
            AddCSharpClassTemplate("Installer", "UntitledInstaller",
                  "using UnityEngine;"
                + "\nusing Zenject;"
                + "\n"
                + "\npublic class CLASS_NAME : Installer<CLASS_NAME>"
                + "\n{"
                + "\n    public override void InstallBindings()"
                + "\n    {"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Editor Window", false, 20)]
        public static void CreateEditorWindow()
        {
            AddCSharpClassTemplate("Editor Window", "UntitledEditorWindow",
                  "using UnityEngine;"
                + "\nusing UnityEditor;"
                + "\nusing Zenject;"
                + "\n"
                + "\npublic class CLASS_NAME : ZenjectEditorWindow"
                + "\n{"
                + "\n    [MenuItem(\"Window/CLASS_NAME\")]"
                + "\n    public static CLASS_NAME GetOrCreateWindow()"
                + "\n    {"
                + "\n        var window = EditorWindow.GetWindow<CLASS_NAME>();"
                + "\n        window.titleContent = new GUIContent(\"CLASS_NAME\");"
                + "\n        return window;"
                + "\n    }"
                + "\n"
                + "\n    public override void InstallBindings()"
                + "\n    {"
                + "\n        // TODO"
                + "\n    }"
                + "\n}");
        }

        public static string AddCSharpClassTemplate(
            string friendlyName, string defaultFileName, string templateStr)
        {
            return AddCSharpClassTemplate(
                friendlyName, defaultFileName, templateStr, ZenUnityEditorUtil.GetCurrentDirectoryAssetPathFromSelection());
        }

        public static string AddCSharpClassTemplate(
            string friendlyName, string defaultFileName,
            string templateStr, string folderPath)
        {
            var absolutePath = EditorUtility.SaveFilePanel(
                "Choose name for " + friendlyName,
                folderPath,
                defaultFileName + ".cs",
                "cs");

            if (absolutePath == "")
            {
                // Dialog was cancelled
                return null;
            }

            if (!absolutePath.ToLower().EndsWith(".cs"))
            {
                absolutePath += ".cs";
            }

            var className = Path.GetFileNameWithoutExtension(absolutePath);
            File.WriteAllText(absolutePath, templateStr.Replace("CLASS_NAME", className));

            AssetDatabase.Refresh();

            var assetPath = ZenUnityEditorUtil.ConvertFullAbsolutePathToAssetPath(absolutePath);

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

            return assetPath;
        }

        [MenuItem("Assets/Create/Zenject/Unit Test", false, 60)]
        public static void CreateUnitTest()
        {
            AddCSharpClassTemplate("Unit Test", "UntitledUnitTest",
                  "using Zenject;"
                + "\nusing NUnit.Framework;"
                + "\n"
                + "\n[TestFixture]"
                + "\npublic class CLASS_NAME : ZenjectUnitTestFixture"
                + "\n{"
                + "\n    [Test]"
                + "\n    public void RunTest1()"
                + "\n    {"
                + "\n        // TODO"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Integration Test", false, 60)]
        public static void CreateIntegrationTest()
        {
            AddCSharpClassTemplate("Integration Test", "UntitledIntegrationTest",
                  "using Zenject;"
                + "\nusing System.Collections;"
                + "\nusing UnityEngine.TestTools;"
                + "\n"
                + "\npublic class CLASS_NAME : ZenjectIntegrationTestFixture"
                + "\n{"
                + "\n    [UnityTest]"
                + "\n    public IEnumerator RunTest1()"
                + "\n    {"
                + "\n        // Setup initial state by creating game objects from scratch, loading prefabs/scenes, etc"
                + "\n"
                + "\n        PreInstall();"
                + "\n"
                + "\n        // Call Container.Bind methods"
                + "\n"
                + "\n        PostInstall();"
                + "\n"
                + "\n        // Add test assertions for expected state"
                + "\n        // Using Container.Resolve or [Inject] fields"
                + "\n        yield break;"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Scene Test", false, 60)]
        public static void CreateSceneTest()
        {
            AddCSharpClassTemplate("Scene Test Fixture", "UntitledSceneTest",
                  "using Zenject;"
                + "\nusing System.Collections;"
                + "\nusing UnityEngine;"
                + "\nusing UnityEngine.TestTools;"
                + "\n"
                + "\npublic class CLASS_NAME : SceneTestFixture"
                + "\n{"
                + "\n    [UnityTest]"
                + "\n    public IEnumerator TestScene()"
                + "\n    {"
                + "\n        yield return LoadScene(\"InsertSceneNameHere\");"
                + "\n"
                + "\n        // TODO: Add assertions here now that the scene has started"
                + "\n        // Or you can just uncomment to simply wait some time to make sure the scene plays without errors"
                + "\n        //yield return new WaitForSeconds(1.0f);"
                + "\n"
                + "\n        // Note that you can use SceneContainer.Resolve to look up objects that you need for assertions"
                + "\n    }"
                + "\n}");
        }
    }
}
#endif

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AppliedMath.Week3.Editor
{
    public static class Week3SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Week3TurretDefense.unity";

        [MenuItem("Applied Mathematics/Build Week 3 Scene")]
        public static void BuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Week3TurretDefense";
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

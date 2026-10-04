using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AppliedMath.Week4.Editor
{
    public static class Week4SceneBuilder
    {
        private const string Week3ScenePath = "Assets/Scenes/Week3TurretDefense.unity";
        private const string Week4ScenePath = "Assets/Scenes/Week4BezierDefense.unity";

        [MenuItem("Applied Mathematics/Build Week 4 Scene")]
        public static void BuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Week4BezierDefense";
            EditorSceneManager.SaveScene(scene, Week4ScenePath);

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
            buildScenes.Add(new EditorBuildSettingsScene(Week4ScenePath, true));
            if (System.IO.File.Exists(Week3ScenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(Week3ScenePath, true));
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

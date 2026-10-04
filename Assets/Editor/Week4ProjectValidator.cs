using System;
using System.IO;
using AppliedMath.Week3;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AppliedMath.Week4.Editor
{
    public static class Week4ProjectValidator
    {
        private const string ScenePath = "Assets/Scenes/Week4BezierDefense.unity";

        public static void ValidateProject()
        {
            ValidateBezierAndEasingMath();
            EditorSceneManager.OpenScene(ScenePath);
            Week4LevelBootstrap.BuildLevelForEditorValidation();
            ValidateRuntimeObjects();
            ValidateCreatureResolutionAndCoinSpawn();
            Debug.Log("WEEK4_VALIDATION_PASSED");
        }

        public static void RenderPreviewProject()
        {
            ValidateProject();
            RenderPreview();
            Debug.Log("WEEK4_PREVIEW_RENDERED");
        }

        private static void ValidateBezierAndEasingMath()
        {
            GameVector2 quadratic = ManualMath.QuadraticBezier(
                new GameVector2(0f, 0f),
                new GameVector2(1f, 2f),
                new GameVector2(2f, 0f),
                0.5f);
            AssertNear(quadratic.x, 1f, "Quadratic midpoint X");
            AssertNear(quadratic.y, 1f, "Quadratic midpoint Y");

            GameVector2 cubicStart = ManualMath.CubicBezier(
                new GameVector2(-2f, 0f),
                new GameVector2(-1f, 2f),
                new GameVector2(1f, -2f),
                new GameVector2(2f, 0f),
                0f);
            GameVector2 cubicEnd = ManualMath.CubicBezier(
                new GameVector2(-2f, 0f),
                new GameVector2(-1f, 2f),
                new GameVector2(1f, -2f),
                new GameVector2(2f, 0f),
                1f);
            AssertNear(cubicStart.x, -2f, "Cubic start X");
            AssertNear(cubicEnd.x, 2f, "Cubic end X");
            AssertNear(ManualMath.EaseOutCubic(0.5f), 0.875f, "Ease-out cubic midpoint");
        }

        private static void ValidateRuntimeObjects()
        {
            if (UnityEngine.Object.FindFirstObjectByType<Week4GameManager>() == null)
                throw new InvalidOperationException("Week 4 game manager is missing.");

            if (UnityEngine.Object.FindObjectsByType<BezierCreatureSpawner>(FindObjectsSortMode.None).Length != 2)
                throw new InvalidOperationException("Week 4 must contain exactly two creature spawners.");

            if (UnityEngine.Object.FindFirstObjectByType<Week4FlameTower>() == null ||
                UnityEngine.Object.FindFirstObjectByType<Week4SniperTower>() == null ||
                UnityEngine.Object.FindFirstObjectByType<Week4ShotgunTower>() == null)
            {
                throw new InvalidOperationException("One or more Week 4 towers are missing.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<GhostHealthBar>() == null ||
                UnityEngine.Object.FindFirstObjectByType<CoinBankUi>() == null)
            {
                throw new InvalidOperationException("The HP bar or coin bank UI is missing.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<ManualPlayerController>() != null)
                throw new InvalidOperationException("The movable Week 3 player must not exist in Week 4.");

            if (UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length > 0 ||
                UnityEngine.Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None).Length > 0 ||
                UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length > 0 ||
                UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Length > 0)
            {
                throw new InvalidOperationException("Week 4 contains a forbidden physics component.");
            }
        }

        private static void ValidateCreatureResolutionAndCoinSpawn()
        {
            Week4GameManager manager = UnityEngine.Object.FindFirstObjectByType<Week4GameManager>();
            GameVector2[] path =
            {
                new GameVector2(-1f, 0f),
                new GameVector2(0f, 1f),
                new GameVector2(1f, 0f)
            };

            GameObject defeatedObject = new GameObject("Validation Defeated Creature");
            BezierCreature defeated = defeatedObject.AddComponent<BezierCreature>();
            defeated.Configure(manager, BezierPathType.Quadratic, path, 2f, 0.3f);
            defeated.Kill();

            if (UnityEngine.Object.FindFirstObjectByType<FlyingCoin>() == null)
                throw new InvalidOperationException("A defeated creature did not create a flying coin.");

            GameObject escapedObject = new GameObject("Validation Escaped Creature");
            BezierCreature escaped = escapedObject.AddComponent<BezierCreature>();
            escaped.Configure(manager, BezierPathType.Quadratic, path, 2f, 0.3f);
            manager.CreatureReachedTarget(escaped);

            if (manager.ActiveCreatures.Count != 0)
                throw new InvalidOperationException("Resolved creatures remained in the active-creature list.");

            UnityEngine.Object.DestroyImmediate(escapedObject);
        }

        private static void RenderPreview()
        {
            Camera camera = Camera.main;
            if (camera == null)
                throw new InvalidOperationException("The generated Week 4 level has no main camera.");

            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            Texture2D image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();
            image.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
            image.Apply();
            File.WriteAllBytes(Path.GetFullPath("Week4Preview.png"), image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(image);
        }

        private static void AssertNear(float actual, float expected, string label)
        {
            if (Math.Abs(actual - expected) > 0.0001f)
            {
                throw new InvalidOperationException(
                    label + " failed. Expected " + expected + ", received " + actual + ".");
            }
        }
    }
}

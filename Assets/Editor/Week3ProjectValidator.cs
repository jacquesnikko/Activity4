using System;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AppliedMath.Week3.Editor
{
    public static class Week3ProjectValidator
    {
        private const string ScenePath = "Assets/Scenes/Week3TurretDefense.unity";

        public static void ValidateProject()
        {
            ValidateMath();
            EditorSceneManager.OpenScene(ScenePath);
            Week3LevelBootstrap.BuildLevelForEditorValidation();
            ValidateRuntimeObjects();
            Debug.Log("WEEK3_VALIDATION_PASSED");
        }

        public static void RenderPreviewProject()
        {
            ValidateProject();
            RenderPreview();
            Debug.Log("WEEK3_PREVIEW_RENDERED");
        }

        private static void ValidateMath()
        {
            AssertNear(ManualMath.Lerp(10f, 20f, 0.3f), 13f, "Scalar lerp");

            GameVector2 right = new GameVector2(1f, 0f);
            GameVector2 up = new GameVector2(0f, 1f);
            AssertNear(ManualMath.Dot(right, right), 1f, "Parallel dot product");
            AssertNear(ManualMath.Dot(right, up), 0f, "Perpendicular dot product");

            if (!ManualMath.IsInsideCone(
                GameVector2.zero,
                right,
                new GameVector2(3f, 1f),
                5f,
                60f))
            {
                throw new InvalidOperationException("Cone test rejected a point inside the cone.");
            }

            if (ManualMath.IsInsideCone(
                GameVector2.zero,
                right,
                new GameVector2(-2f, 0f),
                5f,
                60f))
            {
                throw new InvalidOperationException("Cone test accepted a point behind the cone.");
            }

            if (!ManualMath.IsInsideSightLine(
                GameVector2.zero,
                right,
                new GameVector2(4f, 0.1f),
                6f,
                0.2f))
            {
                throw new InvalidOperationException("Sight-line test rejected a point inside the line.");
            }

            if (!ManualMath.SegmentIntersectsCircle(
                new GameVector2(-2f, 0f),
                new GameVector2(2f, 0f),
                GameVector2.zero,
                0.25f))
            {
                throw new InvalidOperationException("Swept projectile test missed a circle intersection.");
            }

            GameAabb wall = new GameAabb(GameVector2.zero, new GameVector2(1f, 1f));
            if (!ManualMath.SegmentIntersectsAabb(
                new GameVector2(-3f, 0f),
                new GameVector2(3f, 0f),
                wall,
                0.1f))
            {
                throw new InvalidOperationException("Swept projectile test missed a wall intersection.");
            }
        }

        private static void ValidateRuntimeObjects()
        {
            if (UnityEngine.Object.FindFirstObjectByType<ManualGameManager>() == null)
                throw new InvalidOperationException("The runtime level did not create its game manager.");

            if (UnityEngine.Object.FindFirstObjectByType<ManualPlayerController>() == null)
                throw new InvalidOperationException("The runtime level did not create its player.");

            if (UnityEngine.Object.FindFirstObjectByType<FlameTurret>() == null ||
                UnityEngine.Object.FindFirstObjectByType<SniperTurret>() == null ||
                UnityEngine.Object.FindFirstObjectByType<ShotgunTurret>() == null)
            {
                throw new InvalidOperationException("One or more turret types are missing.");
            }

            if (UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length > 0 ||
                UnityEngine.Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None).Length > 0 ||
                UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length > 0 ||
                UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Length > 0)
            {
                throw new InvalidOperationException("The generated level contains a forbidden physics component.");
            }
        }

        private static void RenderPreview()
        {
            Camera camera = Camera.main;
            if (camera == null)
                throw new InvalidOperationException("The generated level has no main camera.");

            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            Texture2D image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;

            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();
            image.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
            image.Apply();

            string outputPath = Path.GetFullPath("Week3Preview.png");
            File.WriteAllBytes(outputPath, image.EncodeToPNG());

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

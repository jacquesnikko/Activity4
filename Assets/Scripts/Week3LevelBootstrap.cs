using UnityEngine;
using UnityEngine.UI;

namespace AppliedMath.Week3
{
    public static class Week3LevelBootstrap
    {
        private static readonly Color BackgroundColor = new Color(0.035f, 0.055f, 0.085f);
        private static readonly Color FloorColor = new Color(0.08f, 0.11f, 0.16f);
        private static readonly Color WallColor = new Color(0.18f, 0.23f, 0.31f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildLevel()
        {
            if (Object.FindFirstObjectByType<ManualGameManager>() != null)
            {
                return;
            }

            ConfigureCamera();

            GameObject managerObject = new GameObject("Week 3 Manual Game Manager");
            ManualGameManager manager = managerObject.AddComponent<ManualGameManager>();

            CreateArena();
            GameAabb[] obstacles = CreateObstacles();
            ManualPlayerController player = CreatePlayer(manager);
            GameVector2 goalPosition = new GameVector2(9.15f, 0f);
            CreateGoal(goalPosition);
            GameObject winPanel = CreateInterface();

            manager.Configure(player, goalPosition, 0.65f, obstacles, winPanel);
            CreateTurrets(manager);
        }

#if UNITY_EDITOR
        public static void BuildLevelForEditorValidation()
        {
            BuildLevel();
        }
#endif

        private static void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 6.1f;
            camera.backgroundColor = BackgroundColor;
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        private static void CreateArena()
        {
            GameObject floor = Week3RuntimeFactory.CreateSquare(
                "Arena Floor",
                GameVector2.zero,
                new GameVector2(21.6f, 10.8f),
                FloorColor,
                0.8f);
            floor.GetComponent<SpriteRenderer>().sortingOrder = -10;

            CreateWallVisual("Top Boundary", new GameVector2(0f, 5.45f), new GameVector2(21.6f, 0.22f));
            CreateWallVisual("Bottom Boundary", new GameVector2(0f, -5.45f), new GameVector2(21.6f, 0.22f));
            CreateWallVisual("Left Boundary", new GameVector2(-10.7f, 0f), new GameVector2(0.22f, 10.8f));
            CreateWallVisual("Right Boundary", new GameVector2(10.7f, 0f), new GameVector2(0.22f, 10.8f));
        }

        private static GameAabb[] CreateObstacles()
        {
            GameAabb[] obstacles =
            {
                new GameAabb(new GameVector2(-5.15f, 0.35f), new GameVector2(0.55f, 2.25f)),
                new GameAabb(new GameVector2(-0.6f, -2.25f), new GameVector2(1.55f, 0.55f)),
                new GameAabb(new GameVector2(4.0f, 1.8f), new GameVector2(1.4f, 0.55f))
            };

            for (int index = 0; index < obstacles.Length; index++)
            {
                GameAabb obstacle = obstacles[index];
                CreateWallVisual(
                    "Manual Collision Wall " + (index + 1),
                    obstacle.center,
                    obstacle.halfSize * 2f);
            }

            return obstacles;
        }

        private static void CreateWallVisual(string name, GameVector2 center, GameVector2 size)
        {
            GameObject wall = Week3RuntimeFactory.CreateSquare(name, center, size, WallColor, 0.2f);
            wall.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }

        private static ManualPlayerController CreatePlayer(ManualGameManager manager)
        {
            GameObject playerObject = Week3RuntimeFactory.CreateSquare(
                "Player",
                new GameVector2(-9.25f, 0f),
                new GameVector2(0.68f, 0.68f),
                new Color(0.2f, 1f, 0.55f),
                -0.3f);
            playerObject.GetComponent<SpriteRenderer>().sortingOrder = 6;
            ManualPlayerController player = playerObject.AddComponent<ManualPlayerController>();
            player.Configure(manager);
            return player;
        }

        private static void CreateGoal(GameVector2 position)
        {
            GameObject goal = Week3RuntimeFactory.CreateSquare(
                "Goal",
                position,
                new GameVector2(1.15f, 1.8f),
                new Color(0.15f, 0.9f, 0.35f, 0.72f),
                0.1f);
            goal.GetComponent<SpriteRenderer>().sortingOrder = 1;

            LineRenderer outline = Week3RuntimeFactory.CreateLine(
                goal,
                "Goal Outline",
                new Color(0.55f, 1f, 0.65f),
                0.07f,
                2);
            outline.positionCount = 5;
            outline.SetPosition(0, new Vector3(position.x - 0.65f, position.y - 1f, -0.1f));
            outline.SetPosition(1, new Vector3(position.x + 0.65f, position.y - 1f, -0.1f));
            outline.SetPosition(2, new Vector3(position.x + 0.65f, position.y + 1f, -0.1f));
            outline.SetPosition(3, new Vector3(position.x - 0.65f, position.y + 1f, -0.1f));
            outline.SetPosition(4, new Vector3(position.x - 0.65f, position.y - 1f, -0.1f));
        }

        private static void CreateTurrets(ManualGameManager manager)
        {
            GameObject flameObject = CreateTurretVisual(
                "Flame Turret",
                new GameVector2(-3.4f, 4.35f),
                new Color(1f, 0.28f, 0.08f));
            FlameTurret flame = flameObject.AddComponent<FlameTurret>();
            flame.Configure(manager, -90f, 4.8f, 64f, 10f, 52f);

            GameObject sniperObject = CreateTurretVisual(
                "Sniper Turret",
                new GameVector2(1.15f, 4.45f),
                new Color(0.2f, 0.65f, 1f));
            SniperTurret sniper = sniperObject.AddComponent<SniperTurret>();
            sniper.Configure(manager, -90f, 8.9f, 0.16f);

            GameObject shotgunObject = CreateTurretVisual(
                "Shotgun Turret",
                new GameVector2(6.0f, -4.2f),
                new Color(0.75f, 0.35f, 1f));
            ShotgunTurret shotgun = shotgunObject.AddComponent<ShotgunTurret>();
            shotgun.Configure(manager, 140f, 5.1f, 72f, 1.45f, 42f, 7);
        }

        private static GameObject CreateTurretVisual(
            string name,
            GameVector2 position,
            Color color)
        {
            GameObject turret = Week3RuntimeFactory.CreateSquare(
                name,
                position,
                new GameVector2(0.9f, 0.9f),
                color,
                -0.15f);
            turret.GetComponent<SpriteRenderer>().sortingOrder = 3;
            Week3RuntimeFactory.CreateBarrel(turret, Color.white);
            return turret;
        }

        private static GameObject CreateInterface()
        {
            GameObject canvasObject = new GameObject("Game UI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            CreateUiText(
                canvasObject.transform,
                "Title",
                "WEEK 3  •  MANUAL TURRET DEFENSE",
                28,
                TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -24f),
                new Vector2(900f, 55f),
                Color.white);

            CreateUiText(
                canvasObject.transform,
                "Instructions",
                "Move: WASD / Arrow Keys     Reach the green goal     Any projectile hit restarts the scene",
                20,
                TextAnchor.LowerCenter,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 22f),
                new Vector2(1200f, 50f),
                new Color(0.82f, 0.88f, 0.95f));

            GameObject winPanel = new GameObject("Win Panel");
            winPanel.transform.SetParent(canvasObject.transform, false);
            RectTransform panelTransform = winPanel.AddComponent<RectTransform>();
            panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
            panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
            panelTransform.sizeDelta = new Vector2(720f, 230f);
            Image panelImage = winPanel.AddComponent<Image>();
            panelImage.color = new Color(0.02f, 0.08f, 0.06f, 0.94f);

            CreateUiText(
                winPanel.transform,
                "Win Message",
                "GOAL REACHED\nAll turrets are disabled",
                46,
                TextAnchor.MiddleCenter,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.35f, 1f, 0.55f));

            return winPanel;
        }

        private static Text CreateUiText(
            Transform parent,
            string name,
            string content,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMinimum,
            Vector2 anchorMaximum,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rectangle = textObject.AddComponent<RectTransform>();
            rectangle.anchorMin = anchorMinimum;
            rectangle.anchorMax = anchorMaximum;
            rectangle.anchoredPosition = anchoredPosition;
            rectangle.sizeDelta = size;

            Text text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
    }
}

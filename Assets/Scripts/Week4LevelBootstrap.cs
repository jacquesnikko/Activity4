using AppliedMath.Week3;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AppliedMath.Week4
{
    public static class Week4LevelBootstrap
    {
        private const int CreaturesPerPath = 12;

        private sealed class UiReferences
        {
            public GhostHealthBar healthBar;
            public CoinBankUi coinBank;
            public GameObject statePanel;
            public Text stateText;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildLevel()
        {
            if (!SceneManager.GetActiveScene().name.StartsWith("Week4") ||
                Object.FindFirstObjectByType<Week4GameManager>() != null)
            {
                return;
            }

            Camera camera = ConfigureCamera();
            CreateArena();

            GameObject managerObject = new GameObject("Week 4 Game Manager");
            Week4GameManager manager = managerObject.AddComponent<Week4GameManager>();
            UiReferences ui = CreateInterface(camera);
            manager.Configure(
                20,
                CreaturesPerPath * 2,
                ui.healthBar,
                ui.coinBank,
                ui.statePanel,
                ui.stateText);

            GameVector2 target = new GameVector2(8.75f, 0f);
            CreateTarget(target);
            CreatePathsAndSpawners(manager, target);
            CreateTowers(manager);
        }

#if UNITY_EDITOR
        public static void BuildLevelForEditorValidation()
        {
            BuildLevel();
        }
#endif

        private static Camera ConfigureCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 6.1f;
            camera.backgroundColor = new Color(0.025f, 0.04f, 0.065f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            return camera;
        }

        private static void CreateArena()
        {
            GameObject floor = Week3RuntimeFactory.CreateSquare(
                "Week 4 Arena",
                GameVector2.zero,
                new GameVector2(21.6f, 10.8f),
                new Color(0.07f, 0.095f, 0.14f),
                0.8f);
            floor.GetComponent<SpriteRenderer>().sortingOrder = -10;

            Color borderColor = new Color(0.18f, 0.24f, 0.34f);
            CreateBlock("Top Border", new GameVector2(0f, 5.45f), new GameVector2(21.6f, 0.2f), borderColor, -5);
            CreateBlock("Bottom Border", new GameVector2(0f, -5.45f), new GameVector2(21.6f, 0.2f), borderColor, -5);
            CreateBlock("Left Border", new GameVector2(-10.7f, 0f), new GameVector2(0.2f, 10.8f), borderColor, -5);
            CreateBlock("Right Border", new GameVector2(10.7f, 0f), new GameVector2(0.2f, 10.8f), borderColor, -5);
        }

        private static void CreatePathsAndSpawners(Week4GameManager manager, GameVector2 target)
        {
            GameVector2[] quadraticPoints =
            {
                new GameVector2(-9.4f, 3.7f),
                new GameVector2(-1.8f, 5.05f),
                target
            };

            GameVector2[] cubicPoints =
            {
                new GameVector2(-9.4f, -3.7f),
                new GameVector2(-5.2f, 3.65f),
                new GameVector2(3.25f, -4.5f),
                target
            };

            DrawPath("Quadratic Bézier Path", BezierPathType.Quadratic, quadraticPoints, new Color(0.1f, 0.85f, 0.8f));
            DrawPath("Cubic Bézier Path", BezierPathType.Cubic, cubicPoints, new Color(1f, 0.35f, 0.7f));
            DrawControlPoints("Quadratic Controls", quadraticPoints, new Color(0.1f, 0.85f, 0.8f, 0.75f));
            DrawControlPoints("Cubic Controls", cubicPoints, new Color(1f, 0.35f, 0.7f, 0.75f));

            GameObject quadraticSpawnerObject = new GameObject("Quadratic Spawn Point");
            quadraticSpawnerObject.transform.position = quadraticPoints[0].ToVector3();
            BezierCreatureSpawner quadraticSpawner = quadraticSpawnerObject.AddComponent<BezierCreatureSpawner>();
            quadraticSpawner.Configure(
                manager,
                BezierPathType.Quadratic,
                quadraticPoints,
                CreaturesPerPath,
                0.92f,
                0.4f,
                11.5f,
                new Color(0.1f, 0.95f, 0.85f));

            GameObject cubicSpawnerObject = new GameObject("Cubic Spawn Point");
            cubicSpawnerObject.transform.position = cubicPoints[0].ToVector3();
            BezierCreatureSpawner cubicSpawner = cubicSpawnerObject.AddComponent<BezierCreatureSpawner>();
            cubicSpawner.Configure(
                manager,
                BezierPathType.Cubic,
                cubicPoints,
                CreaturesPerPath,
                1.02f,
                0.85f,
                12.4f,
                new Color(1f, 0.35f, 0.72f));
        }

        private static void DrawPath(
            string name,
            BezierPathType pathType,
            GameVector2[] points,
            Color color)
        {
            GameObject pathObject = new GameObject(name);
            LineRenderer line = Week3RuntimeFactory.CreateLine(pathObject, "Visible Path", color, 0.055f, -1);
            const int samples = 64;
            line.positionCount = samples + 1;

            for (int index = 0; index <= samples; index++)
            {
                float t = index / (float)samples;
                GameVector2 point = pathType == BezierPathType.Quadratic
                    ? ManualMath.QuadraticBezier(points[0], points[1], points[2], t)
                    : ManualMath.CubicBezier(points[0], points[1], points[2], points[3], t);
                line.SetPosition(index, point.ToVector3(0.25f));
            }
        }

        private static void DrawControlPoints(string name, GameVector2[] points, Color color)
        {
            GameObject group = new GameObject(name);
            for (int index = 0; index < points.Length; index++)
            {
                GameObject marker = CreateBlock(
                    name + " P" + index,
                    points[index],
                    new GameVector2(index == 0 ? 0.5f : 0.3f, index == 0 ? 0.5f : 0.3f),
                    color,
                    0);
                marker.transform.SetParent(group.transform, true);
            }
        }

        private static void CreateTarget(GameVector2 position)
        {
            CreateBlock(
                "Shared Base Target",
                position,
                new GameVector2(1.15f, 2.15f),
                new Color(0.95f, 0.18f, 0.22f),
                2);

            GameObject outlineOwner = new GameObject("Base Target Outline");
            LineRenderer outline = Week3RuntimeFactory.CreateLine(
                outlineOwner,
                "Base Outline",
                new Color(1f, 0.65f, 0.2f),
                0.075f,
                3);
            outline.positionCount = 5;
            outline.SetPosition(0, new Vector3(position.x - 0.7f, position.y - 1.25f, -0.1f));
            outline.SetPosition(1, new Vector3(position.x + 0.7f, position.y - 1.25f, -0.1f));
            outline.SetPosition(2, new Vector3(position.x + 0.7f, position.y + 1.25f, -0.1f));
            outline.SetPosition(3, new Vector3(position.x - 0.7f, position.y + 1.25f, -0.1f));
            outline.SetPosition(4, new Vector3(position.x - 0.7f, position.y - 1.25f, -0.1f));
        }

        private static void CreateTowers(Week4GameManager manager)
        {
            GameObject flameObject = CreateTowerVisual(
                "Flame Tower",
                new GameVector2(-3.4f, 1.35f),
                new Color(1f, 0.28f, 0.06f));
            flameObject.AddComponent<Week4FlameTower>().Configure(manager);

            GameObject sniperObject = CreateTowerVisual(
                "Sniper Tower",
                new GameVector2(1.0f, 1.25f),
                new Color(0.2f, 0.65f, 1f));
            sniperObject.AddComponent<Week4SniperTower>().Configure(manager);

            GameObject shotgunObject = CreateTowerVisual(
                "Shotgun Tower",
                new GameVector2(5.25f, 1.9f),
                new Color(0.75f, 0.35f, 1f));
            shotgunObject.AddComponent<Week4ShotgunTower>().Configure(manager);
        }

        private static GameObject CreateTowerVisual(string name, GameVector2 position, Color color)
        {
            GameObject tower = CreateBlock(name, position, new GameVector2(0.86f, 0.86f), color, 4);
            Week3RuntimeFactory.CreateBarrel(tower, Color.white);
            return tower;
        }

        private static GameObject CreateBlock(
            string name,
            GameVector2 position,
            GameVector2 size,
            Color color,
            int sortingOrder)
        {
            GameObject block = Week3RuntimeFactory.CreateSquare(name, position, size, color, 0f);
            block.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
            return block;
        }

        private static UiReferences CreateInterface(Camera camera)
        {
            GameObject canvasObject = new GameObject("Week 4 UI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            RectTransform canvasRectangle = canvasObject.GetComponent<RectTransform>();

            CreateText(
                canvasObject.transform,
                "Title",
                "ACTIVITY 4  •  BÉZIER DEFENSE",
                26,
                TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -20f),
                new Vector2(760f, 50f),
                Color.white);

            GameObject healthPanel = CreateUiPanel(
                canvasObject.transform,
                "Health Panel",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(300f, -42f),
                new Vector2(500f, 92f),
                new Color(0.025f, 0.04f, 0.065f, 0.92f));

            Text hpLabel = CreateText(
                healthPanel.transform,
                "HP Label",
                "BASE HP  20 / 20",
                22,
                TextAnchor.UpperLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(30f, -14f),
                new Vector2(430f, 34f),
                Color.white);

            CreateFillImage(
                healthPanel.transform,
                "HP Background",
                new Vector2(30f, -58f),
                new Vector2(440f, 22f),
                new Color(0.12f, 0.15f, 0.2f),
                false);
            Image ghost = CreateFillImage(
                healthPanel.transform,
                "Ghost HP",
                new Vector2(30f, -58f),
                new Vector2(440f, 22f),
                new Color(1f, 0.64f, 0.12f),
                true);
            Image real = CreateFillImage(
                healthPanel.transform,
                "Real HP",
                new Vector2(30f, -58f),
                new Vector2(440f, 22f),
                new Color(0.95f, 0.16f, 0.22f),
                true);

            GhostHealthBar healthBar = healthPanel.AddComponent<GhostHealthBar>();
            healthBar.Configure(real, ghost, hpLabel);

            GameObject bankPanel = CreateUiPanel(
                canvasObject.transform,
                "Coin Bank",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-205f, -42f),
                new Vector2(330f, 84f),
                new Color(0.025f, 0.04f, 0.065f, 0.92f));
            Text bankLabel = CreateText(
                bankPanel.transform,
                "Bank Label",
                "BANK  100",
                28,
                TextAnchor.MiddleCenter,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                Vector2.zero,
                Vector2.zero,
                new Color(1f, 0.82f, 0.18f));

            CoinBankUi coinBank = bankPanel.AddComponent<CoinBankUi>();
            coinBank.Configure(
                canvasRectangle,
                bankPanel.GetComponent<RectTransform>(),
                bankLabel,
                camera,
                100);

            GameObject statePanel = CreateUiPanel(
                canvasObject.transform,
                "Game State Panel",
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(720f, 220f),
                new Color(0.02f, 0.04f, 0.07f, 0.96f));
            Text stateText = CreateText(
                statePanel.transform,
                "Game State Text",
                "WAVE CLEARED",
                44,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                Color.white);

            CreateText(
                canvasObject.transform,
                "Legend",
                "CYAN: quadratic path     PINK: cubic path     Orange bar: delayed ghost HP",
                18,
                TextAnchor.LowerCenter,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 18f),
                new Vector2(1100f, 40f),
                new Color(0.82f, 0.88f, 0.95f));

            return new UiReferences
            {
                healthBar = healthBar,
                coinBank = coinBank,
                statePanel = statePanel,
                stateText = stateText
            };
        }

        private static GameObject CreateUiPanel(
            Transform parent,
            string name,
            Vector2 anchorMinimum,
            Vector2 anchorMaximum,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rectangle = panel.AddComponent<RectTransform>();
            rectangle.anchorMin = anchorMinimum;
            rectangle.anchorMax = anchorMaximum;
            rectangle.anchoredPosition = anchoredPosition;
            rectangle.sizeDelta = size;
            Image image = panel.AddComponent<Image>();
            image.sprite = Week3RuntimeFactory.SquareSprite;
            image.color = color;
            return panel;
        }

        private static Image CreateFillImage(
            Transform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color,
            bool filled)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);
            RectTransform rectangle = imageObject.AddComponent<RectTransform>();
            rectangle.anchorMin = new Vector2(0f, 1f);
            rectangle.anchorMax = new Vector2(0f, 1f);
            rectangle.pivot = new Vector2(0f, 0.5f);
            rectangle.anchoredPosition = anchoredPosition;
            rectangle.sizeDelta = size;

            Image image = imageObject.AddComponent<Image>();
            image.sprite = Week3RuntimeFactory.SquareSprite;
            image.color = color;
            if (filled)
            {
                image.type = Image.Type.Filled;
                image.fillMethod = Image.FillMethod.Horizontal;
                image.fillOrigin = 0;
                image.fillAmount = 1f;
            }

            return image;
        }

        private static Text CreateText(
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

using UnityEngine;

namespace AppliedMath.Week3
{
    public static class Week3RuntimeFactory
    {
        private static Sprite squareSprite;
        private static Material lineMaterial;

        public static Sprite SquareSprite
        {
            get
            {
                if (squareSprite == null)
                {
                    squareSprite = Sprite.Create(
                        Texture2D.whiteTexture,
                        new Rect(0f, 0f, 1f, 1f),
                        new Vector2(0.5f, 0.5f),
                        1f);
                    squareSprite.name = "Runtime White Square";
                }

                return squareSprite;
            }
        }

        public static Material LineMaterial
        {
            get
            {
                if (lineMaterial == null)
                {
                    Shader shader = Shader.Find("Sprites/Default");
                    lineMaterial = new Material(shader);
                    lineMaterial.name = "Runtime Line Material";
                }

                return lineMaterial;
            }
        }

        public static GameObject CreateSquare(
            string name,
            GameVector2 position,
            GameVector2 size,
            Color color,
            float z = 0f)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.position = position.ToVector3(z);
            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = color;
            return gameObject;
        }

        public static LineRenderer CreateLine(
            GameObject owner,
            string name,
            Color color,
            float width,
            int sortingOrder = 0)
        {
            GameObject lineObject = new GameObject(name);
            lineObject.transform.SetParent(owner.transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = false;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.material = LineMaterial;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            line.sortingOrder = sortingOrder;
            return line;
        }

        public static void CreateBarrel(GameObject owner, Color color)
        {
            LineRenderer barrel = CreateLine(owner, "Facing Direction", color, 0.16f, 4);
            barrel.useWorldSpace = false;
            barrel.positionCount = 2;
            barrel.SetPosition(0, new Vector3(0f, 0f, -0.02f));
            barrel.SetPosition(1, new Vector3(0.65f, 0f, -0.02f));
        }

        public static ManualProjectile CreateProjectile(
            ManualGameManager manager,
            string name,
            GameVector2 position,
            GameVector2 direction,
            float speed,
            float radius,
            float lifetime,
            Color color)
        {
            GameObject projectileObject = CreateSquare(
                name,
                position,
                new GameVector2(radius * 2f, radius * 2f),
                color,
                -0.2f);
            SpriteRenderer renderer = projectileObject.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = 5;
            ManualProjectile projectile = projectileObject.AddComponent<ManualProjectile>();
            projectile.Configure(manager, direction, speed, radius, lifetime);
            return projectile;
        }
    }
}

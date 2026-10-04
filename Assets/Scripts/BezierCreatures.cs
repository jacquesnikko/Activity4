using AppliedMath.Week3;
using UnityEngine;

namespace AppliedMath.Week4
{
    public enum BezierPathType
    {
        Quadratic,
        Cubic
    }

    public sealed class BezierCreature : MonoBehaviour
    {
        private Week4GameManager gameManager;
        private BezierPathType pathType;
        private GameVector2[] controlPoints;
        private float travelDuration;
        private float elapsed;

        public bool IsAlive { get; private set; }
        public float CollisionRadius { get; private set; }
        public GameVector2 Position => GameVector2.FromUnity(transform.position);

        public void Configure(
            Week4GameManager manager,
            BezierPathType type,
            GameVector2[] points,
            float duration,
            float collisionRadius)
        {
            gameManager = manager;
            pathType = type;
            controlPoints = points;
            travelDuration = duration;
            CollisionRadius = collisionRadius;
            elapsed = 0f;
            IsAlive = true;
            transform.position = Evaluate(0f).ToVector3(-0.25f);
            gameManager.RegisterCreature(this);
        }

        private void Update()
        {
            if (!IsAlive || !gameManager.IsRunning)
            {
                return;
            }

            elapsed += Time.deltaTime;
            float t = ManualMath.Clamp01(elapsed / travelDuration);
            GameVector2 position = Evaluate(t);
            transform.position = position.ToVector3(-0.25f);

            float lookAheadT = ManualMath.Clamp01(t + 0.01f);
            GameVector2 direction = Evaluate(lookAheadT) - position;
            if (ManualMath.LengthSquared(direction) > ManualMath.Epsilon)
            {
                float angle = ManualMath.AngleDegrees(direction);
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            if (t >= 1f)
            {
                IsAlive = false;
                gameManager.CreatureReachedTarget(this);
                DestroyCreatureObject();
            }
        }

        public void Kill()
        {
            if (!IsAlive)
            {
                return;
            }

            IsAlive = false;
            GameVector2 deathPosition = Position;
            gameManager.CreatureKilled(this, deathPosition);
            DestroyCreatureObject();
        }

        private void DestroyCreatureObject()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(gameObject);
                return;
            }
#endif
            Destroy(gameObject);
        }

        private GameVector2 Evaluate(float t)
        {
            if (pathType == BezierPathType.Quadratic)
            {
                return ManualMath.QuadraticBezier(
                    controlPoints[0],
                    controlPoints[1],
                    controlPoints[2],
                    t);
            }

            return ManualMath.CubicBezier(
                controlPoints[0],
                controlPoints[1],
                controlPoints[2],
                controlPoints[3],
                t);
        }
    }

    public sealed class BezierCreatureSpawner : MonoBehaviour
    {
        private Week4GameManager gameManager;
        private BezierPathType pathType;
        private GameVector2[] controlPoints;
        private float creatureTravelDuration;
        private float spawnInterval;
        private float spawnTimer;
        private int remainingCreatures;
        private Color creatureColor;
        private int sequence;

        public void Configure(
            Week4GameManager manager,
            BezierPathType type,
            GameVector2[] points,
            int count,
            float interval,
            float initialDelay,
            float travelDuration,
            Color color)
        {
            gameManager = manager;
            pathType = type;
            controlPoints = points;
            remainingCreatures = count;
            spawnInterval = interval;
            spawnTimer = initialDelay;
            creatureTravelDuration = travelDuration;
            creatureColor = color;
        }

        private void Update()
        {
            if (!gameManager.IsRunning || remainingCreatures <= 0)
            {
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
            {
                return;
            }

            spawnTimer = spawnInterval;
            remainingCreatures--;
            SpawnCreature();
        }

        private void SpawnCreature()
        {
            sequence++;
            GameObject creatureObject = Week3RuntimeFactory.CreateSquare(
                pathType + " Creature " + sequence,
                controlPoints[0],
                new GameVector2(0.62f, 0.48f),
                creatureColor,
                -0.25f);
            creatureObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
            BezierCreature creature = creatureObject.AddComponent<BezierCreature>();
            creature.Configure(
                gameManager,
                pathType,
                controlPoints,
                creatureTravelDuration,
                0.31f);
        }
    }
}

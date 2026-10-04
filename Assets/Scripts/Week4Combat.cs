using AppliedMath.Week3;
using UnityEngine;

namespace AppliedMath.Week4
{
    public sealed class Week4Projectile : MonoBehaviour
    {
        private Week4GameManager gameManager;
        private GameVector2 direction;
        private float speed;
        private float radius;
        private float remainingLifetime;

        public void Configure(
            Week4GameManager manager,
            GameVector2 travelDirection,
            float travelSpeed,
            float collisionRadius,
            float lifetime)
        {
            gameManager = manager;
            direction = ManualMath.NormalizeSafe(travelDirection);
            speed = travelSpeed;
            radius = collisionRadius;
            remainingLifetime = lifetime;
        }

        private void Update()
        {
            if (!gameManager.IsRunning)
            {
                Destroy(gameObject);
                return;
            }

            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            GameVector2 start = GameVector2.FromUnity(transform.position);
            GameVector2 end = start + direction * (speed * Time.deltaTime);

            for (int index = gameManager.ActiveCreatures.Count - 1; index >= 0; index--)
            {
                BezierCreature creature = gameManager.ActiveCreatures[index];
                if (creature == null || !creature.IsAlive)
                {
                    continue;
                }

                if (ManualMath.SegmentIntersectsCircle(
                    start,
                    end,
                    creature.Position,
                    creature.CollisionRadius + radius))
                {
                    creature.Kill();
                    Destroy(gameObject);
                    return;
                }
            }

            transform.position = end.ToVector3(-0.3f);
        }
    }

    public abstract class Week4TowerBase : MonoBehaviour
    {
        protected Week4GameManager gameManager;
        protected float range;
        protected float facingAngle;
        private LineRenderer rangeLine;

        protected GameVector2 Position => GameVector2.FromUnity(transform.position);
        protected GameVector2 Forward => ManualMath.DirectionFromDegrees(facingAngle);

        protected void ConfigureBase(
            Week4GameManager manager,
            float detectionRange,
            float initialAngle,
            Color rangeColor)
        {
            gameManager = manager;
            range = detectionRange;
            facingAngle = initialAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, facingAngle);
            rangeLine = Week3RuntimeFactory.CreateLine(
                gameObject,
                "Tower Range",
                rangeColor,
                0.035f,
                1);
            DrawRangeCircle();
        }

        protected BezierCreature FindTarget()
        {
            return gameManager.FindNearestCreature(Position, range);
        }

        protected void RotateToward(GameVector2 target, float turnSpeed)
        {
            GameVector2 direction = target - Position;
            if (ManualMath.LengthSquared(direction) <= ManualMath.Epsilon)
            {
                return;
            }

            float desiredAngle = ManualMath.AngleDegrees(direction);
            float difference = ManualMath.DeltaAngleDegrees(facingAngle, desiredAngle);
            float maximumStep = turnSpeed * Time.deltaTime;
            facingAngle += ManualMath.Clamp(difference, -maximumStep, maximumStep);
            transform.rotation = Quaternion.Euler(0f, 0f, facingAngle);
        }

        protected void Fire(
            string projectileName,
            GameVector2 direction,
            float speed,
            float projectileRadius,
            Color color)
        {
            GameVector2 unitDirection = ManualMath.NormalizeSafe(direction);
            GameVector2 spawnPosition = Position + unitDirection * 0.62f;
            GameObject projectileObject = Week3RuntimeFactory.CreateSquare(
                projectileName,
                spawnPosition,
                new GameVector2(projectileRadius * 2f, projectileRadius * 2f),
                color,
                -0.3f);
            projectileObject.GetComponent<SpriteRenderer>().sortingOrder = 6;
            Week4Projectile projectile = projectileObject.AddComponent<Week4Projectile>();
            projectile.Configure(gameManager, unitDirection, speed, projectileRadius, 2.25f);
        }

        private void DrawRangeCircle()
        {
            const int segments = 48;
            rangeLine.positionCount = segments + 1;
            for (int index = 0; index <= segments; index++)
            {
                float angle = 360f * index / segments;
                GameVector2 point = Position + ManualMath.DirectionFromDegrees(angle) * range;
                rangeLine.SetPosition(index, point.ToVector3(0.1f));
            }
        }
    }

    public sealed class Week4FlameTower : Week4TowerBase
    {
        private float fireTimer;

        public void Configure(Week4GameManager manager)
        {
            ConfigureBase(manager, 3.15f, 0f, new Color(1f, 0.3f, 0.06f, 0.34f));
        }

        private void Update()
        {
            if (!gameManager.IsRunning) return;
            BezierCreature target = FindTarget();
            if (target == null) return;

            RotateToward(target.Position, 150f);
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                fireTimer = 0.5f;
                Fire(
                    "Flame Bolt",
                    target.Position - Position,
                    7.5f,
                    0.15f,
                    new Color(1f, 0.34f, 0.04f));
            }
        }
    }

    public sealed class Week4SniperTower : Week4TowerBase
    {
        private float fireTimer;

        public void Configure(Week4GameManager manager)
        {
            ConfigureBase(manager, 5.0f, 180f, new Color(0.25f, 0.65f, 1f, 0.28f));
        }

        private void Update()
        {
            if (!gameManager.IsRunning) return;
            fireTimer -= Time.deltaTime;
            BezierCreature target = FindTarget();
            if (target == null) return;

            RotateToward(target.Position, 105f);
            GameVector2 toTarget = ManualMath.NormalizeSafe(target.Position - Position);
            float alignment = ManualMath.Dot(Forward, toTarget);
            if (fireTimer <= 0f && alignment >= 0.995f)
            {
                fireTimer = 1.25f;
                Fire(
                    "Sniper Bolt",
                    Forward,
                    16f,
                    0.1f,
                    new Color(0.35f, 0.82f, 1f));
            }
        }
    }

    public sealed class Week4ShotgunTower : Week4TowerBase
    {
        private float fireTimer;

        public void Configure(Week4GameManager manager)
        {
            ConfigureBase(manager, 3.7f, 180f, new Color(0.75f, 0.35f, 1f, 0.3f));
        }

        private void Update()
        {
            if (!gameManager.IsRunning) return;
            fireTimer -= Time.deltaTime;
            BezierCreature target = FindTarget();
            if (target == null) return;

            RotateToward(target.Position, 120f);
            if (fireTimer > 0f) return;

            fireTimer = 1.65f;
            const int pelletCount = 5;
            const float spreadDegrees = 28f;
            for (int pellet = 0; pellet < pelletCount; pellet++)
            {
                float t = pellet / (float)(pelletCount - 1);
                float offset = ManualMath.Lerp(-spreadDegrees * 0.5f, spreadDegrees * 0.5f, t);
                Fire(
                    "Shotgun Pellet",
                    ManualMath.DirectionFromDegrees(facingAngle + offset),
                    9.5f,
                    0.12f,
                    new Color(0.9f, 0.6f, 1f));
            }
        }
    }
}

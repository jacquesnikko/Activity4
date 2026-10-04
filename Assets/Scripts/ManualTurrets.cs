using System;
using UnityEngine;

namespace AppliedMath.Week3
{
    public abstract class ManualTurretBase : MonoBehaviour
    {
        protected ManualGameManager gameManager;
        protected LineRenderer rangeLine;
        protected float facingAngleDegrees;
        protected float range;
        protected bool firingEnabled = true;

        protected GameVector2 Position => GameVector2.FromUnity(transform.position);
        protected GameVector2 Forward => ManualMath.DirectionFromDegrees(facingAngleDegrees);
        protected bool CanFire =>
            firingEnabled && gameManager != null && !gameManager.HasWon && gameManager.Player != null;

        protected void ConfigureBase(
            ManualGameManager manager,
            float initialFacingAngle,
            float detectionRange,
            Color rangeColor)
        {
            gameManager = manager;
            facingAngleDegrees = initialFacingAngle;
            range = detectionRange;
            transform.rotation = Quaternion.Euler(0f, 0f, facingAngleDegrees);
            rangeLine = Week3RuntimeFactory.CreateLine(
                gameObject,
                "Detection Range",
                rangeColor,
                0.045f,
                1);
            gameManager.RegisterTurret(this);
        }

        public void SetFiringEnabled(bool enabled)
        {
            firingEnabled = enabled;
        }

        protected void RotateTowardPlayer(float degreesPerSecond)
        {
            GameVector2 toPlayer = gameManager.Player.Position - Position;
            if (ManualMath.LengthSquared(toPlayer) <= ManualMath.Epsilon)
            {
                return;
            }

            float targetAngle = ManualMath.AngleDegrees(toPlayer);
            float difference = ManualMath.DeltaAngleDegrees(facingAngleDegrees, targetAngle);
            float maximumStep = degreesPerSecond * Time.deltaTime;
            facingAngleDegrees += ManualMath.Clamp(difference, -maximumStep, maximumStep);
            transform.rotation = Quaternion.Euler(0f, 0f, facingAngleDegrees);
        }

        protected void DrawCone(float fullConeAngle, int arcSegments = 24)
        {
            int pointCount = arcSegments + 3;
            rangeLine.positionCount = pointCount;
            rangeLine.SetPosition(0, Position.ToVector3(-0.05f));

            float startingAngle = facingAngleDegrees - fullConeAngle * 0.5f;
            for (int index = 0; index <= arcSegments; index++)
            {
                float interpolation = index / (float)arcSegments;
                float angle = startingAngle + fullConeAngle * interpolation;
                GameVector2 point = Position + ManualMath.DirectionFromDegrees(angle) * range;
                rangeLine.SetPosition(index + 1, point.ToVector3(-0.05f));
            }

            rangeLine.SetPosition(pointCount - 1, Position.ToVector3(-0.05f));
        }

        protected void DrawSightLine(float halfWidth)
        {
            GameVector2 forward = Forward;
            GameVector2 side = new GameVector2(-forward.y, forward.x) * halfWidth;
            GameVector2 end = Position + forward * range;
            rangeLine.positionCount = 5;
            rangeLine.SetPosition(0, (Position + side).ToVector3(-0.05f));
            rangeLine.SetPosition(1, (end + side).ToVector3(-0.05f));
            rangeLine.SetPosition(2, (end - side).ToVector3(-0.05f));
            rangeLine.SetPosition(3, (Position - side).ToVector3(-0.05f));
            rangeLine.SetPosition(4, (Position + side).ToVector3(-0.05f));
        }

        protected void SpawnProjectile(
            string projectileName,
            GameVector2 direction,
            float speed,
            float radius,
            float lifetime,
            Color color)
        {
            GameVector2 unitDirection = ManualMath.NormalizeSafe(direction);
            GameVector2 spawnPosition = Position + unitDirection * 0.72f;
            Week3RuntimeFactory.CreateProjectile(
                gameManager,
                projectileName,
                spawnPosition,
                unitDirection,
                speed,
                radius,
                lifetime,
                color);
        }
    }

    public sealed class FlameTurret : ManualTurretBase
    {
        private readonly System.Random random = new System.Random(31415);
        private float coneAngle;
        private float fireInterval;
        private float fireTimer;
        private float turnSpeed;

        public void Configure(
            ManualGameManager manager,
            float facingAngle,
            float detectionRange,
            float fullConeAngle,
            float shotsPerSecond,
            float degreesPerSecond)
        {
            ConfigureBase(manager, facingAngle, detectionRange, new Color(1f, 0.3f, 0.08f, 0.85f));
            coneAngle = fullConeAngle;
            fireInterval = 1f / shotsPerSecond;
            turnSpeed = degreesPerSecond;
            DrawCone(coneAngle);
        }

        private void Update()
        {
            if (!CanFire) return;

            RotateTowardPlayer(turnSpeed);
            DrawCone(coneAngle);
            fireTimer -= Time.deltaTime;

            if (!ManualMath.IsInsideCone(
                Position,
                Forward,
                gameManager.Player.Position,
                range,
                coneAngle) || fireTimer > 0f)
            {
                return;
            }

            fireTimer = fireInterval;
            float jitter = (float)(random.NextDouble() * 12.0 - 6.0);
            GameVector2 flameDirection = ManualMath.DirectionFromDegrees(facingAngleDegrees + jitter);
            SpawnProjectile(
                "Flame Particle",
                flameDirection,
                5.2f,
                0.17f,
                0.95f,
                new Color(1f, 0.35f, 0.05f));
        }
    }

    public sealed class SniperTurret : ManualTurretBase
    {
        private float sightHalfWidth;
        private bool playerWasInSight;

        public void Configure(
            ManualGameManager manager,
            float facingAngle,
            float detectionRange,
            float lineHalfWidth)
        {
            ConfigureBase(manager, facingAngle, detectionRange, new Color(0.2f, 0.65f, 1f, 0.9f));
            sightHalfWidth = lineHalfWidth;
            DrawSightLine(sightHalfWidth);
        }

        private void Update()
        {
            if (!CanFire) return;

            bool playerInSight = ManualMath.IsInsideSightLine(
                Position,
                Forward,
                gameManager.Player.Position,
                range,
                sightHalfWidth);

            if (playerInSight && !playerWasInSight)
            {
                SpawnProjectile(
                    "Sniper Round",
                    Forward,
                    18f,
                    0.12f,
                    1.1f,
                    new Color(0.35f, 0.8f, 1f));
            }

            playerWasInSight = playerInSight;
        }
    }

    public sealed class ShotgunTurret : ManualTurretBase
    {
        private float coneAngle;
        private float cooldown;
        private float cooldownTimer;
        private float turnSpeed;
        private int pelletCount;

        public void Configure(
            ManualGameManager manager,
            float facingAngle,
            float detectionRange,
            float fullConeAngle,
            float secondsBetweenShots,
            float degreesPerSecond,
            int pellets)
        {
            ConfigureBase(manager, facingAngle, detectionRange, new Color(0.75f, 0.35f, 1f, 0.85f));
            coneAngle = fullConeAngle;
            cooldown = secondsBetweenShots;
            turnSpeed = degreesPerSecond;
            pelletCount = pellets;
            DrawCone(coneAngle);
        }

        private void Update()
        {
            if (!CanFire) return;

            RotateTowardPlayer(turnSpeed);
            DrawCone(coneAngle);
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer > 0f || !ManualMath.IsInsideCone(
                Position,
                Forward,
                gameManager.Player.Position,
                range,
                coneAngle))
            {
                return;
            }

            cooldownTimer = cooldown;
            float spread = 30f;
            for (int pellet = 0; pellet < pelletCount; pellet++)
            {
                float t = pelletCount <= 1 ? 0.5f : pellet / (float)(pelletCount - 1);
                float offset = ManualMath.Lerp(-spread * 0.5f, spread * 0.5f, t);
                GameVector2 direction = ManualMath.DirectionFromDegrees(facingAngleDegrees + offset);
                SpawnProjectile(
                    "Shotgun Pellet",
                    direction,
                    9.5f,
                    0.13f,
                    1.05f,
                    new Color(0.9f, 0.55f, 1f));
            }
        }
    }
}

using UnityEngine;

namespace AppliedMath.Week3
{
    public sealed class ManualProjectile : MonoBehaviour
    {
        private ManualGameManager gameManager;
        private GameVector2 direction;
        private float speed;
        private float collisionRadius;
        private float remainingLifetime;

        public void Configure(
            ManualGameManager manager,
            GameVector2 travelDirection,
            float travelSpeed,
            float radius,
            float lifetime)
        {
            gameManager = manager;
            direction = ManualMath.NormalizeSafe(travelDirection);
            speed = travelSpeed;
            collisionRadius = radius;
            remainingLifetime = lifetime;
        }

        private void Update()
        {
            if (gameManager == null || gameManager.HasWon)
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

            if (gameManager.ProjectilePathBlocked(start, end, collisionRadius))
            {
                Destroy(gameObject);
                return;
            }

            ManualPlayerController player = gameManager.Player;
            if (player != null && ManualMath.SegmentIntersectsCircle(
                start,
                end,
                player.Position,
                player.CollisionRadius + collisionRadius))
            {
                gameManager.PlayerWasHit();
                Destroy(gameObject);
                return;
            }

            transform.position = end.ToVector3(transform.position.z);
        }
    }
}

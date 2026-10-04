using UnityEngine;

namespace AppliedMath.Week3
{
    public sealed class ManualPlayerController : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = 5.25f;
        [SerializeField] private float collisionRadius = 0.34f;
        [SerializeField] private float horizontalLimit = 10.2f;
        [SerializeField] private float verticalLimit = 5.15f;

        private ManualGameManager gameManager;

        public float CollisionRadius => collisionRadius;
        public GameVector2 Position => GameVector2.FromUnity(transform.position);

        public void Configure(ManualGameManager manager)
        {
            gameManager = manager;
        }

        private void Update()
        {
            if (gameManager == null || gameManager.HasWon)
            {
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;

            GameVector2 input = ManualMath.NormalizeSafe(new GameVector2(horizontal, vertical));
            GameVector2 movement = input * (movementSpeed * Time.deltaTime);
            GameVector2 current = Position;

            GameVector2 horizontalCandidate = new GameVector2(
                ManualMath.Clamp(current.x + movement.x, -horizontalLimit, horizontalLimit),
                current.y);

            if (!gameManager.PositionBlocked(horizontalCandidate, collisionRadius))
            {
                current = horizontalCandidate;
            }

            GameVector2 verticalCandidate = new GameVector2(
                current.x,
                ManualMath.Clamp(current.y + movement.y, -verticalLimit, verticalLimit));

            if (!gameManager.PositionBlocked(verticalCandidate, collisionRadius))
            {
                current = verticalCandidate;
            }

            transform.position = current.ToVector3(transform.position.z);
        }
    }
}

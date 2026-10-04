using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AppliedMath.Week3
{
    public sealed class ManualGameManager : MonoBehaviour
    {
        public static ManualGameManager Current { get; private set; }

        private readonly List<ManualTurretBase> turrets = new List<ManualTurretBase>();
        private ManualPlayerController player;
        private GameAabb[] obstacles = new GameAabb[0];
        private GameVector2 goalPosition;
        private float goalRadius;
        private GameObject winPanel;
        private bool restarting;

        public bool HasWon { get; private set; }
        public ManualPlayerController Player => player;
        public IReadOnlyList<GameAabb> Obstacles => obstacles;

        private void Awake()
        {
            Current = this;
        }

        public void Configure(
            ManualPlayerController playerController,
            GameVector2 goal,
            float radius,
            GameAabb[] levelObstacles,
            GameObject winUi)
        {
            player = playerController;
            goalPosition = goal;
            goalRadius = radius;
            obstacles = levelObstacles ?? new GameAabb[0];
            winPanel = winUi;
            HasWon = false;
            restarting = false;

            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }
        }

        public void RegisterTurret(ManualTurretBase turret)
        {
            if (turret != null && !turrets.Contains(turret))
            {
                turrets.Add(turret);
            }
        }

        private void Update()
        {
            if (HasWon || restarting || player == null)
            {
                return;
            }

            float combinedRadius = goalRadius + player.CollisionRadius;
            if (ManualMath.DistanceSquared(player.Position, goalPosition) <=
                combinedRadius * combinedRadius)
            {
                Win();
            }
        }

        public bool PositionBlocked(GameVector2 center, float radius)
        {
            for (int index = 0; index < obstacles.Length; index++)
            {
                if (ManualMath.CircleOverlapsAabb(center, radius, obstacles[index]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool ProjectilePathBlocked(
            GameVector2 start,
            GameVector2 end,
            float projectileRadius)
        {
            for (int index = 0; index < obstacles.Length; index++)
            {
                if (ManualMath.SegmentIntersectsAabb(
                    start,
                    end,
                    obstacles[index],
                    projectileRadius))
                {
                    return true;
                }
            }

            return false;
        }

        public void PlayerWasHit()
        {
            if (HasWon || restarting)
            {
                return;
            }

            restarting = true;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }

        private void Win()
        {
            HasWon = true;
            for (int index = 0; index < turrets.Count; index++)
            {
                if (turrets[index] != null)
                {
                    turrets[index].SetFiringEnabled(false);
                }
            }

            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
        }
    }
}

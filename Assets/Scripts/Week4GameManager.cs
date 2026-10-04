using System.Collections.Generic;
using AppliedMath.Week3;
using UnityEngine;
using UnityEngine.UI;

namespace AppliedMath.Week4
{
    public enum Week4GameState
    {
        Running,
        WaveCleared,
        Defeated
    }

    public sealed class Week4GameManager : MonoBehaviour
    {
        private readonly List<BezierCreature> activeCreatures = new List<BezierCreature>();
        private GhostHealthBar healthBar;
        private CoinBankUi coinBank;
        private GameObject statePanel;
        private Text stateText;
        private int maximumHealth;
        private int currentHealth;
        private int expectedCreatures;
        private int resolvedCreatures;

        public Week4GameState State { get; private set; }
        public bool IsRunning => State == Week4GameState.Running;
        public IReadOnlyList<BezierCreature> ActiveCreatures => activeCreatures;

        public void Configure(
            int startingHealth,
            int totalCreatures,
            GhostHealthBar hpBar,
            CoinBankUi bank,
            GameObject resultPanel,
            Text resultText)
        {
            maximumHealth = startingHealth;
            currentHealth = startingHealth;
            expectedCreatures = totalCreatures;
            healthBar = hpBar;
            coinBank = bank;
            statePanel = resultPanel;
            stateText = resultText;
            State = Week4GameState.Running;
            resolvedCreatures = 0;

            healthBar.SetHealth(currentHealth, maximumHealth, true);
            statePanel.SetActive(false);
        }

        public void RegisterCreature(BezierCreature creature)
        {
            if (creature != null && !activeCreatures.Contains(creature))
            {
                activeCreatures.Add(creature);
            }
        }

        public void CreatureKilled(BezierCreature creature, GameVector2 deathPosition)
        {
            if (!activeCreatures.Remove(creature))
            {
                return;
            }

            if (IsRunning)
            {
                coinBank.SpawnCoin(deathPosition, 10);
            }

            ResolveCreature();
        }

        public void CreatureReachedTarget(BezierCreature creature)
        {
            if (!activeCreatures.Remove(creature))
            {
                return;
            }

            if (IsRunning)
            {
                currentHealth--;
                if (currentHealth < 0) currentHealth = 0;
                healthBar.SetHealth(currentHealth, maximumHealth, false);
            }

            ResolveCreature();

            if (currentHealth <= 0 && State == Week4GameState.Running)
            {
                SetResult(Week4GameState.Defeated, "BASE DESTROYED\nHP reached zero");
            }
        }

        public BezierCreature FindNearestCreature(GameVector2 position, float maximumRange)
        {
            BezierCreature nearest = null;
            float nearestDistanceSquared = maximumRange * maximumRange;

            for (int index = 0; index < activeCreatures.Count; index++)
            {
                BezierCreature candidate = activeCreatures[index];
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                float distanceSquared = ManualMath.DistanceSquared(position, candidate.Position);
                if (distanceSquared <= nearestDistanceSquared)
                {
                    nearestDistanceSquared = distanceSquared;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private void ResolveCreature()
        {
            resolvedCreatures++;
            if (resolvedCreatures >= expectedCreatures && currentHealth > 0 && IsRunning)
            {
                SetResult(
                    Week4GameState.WaveCleared,
                    "WAVE CLEARED\nBase HP: " + currentHealth + " / " + maximumHealth);
            }
        }

        private void SetResult(Week4GameState newState, string message)
        {
            State = newState;
            stateText.text = message;
            statePanel.SetActive(true);
        }
    }
}

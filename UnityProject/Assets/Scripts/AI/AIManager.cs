using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Data;

namespace SoccerGame.AI
{
    public class AIManager : MonoBehaviour
    {
        [SerializeField] private TeamStrategy homeStrategy = new TeamStrategy();
        [SerializeField] private TeamStrategy awayStrategy = new TeamStrategy();
        [SerializeField] private float matchDuration = 90f;

        public TeamStrategy HomeStrategy => homeStrategy;
        public TeamStrategy AwayStrategy => awayStrategy;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            homeStrategy = new TeamStrategy();
            awayStrategy = new TeamStrategy();
        }

        public TeamStrategy GetStrategy(TeamSide side)
        {
            return side == TeamSide.Home ? homeStrategy : awayStrategy;
        }

        public void UpdateStrategies(int scoreDiff, float timeRemaining)
        {
            // scoreDiff is from the home team's perspective.
            AdjustStrategy(homeStrategy, scoreDiff, timeRemaining);
            AdjustStrategy(awayStrategy, -scoreDiff, timeRemaining);
        }

        private void AdjustStrategy(TeamStrategy strategy, int scoreDiff, float timeRemaining)
        {
            float urgency = Mathf.Clamp01(1f - timeRemaining / matchDuration);

            if (scoreDiff < 0)
            {
                // Trailing: push the line up, press harder and raise the tempo.
                strategy.Mentality = MentalityType.Attacking;
                strategy.DefensiveLine = Mathf.Clamp01(0.55f + 0.35f * urgency);
                strategy.PressingIntensity = Mathf.Clamp01(0.6f + 0.4f * urgency);
                strategy.Tempo = Mathf.Clamp01(0.6f + 0.4f * urgency);
                strategy.WidthFactor = Mathf.Clamp(1f + 0.25f * urgency, 0.7f, 1.3f);
                strategy.OffsideTrap = urgency > 0.6f;
            }
            else if (scoreDiff > 0)
            {
                // Leading: drop deeper, sit off and slow the game down late on.
                strategy.Mentality = MentalityType.Defensive;
                strategy.DefensiveLine = Mathf.Clamp01(0.45f - 0.3f * urgency);
                strategy.PressingIntensity = Mathf.Clamp01(0.5f - 0.25f * urgency);
                strategy.Tempo = Mathf.Clamp01(0.5f - 0.35f * urgency);
                strategy.WidthFactor = Mathf.Clamp(1f - 0.2f * urgency, 0.7f, 1.3f);
                strategy.OffsideTrap = false;
            }
            else
            {
                // Level: stay balanced with a slight lift in urgency late in the game.
                strategy.SetBalanced();
                strategy.Tempo = Mathf.Clamp01(0.5f + 0.2f * urgency);
                strategy.PressingIntensity = Mathf.Clamp01(0.5f + 0.2f * urgency);
            }

            strategy.ClampValues();
        }
    }
}

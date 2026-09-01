using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Data;

namespace SoccerGame.AI
{
    [System.Serializable]
    public class TeamStrategy
    {
        public MentalityType Mentality;
        [Range(0f, 1f)] public float DefensiveLine;
        [Range(0f, 1f)] public float PressingIntensity;
        [Range(0.7f, 1.3f)] public float WidthFactor;
        [Range(0f, 1f)] public float Tempo;
        public bool OffsideTrap;

        public TeamStrategy()
        {
            SetBalanced();
        }

        public void SetBalanced()
        {
            Mentality = MentalityType.Balanced;
            DefensiveLine = 0.5f;
            PressingIntensity = 0.5f;
            WidthFactor = 1f;
            Tempo = 0.5f;
            OffsideTrap = false;
        }

        public void ClampValues()
        {
            DefensiveLine = Mathf.Clamp01(DefensiveLine);
            PressingIntensity = Mathf.Clamp01(PressingIntensity);
            WidthFactor = Mathf.Clamp(WidthFactor, 0.7f, 1.3f);
            Tempo = Mathf.Clamp01(Tempo);
        }
    }
}

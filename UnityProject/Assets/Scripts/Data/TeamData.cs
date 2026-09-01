using UnityEngine;

namespace SoccerGame.Data
{
    [CreateAssetMenu(fileName = "Team", menuName = "Soccer/Team")]
    public class TeamData : ScriptableObject
    {
        public string TeamName;
        public string ShortName;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public PlayerData[] StartingXI;
        public PlayerData[] Substitutes;
        public int OverallRating;
    }
}

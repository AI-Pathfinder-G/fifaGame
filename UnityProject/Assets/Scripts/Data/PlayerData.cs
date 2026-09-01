using UnityEngine;

namespace SoccerGame.Data
{
    [CreateAssetMenu(fileName = "Player", menuName = "Soccer/Player")]
    public class PlayerData : ScriptableObject
    {
        public string PlayerName;
        public int FieldNumber;
        public SoccerGame.Core.PositionRole PreferredRole;
        public bool IsGoalkeeper;
        public PlayerStats Stats;
    }
}

using System;

namespace SoccerGame.Data
{
    [Serializable]
    public struct PlayerStats
    {
        public int Pace;
        public int Acceleration;
        public int Stamina;
        public int Strength;
        public int Jumping;
        public int Shooting;
        public int Finishing;
        public int Passing;
        public int Crossing;
        public int Dribbling;
        public int BallControl;
        public int Technique;
        public int Defending;
        public int Tackling;
        public int Heading;
        public int Interception;
        public int Vision;
        public int Composure;
        public int Aggression;
        public int Positioning;
        public int GKReflexes;
        public int GKDiving;
        public int GKHandling;
        public int GKKicking;
        public int GKPositioning;

        public int Overall()
        {
            return (Pace + Shooting + Passing + Dribbling + Defending + Stamina) / 6;
        }
    }
}

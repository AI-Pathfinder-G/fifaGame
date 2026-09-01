using System;
using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Ball;
using SoccerGame.Data;

namespace SoccerGame.Match
{
    [Serializable]
    public class FoulData
    {
        public PlayerEntity Fouler;
        public PlayerEntity Victim;
        public Vector3 Position;
        public string Severity; // "Minor", "Moderate", "Severe" or "Penalty"

        public FoulData() { }

        public FoulData(PlayerEntity fouler, PlayerEntity victim, Vector3 position, string severity)
        {
            Fouler = fouler;
            Victim = victim;
            Position = position;
            Severity = severity;
        }
    }
}

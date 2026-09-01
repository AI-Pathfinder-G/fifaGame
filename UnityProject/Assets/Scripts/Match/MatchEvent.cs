using System;
using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Ball;
using SoccerGame.Data;

namespace SoccerGame.Match
{
    [Serializable]
    public class MatchEvent
    {
        public float Time;
        public string EventType;
        public string Team;
        public string Description;

        public MatchEvent() { }

        public MatchEvent(float time, string eventType, string team, string description)
        {
            Time = time;
            EventType = eventType;
            Team = team;
            Description = description;
        }

        public override string ToString()
        {
            int minutes = Mathf.FloorToInt(Time / 60f);
            return $"[{minutes}' {Team}] {EventType}: {Description}";
        }
    }
}

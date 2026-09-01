using System;
using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Ball;
using SoccerGame.Data;

namespace SoccerGame.Match
{
    [Serializable]
    public class CardData
    {
        public PlayerEntity Player;
        public string CardType; // "Yellow" or "Red"
        public float TimeIssued;
        public string Reason;

        public CardData() { }

        public CardData(PlayerEntity player, string cardType, float timeIssued, string reason)
        {
            Player = player;
            CardType = cardType;
            TimeIssued = timeIssued;
            Reason = reason;
        }
    }
}

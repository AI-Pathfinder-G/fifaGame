using System;
using UnityEngine;

namespace SoccerGame.Data
{
    [Serializable]
    public struct FormationSlot
    {
        public SoccerGame.Core.PositionRole Role;
        public Vector2 BasePosition;
        public float AttackBias;
        public float DefenseBias;
    }
}

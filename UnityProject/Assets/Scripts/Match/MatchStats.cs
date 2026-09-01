using System;
using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Ball;
using SoccerGame.Data;

namespace SoccerGame.Match
{
    [Serializable]
    public class MatchStats
    {
        public int ShotsHome;
        public int ShotsAway;
        public int ShotsOnTargetHome;
        public int ShotsOnTargetAway;
        public int PassesHome;
        public int PassesAway;
        public int CornersHome;
        public int CornersAway;
        public int FoulsHome;
        public int FoulsAway;
        public int YellowHome;
        public int YellowAway;
        public int RedHome;
        public int RedAway;
        public float PossessionHome;
        public float PossessionAway;

        public void Reset()
        {
            ShotsHome = 0;
            ShotsAway = 0;
            ShotsOnTargetHome = 0;
            ShotsOnTargetAway = 0;
            PassesHome = 0;
            PassesAway = 0;
            CornersHome = 0;
            CornersAway = 0;
            FoulsHome = 0;
            FoulsAway = 0;
            YellowHome = 0;
            YellowAway = 0;
            RedHome = 0;
            RedAway = 0;
            PossessionHome = 0f;
            PossessionAway = 0f;
        }

        public void RegisterShot(TeamSide team, bool onTarget)
        {
            if (team == TeamSide.Home)
            {
                ShotsHome++;
                if (onTarget) ShotsOnTargetHome++;
            }
            else
            {
                ShotsAway++;
                if (onTarget) ShotsOnTargetAway++;
            }
        }

        public void RegisterPass(TeamSide team)
        {
            if (team == TeamSide.Home) PassesHome++;
            else PassesAway++;
        }

        public void RegisterCorner(TeamSide team)
        {
            if (team == TeamSide.Home) CornersHome++;
            else CornersAway++;
        }

        public void RegisterFoul(TeamSide team)
        {
            if (team == TeamSide.Home) FoulsHome++;
            else FoulsAway++;
        }

        public void RegisterCard(TeamSide team, string cardType)
        {
            if (cardType == "Red")
            {
                if (team == TeamSide.Home) RedHome++;
                else RedAway++;
            }
            else
            {
                if (team == TeamSide.Home) YellowHome++;
                else YellowAway++;
            }
        }

        /// <summary>
        /// Converts raw possession seconds into percentages that sum to 100.
        /// </summary>
        public void NormalizePossession()
        {
            float total = PossessionHome + PossessionAway;
            if (total <= 0f)
            {
                PossessionHome = 50f;
                PossessionAway = 50f;
                return;
            }

            PossessionHome = PossessionHome / total * 100f;
            PossessionAway = 100f - PossessionHome;
        }
    }
}

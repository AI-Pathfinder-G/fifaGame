using System.Collections.Generic;
using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Ball;
using SoccerGame.Data;

namespace SoccerGame.Match
{
    /// <summary>
    /// Evaluates tackles, detects fouls, issues cards and checks offside.
    /// </summary>
    public class RefereeSystem : MonoBehaviour
    {
        [SerializeField] private MatchManager match;
        [SerializeField] private BallEntity ball;

        [Header("Tackle Detection")]
        [SerializeField] private float behindDotThreshold = -0.35f;
        [SerializeField] private float lateBallDistance = 2.5f;

        private readonly List<FoulData> fouls = new List<FoulData>();
        private readonly List<CardData> cards = new List<CardData>();

        public IReadOnlyList<FoulData> Fouls => fouls;
        public IReadOnlyList<CardData> Cards => cards;

        /// <summary>
        /// Returns true when the tackle is fair, false when a foul is called.
        /// </summary>
        public bool EvaluateTackle(PlayerEntity tackler, PlayerEntity ballCarrier)
        {
            if (tackler == null || ballCarrier == null)
                return true;

            if (!DetectFoul(tackler, ballCarrier))
                return true;

            FoulData foul = new FoulData
            {
                Fouler = tackler,
                Victim = ballCarrier,
                Position = ballCarrier.transform.position,
                Severity = ClassifySeverity(tackler, ballCarrier)
            };
            fouls.Add(foul);
            GameEvents.RaiseFoulCommitted($"{tackler.Data?.PlayerName} fouled {ballCarrier.Data?.PlayerName}");

            TeamSide victimTeam = ballCarrier.Team;

            if (foul.Severity == "Penalty")
                match.AwardSetPiece(SetPieceType.PenaltyKick, victimTeam, foul.Position);
            else
                AwardFreeKick(foul.Position, victimTeam);

            if (foul.Severity == "Severe")
                IssueCard(tackler, "Red", "Dangerous tackle");
            else if (foul.Severity == "Moderate")
                IssueCard(tackler, "Yellow", "Reckless tackle");

            return false;
        }

        /// <summary>
        /// A tackle is a foul when it comes from behind or arrives late.
        /// </summary>
        public bool DetectFoul(PlayerEntity tackler, PlayerEntity ballCarrier)
        {
            return IsTackleFromBehind(tackler, ballCarrier) || IsTackleLate(ballCarrier);
        }

        private bool IsTackleFromBehind(PlayerEntity tackler, PlayerEntity ballCarrier)
        {
            if (tackler == null || ballCarrier == null)
                return false;

            Vector3 toTackler = tackler.transform.position - ballCarrier.transform.position;
            toTackler.y = 0f;
            if (toTackler.sqrMagnitude < 0.0001f)
                return false;
            toTackler.Normalize();

            Vector3 carrierForward = ballCarrier.transform.forward;
            carrierForward.y = 0f;
            if (carrierForward.sqrMagnitude < 0.0001f)
                return false;
            carrierForward.Normalize();

            // Tackler positioned behind the direction the carrier is facing.
            return Vector3.Dot(carrierForward, toTackler) < behindDotThreshold;
        }

        private bool IsTackleLate(PlayerEntity ballCarrier)
        {
            if (ball == null || ballCarrier == null)
                return false;

            // Ball has already been played away from the carrier.
            Vector3 toBall = ball.transform.position - ballCarrier.transform.position;
            toBall.y = 0f;
            return toBall.magnitude > lateBallDistance;
        }

        private string ClassifySeverity(PlayerEntity tackler, PlayerEntity ballCarrier)
        {
            bool fromBehind = IsTackleFromBehind(tackler, ballCarrier);
            bool late = IsTackleLate(ballCarrier);

            if (fromBehind && late)
                return "Severe";
            if (fromBehind || late)
                return "Moderate";
            return "Minor";
        }

        public void AwardFreeKick(Vector3 pos, TeamSide team)
        {
            if (match != null)
                match.AwardSetPiece(SetPieceType.FreeKick, team, pos);
        }

        public CardData IssueCard(PlayerEntity player, string cardType, string reason)
        {
            CardData card = new CardData
            {
                Player = player,
                CardType = cardType,
                TimeIssued = match != null ? match.MatchTime : 0f,
                Reason = reason
            };
            cards.Add(card);
            GameEvents.RaiseCardShown(player.Data?.PlayerName ?? "Unknown", cardType);
            return card;
        }

        /// <summary>
        /// Simplified offside: attacker is beyond the ball in the attacking direction.
        /// Home attacks toward +Z, Away attacks toward -Z.
        /// </summary>
        public bool CheckOffside(PlayerEntity attacker, Vector3 ballPos, TeamSide attacking)
        {
            if (attacker == null)
                return false;

            float attackerZ = attacker.transform.position.z;

            return attacking == TeamSide.Home
                ? attackerZ > ballPos.z
                : attackerZ < ballPos.z;
        }
    }
}

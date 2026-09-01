using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Ball;
using SoccerGame.Data;

namespace SoccerGame.Match
{
    /// <summary>
    /// Core match flow controller: score, clock, halves, possession and set piece awards.
    /// </summary>
    public class MatchManager : MonoBehaviour, IGameSystem
    {
        [Header("Match Settings")]
        [SerializeField] private float halfDuration = 2700f;
        [SerializeField] private TeamSide initialKickoffTeam = TeamSide.Home;

        [Header("References")]
        [SerializeField] private SetPieceController setPieceController;
        [SerializeField] private MatchStats stats = new MatchStats();

        private bool matchRunning;

        public int HomeScore { get; private set; }
        public int AwayScore { get; private set; }
        public float MatchTime { get; private set; }
        public float HalfDuration
        {
            get => halfDuration;
            set => halfDuration = Mathf.Max(1f, value);
        }
        public int CurrentHalf { get; private set; } = 1;
        public TeamSide KickoffTeam { get; private set; }
        public TeamSide Possession { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsMatchRunning => matchRunning;
        public MatchStats Stats => stats;

        private void OnEnable()
        {
            GameEvents.SubscribeGoalScored(HandleGoalScored);
        }

        private void OnDisable()
        {
            GameEvents.UnsubscribeGoalScored(HandleGoalScored);
        }

        private void HandleGoalScored(string teamStr)
        {
            if (System.Enum.TryParse<TeamSide>(teamStr, out TeamSide scoringTeam))
                ScoreGoal(scoringTeam);
        }

        private void Awake()
        {
            if (!IsInitialized)
                Initialize();
        }

        public void Initialize()
        {
            HomeScore = 0;
            AwayScore = 0;
            MatchTime = 0f;
            CurrentHalf = 1;
            KickoffTeam = initialKickoffTeam;
            Possession = initialKickoffTeam;
            matchRunning = false;

            if (stats == null)
                stats = new MatchStats();
            stats.Reset();

            IsInitialized = true;
        }

        public void Shutdown()
        {
            matchRunning = false;
            IsInitialized = false;
        }

        private void Update()
        {
            if (!IsInitialized || !matchRunning)
                return;

            float delta = Time.deltaTime * 2f;
            MatchTime += delta;

            if (Possession == TeamSide.Home)
                stats.PossessionHome += delta;
            else
                stats.PossessionAway += delta;

            if (MatchTime < halfDuration)
                return;

            if (CurrentHalf == 1)
                EndHalf();
            else
                EndMatch();
        }

        public void StartMatch()
        {
            if (!IsInitialized)
                Initialize();

            HomeScore = 0;
            AwayScore = 0;
            MatchTime = 0f;
            CurrentHalf = 1;
            KickoffTeam = initialKickoffTeam;
            Possession = initialKickoffTeam;
            matchRunning = true;

            GameEvents.RaiseKickoff();
            setPieceController?.SetupKickoff(KickoffTeam);
        }

        public void ScoreGoal(TeamSide scoringTeam)
        {
            if (!matchRunning)
                return;

            if (scoringTeam == TeamSide.Home)
                HomeScore++;
            else
                AwayScore++;

            GameEvents.RaiseScoreChanged(HomeScore, AwayScore);
            GameEvents.RaiseGoalScored(scoringTeam.ToString());

            // Conceding team restarts play from the center.
            KickoffTeam = scoringTeam == TeamSide.Home ? TeamSide.Away : TeamSide.Home;
            Possession = KickoffTeam;
            setPieceController?.SetupKickoff(KickoffTeam);
        }

        public void EndHalf()
        {
            if (CurrentHalf != 1)
            {
                EndMatch();
                return;
            }

            matchRunning = false;
            CurrentHalf = 2;
            MatchTime = 0f;

            GameEvents.RaiseHalftime();

            // Teams swap kickoff for the second half.
            KickoffTeam = initialKickoffTeam == TeamSide.Home ? TeamSide.Away : TeamSide.Home;
            Possession = KickoffTeam;
        }

        public void BeginSecondHalf()
        {
            if (CurrentHalf != 2 || matchRunning)
                return;

            MatchTime = 0f;
            matchRunning = true;
            setPieceController?.SetupKickoff(KickoffTeam);
        }

        public void EndMatch()
        {
            matchRunning = false;
            stats.NormalizePossession();
            GameEvents.RaiseFulltime();
        }

        public void AwardSetPiece(SetPieceType type, TeamSide team, Vector3 position)
        {
            Possession = team;
            GameEvents.RaiseSetPiece(type.ToString());

            if (setPieceController == null)
                return;

            switch (type)
            {
                case SetPieceType.Kickoff:
                    setPieceController.SetupKickoff(team);
                    break;
                case SetPieceType.FreeKick:
                    setPieceController.SetupFreeKick(position, team);
                    break;
                case SetPieceType.CornerKick:
                    setPieceController.SetupCorner(position, team);
                    break;
                case SetPieceType.ThrowIn:
                    setPieceController.SetupThrowIn(position, team);
                    break;
                case SetPieceType.GoalKick:
                    setPieceController.SetupGoalKick(position, team);
                    break;
                case SetPieceType.PenaltyKick:
                    setPieceController.SetupPenalty(team);
                    break;
            }
        }

        public void SetPossession(TeamSide team)
        {
            Possession = team;
        }
    }
}

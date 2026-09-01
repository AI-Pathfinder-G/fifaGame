using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Ball;
using SoccerGame.Data;

namespace SoccerGame.Match
{
    /// <summary>
    /// Places the ball and notifies listeners for every restart of play.
    /// Home attacks toward +Z, Away attacks toward -Z.
    /// </summary>
    public class SetPieceController : MonoBehaviour
    {
        [SerializeField] private BallEntity ball;

        [Header("Pitch Markings")]
        [SerializeField] private Vector3 centerSpot = Vector3.zero;
        [SerializeField] private float penaltySpotZ = 44f;
        [SerializeField] private float goalKickZ = 50f;
        [SerializeField] private float goalKickMaxX = 6f;
        [SerializeField] private float cornerX = 30f;
        [SerializeField] private float cornerZ = 55f;

        public void SetupKickoff(TeamSide team)
        {
            PlaceBall(centerSpot);
            GameEvents.RaiseSetPiece(SetPieceType.Kickoff.ToString());
        }

        public void SetupFreeKick(Vector3 pos, TeamSide team)
        {
            Vector3 spot = Flatten(pos);
            PlaceBall(spot);
            GameEvents.RaiseSetPiece(SetPieceType.FreeKick.ToString());
        }

        public void SetupCorner(Vector3 pos, TeamSide team)
        {
            // Snap to the nearest corner arc on the attacking side.
            float x = pos.x >= 0f ? cornerX : -cornerX;
            float z = team == TeamSide.Home ? cornerZ : -cornerZ;
            Vector3 spot = new Vector3(x, 0f, z);
            PlaceBall(spot);
            GameEvents.RaiseSetPiece(SetPieceType.CornerKick.ToString());
        }

        public void SetupThrowIn(Vector3 pos, TeamSide team)
        {
            Vector3 spot = Flatten(pos);
            PlaceBall(spot);
            GameEvents.RaiseSetPiece(SetPieceType.ThrowIn.ToString());
        }

        public void SetupGoalKick(Vector3 pos, TeamSide team)
        {
            // Defending team restarts from its own goal area.
            float z = team == TeamSide.Home ? -goalKickZ : goalKickZ;
            float x = Mathf.Clamp(pos.x, -goalKickMaxX, goalKickMaxX);
            Vector3 spot = new Vector3(x, 0f, z);
            PlaceBall(spot);
            GameEvents.RaiseSetPiece(SetPieceType.GoalKick.ToString());
        }

        public void SetupPenalty(TeamSide team)
        {
            // Kicking team shoots at the opponent's goal.
            float z = team == TeamSide.Home ? penaltySpotZ : -penaltySpotZ;
            Vector3 spot = new Vector3(0f, 0f, z);
            PlaceBall(spot);
            GameEvents.RaiseSetPiece(SetPieceType.PenaltyKick.ToString());
        }

        private static Vector3 Flatten(Vector3 pos)
        {
            pos.y = 0f;
            return pos;
        }

        private void PlaceBall(Vector3 pos)
        {
            if (ball == null)
                return;

            ball.transform.position = pos;

            Rigidbody body = ball.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }
    }
}

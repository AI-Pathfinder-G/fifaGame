using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;

namespace SoccerGame.Ball
{
    /// <summary>
    /// Routes ball collisions: goals, goal-post bounces and player
    /// deflections / possession changes.
    /// </summary>
    [RequireComponent(typeof(BallEntity))]
    public class BallCollisionHandler : MonoBehaviour
    {
        private const float PostRestitution = 0.7f;
        private const float DeflectionDamping = 0.55f;
        private const float DeflectionSpeedThreshold = 5f;

        [SerializeField] private BallEntity ball;

        private void Awake()
        {
            if (ball == null) ball = GetComponent<BallEntity>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("GoalTrigger")) return;

            TeamSide scoringTeam = ResolveScoringTeam(other);
            GameEvents.RaiseGoalScored(scoringTeam.ToString());
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.CompareTag("GoalPost"))
            {
                BounceOffPost(col);
                return;
            }

            if (col.gameObject.CompareTag("Player"))
            {
                HandlePlayerCollision(col);
            }
        }

        private void BounceOffPost(Collision col)
        {
            Vector3 normal = col.GetContact(0).normal;
            Vector3 velocity = ball.Rb.linearVelocity;

            Vector3 reflected = Vector3.Reflect(velocity, normal) * PostRestitution;
            ball.Rb.linearVelocity = reflected;

            // Glancing contact imparts sideways spin.
            ball.Spin = Vector3.Cross(normal, reflected) * 0.5f;
        }

        private void HandlePlayerCollision(Collision col)
        {
            PlayerEntity player = col.gameObject.GetComponentInParent<PlayerEntity>();
            if (player == null) return;

            // Ignore contact from the current owner (dribbling touches).
            if (ball.Owner == player) return;

            Vector3 normal = col.GetContact(0).normal;
            float closingSpeed = Vector3.Dot(col.relativeVelocity, -normal);

            if (closingSpeed >= DeflectionSpeedThreshold)
            {
                // Hard contact: deflect the ball away, knocking it loose.
                Vector3 deflected = Vector3.Reflect(ball.Rb.linearVelocity, normal) * DeflectionDamping;
                Vector3 spin = Vector3.Cross(normal, deflected) * 0.5f;

                bool hadOwner = ball.Owner != null;
                ball.Release(deflected, spin);

                if (hadOwner)
                    GameEvents.RaisePossessionChanged("None");
            }
            else
            {
                // Soft contact: the player brings the ball under control.
                ball.SetOwner(player);
                GameEvents.RaisePossessionChanged(player.Team.ToString());
            }
        }

        private static TeamSide ResolveScoringTeam(Collider trigger)
        {
            // GoalTrigger (SoccerGame.Core) knows which team defends this goal;
            // a goal here scores for the opposing team.
            GoalTrigger goal = trigger.GetComponentInParent<GoalTrigger>();
            if (goal == null)
            {
                Debug.LogWarning($"[BallCollisionHandler] '{trigger.name}' is tagged GoalTrigger but has no GoalTrigger component.");
                return TeamSide.Home;
            }

            return goal.DefendingTeam == TeamSide.Home ? TeamSide.Away : TeamSide.Home;
        }
    }
}

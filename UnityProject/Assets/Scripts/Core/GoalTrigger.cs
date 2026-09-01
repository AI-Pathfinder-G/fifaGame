using UnityEngine;
using SoccerGame.Core;

namespace SoccerGame.Core
{
    public class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private TeamSide defendingTeam = TeamSide.Home;

        public TeamSide DefendingTeam => defendingTeam;

        public void SetDefendingTeam(TeamSide team) { defendingTeam = team; }

        private void OnTriggerEnter(Collider other)
        {
            // Goal detection handled by BallCollisionHandler
        }
    }
}
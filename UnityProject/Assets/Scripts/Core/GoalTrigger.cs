using UnityEngine;
using SoccerGame.Core;

namespace SoccerGame.Core
{
    public class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private TeamSide defendingTeam = TeamSide.Home;

        public TeamSide DefendingTeam => defendingTeam;

        private void OnTriggerEnter(Collider other)
        {
            // Goal detection handled by BallCollisionHandler
        }
    }
}
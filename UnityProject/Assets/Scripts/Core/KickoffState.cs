using UnityEngine;

namespace SoccerGame.Core
{
    public class KickoffState : IGameState
    {
        private GameStateType _nextState;

        public KickoffState()
        {
            _nextState = GameStateType.Play;
        }

        public void Enter()
        {
            Debug.Log("Kickoff");
        }

        public void UpdateState()
        {
        }

        public void Exit()
        {
        }

        public GameStateType NextState()
        {
            return _nextState;
        }
    }
}

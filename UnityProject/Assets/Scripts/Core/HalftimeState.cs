using UnityEngine;

namespace SoccerGame.Core
{
    public class HalftimeState : IGameState
    {
        private GameStateType _nextState;

        public HalftimeState()
        {
            _nextState = GameStateType.Kickoff;
        }

        public void Enter()
        {
            Debug.Log("Halftime");
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

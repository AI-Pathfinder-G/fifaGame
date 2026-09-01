using UnityEngine;

namespace SoccerGame.Core
{
    public class FulltimeState : IGameState
    {
        private GameStateType _nextState;

        public FulltimeState()
        {
            _nextState = GameStateType.Menu;
        }

        public void Enter()
        {
            Debug.Log("Fulltime");
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

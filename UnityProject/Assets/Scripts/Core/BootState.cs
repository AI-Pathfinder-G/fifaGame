using UnityEngine;

namespace SoccerGame.Core
{
    public class BootState : IGameState
    {
        private GameStateType _nextState;

        public BootState()
        {
            _nextState = GameStateType.Menu;
        }

        public void Enter()
        {
            Debug.Log("Boot");
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

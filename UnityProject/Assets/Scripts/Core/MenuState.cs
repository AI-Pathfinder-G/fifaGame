using UnityEngine;

namespace SoccerGame.Core
{
    public class MenuState : IGameState
    {
        private GameStateType _nextState;

        public MenuState()
        {
            _nextState = GameStateType.Kickoff;
        }

        public void Enter()
        {
            Debug.Log("Menu");
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

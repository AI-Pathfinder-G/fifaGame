using UnityEngine;

namespace SoccerGame.Core
{
    public class SetPieceState : IGameState
    {
        private GameStateType _nextState;

        public SetPieceState()
        {
            _nextState = GameStateType.Play;
        }

        public void Enter()
        {
            Debug.Log("SetPiece");
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

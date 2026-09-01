using UnityEngine;

namespace SoccerGame.Core
{
    public class PlayState : IGameState
    {
        private GameStateType _nextState;

        public PlayState()
        {
            _nextState = GameStateType.Play;
        }

        public void Enter()
        {
            Debug.Log("Play");
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

        public void RequestSetPiece(string setPieceType)
        {
            Debug.Log($"Set piece requested: {setPieceType}");
            _nextState = GameStateType.SetPiece;
        }

        public void RequestHalftime()
        {
            _nextState = GameStateType.Halftime;
        }
    }
}

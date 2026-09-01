namespace SoccerGame.Core
{
    public interface IGameState
    {
        void Enter();
        void UpdateState();
        void Exit();
        GameStateType NextState();
    }
}

using System;

namespace SoccerGame.Core
{
    public static class StateFactory
    {
        public static IGameState CreateState(GameStateType stateType)
        {
            return stateType switch
            {
                GameStateType.Boot => new BootState(),
                GameStateType.Menu => new MenuState(),
                GameStateType.Kickoff => new KickoffState(),
                GameStateType.Play => new PlayState(),
                GameStateType.SetPiece => new SetPieceState(),
                GameStateType.Halftime => new HalftimeState(),
                GameStateType.Fulltime => new FulltimeState(),
                _ => throw new ArgumentOutOfRangeException(nameof(stateType), stateType, "Unsupported game state type.")
            };
        }
    }
}

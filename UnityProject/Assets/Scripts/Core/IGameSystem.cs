namespace SoccerGame.Core
{
    public interface IGameSystem
    {
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
    }
}

using System;

namespace SoccerGame.Core
{
    public static class GameEvents
    {
        public static event Action<int, int> OnScoreChanged;
        public static event Action<float> OnMatchTimeChanged;
        public static event Action<string> OnPossessionChanged;
        public static event Action<string> OnFoulCommitted;
        public static event Action<string, string> OnCardShown;
        public static event Action<int> OnPlayerSwitched;
        public static event Action<string> OnPlayerAction;
        public static event Action OnKickoff;
        public static event Action OnHalftime;
        public static event Action OnFulltime;
        public static event Action<string> OnSetPiece;
        public static event Action<string> OnGoalScored;

        public static void SubscribeScoreChanged(Action<int, int> handler) => OnScoreChanged += handler;
        public static void SubscribeMatchTimeChanged(Action<float> handler) => OnMatchTimeChanged += handler;
        public static void SubscribePossessionChanged(Action<string> handler) => OnPossessionChanged += handler;
        public static void SubscribeFoulCommitted(Action<string> handler) => OnFoulCommitted += handler;
        public static void SubscribeCardShown(Action<string, string> handler) => OnCardShown += handler;
        public static void SubscribePlayerSwitched(Action<int> handler) => OnPlayerSwitched += handler;
        public static void SubscribePlayerAction(Action<string> handler) => OnPlayerAction += handler;
        public static void SubscribeKickoff(Action handler) => OnKickoff += handler;
        public static void SubscribeHalftime(Action handler) => OnHalftime += handler;
        public static void SubscribeFulltime(Action handler) => OnFulltime += handler;
        public static void SubscribeSetPiece(Action<string> handler) => OnSetPiece += handler;
        public static void SubscribeGoalScored(Action<string> handler) => OnGoalScored += handler;

        public static void UnsubscribeScoreChanged(Action<int, int> handler) => OnScoreChanged -= handler;
        public static void UnsubscribeMatchTimeChanged(Action<float> handler) => OnMatchTimeChanged -= handler;
        public static void UnsubscribePossessionChanged(Action<string> handler) => OnPossessionChanged -= handler;
        public static void UnsubscribeFoulCommitted(Action<string> handler) => OnFoulCommitted -= handler;
        public static void UnsubscribeCardShown(Action<string, string> handler) => OnCardShown -= handler;
        public static void UnsubscribePlayerSwitched(Action<int> handler) => OnPlayerSwitched -= handler;
        public static void UnsubscribePlayerAction(Action<string> handler) => OnPlayerAction -= handler;
        public static void UnsubscribeKickoff(Action handler) => OnKickoff -= handler;
        public static void UnsubscribeHalftime(Action handler) => OnHalftime -= handler;
        public static void UnsubscribeFulltime(Action handler) => OnFulltime -= handler;
        public static void UnsubscribeSetPiece(Action<string> handler) => OnSetPiece -= handler;
        public static void UnsubscribeGoalScored(Action<string> handler) => OnGoalScored -= handler;

        public static void RaiseScoreChanged(int homeScore, int awayScore) => OnScoreChanged?.Invoke(homeScore, awayScore);
        public static void RaiseMatchTimeChanged(float matchTime) => OnMatchTimeChanged?.Invoke(matchTime);
        public static void RaisePossessionChanged(string teamName) => OnPossessionChanged?.Invoke(teamName);
        public static void RaiseFoulCommitted(string playerName) => OnFoulCommitted?.Invoke(playerName);
        public static void RaiseCardShown(string playerName, string cardType) => OnCardShown?.Invoke(playerName, cardType);
        public static void RaisePlayerSwitched(int playerIndex) => OnPlayerSwitched?.Invoke(playerIndex);
        public static void RaisePlayerAction(string actionName) => OnPlayerAction?.Invoke(actionName);
        public static void RaiseKickoff() => OnKickoff?.Invoke();
        public static void RaiseHalftime() => OnHalftime?.Invoke();
        public static void RaiseFulltime() => OnFulltime?.Invoke();
        public static void RaiseSetPiece(string setPieceType) => OnSetPiece?.Invoke(setPieceType);
        public static void RaiseGoalScored(string playerName) => OnGoalScored?.Invoke(playerName);

        public static void ClearAll()
        {
            OnScoreChanged = null;
            OnMatchTimeChanged = null;
            OnPossessionChanged = null;
            OnFoulCommitted = null;
            OnCardShown = null;
            OnPlayerSwitched = null;
            OnPlayerAction = null;
            OnKickoff = null;
            OnHalftime = null;
            OnFulltime = null;
            OnSetPiece = null;
            OnGoalScored = null;
        }
    }
}

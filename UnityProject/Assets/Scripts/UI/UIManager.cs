using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SoccerGame.Core;
using SoccerGame.Player;

namespace SoccerGame.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private ScoreboardUI scoreboard;
        [SerializeField] private RadarUI radar;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private TMP_Text messageText;

        private Coroutine _messageRoutine;

        private void Awake()
        {
            if (mainCanvas == null)
                mainCanvas = GetComponentInParent<Canvas>();
        }

        private void OnEnable()
        {
            GameEvents.SubscribeScoreChanged(HandleScoreChanged);
            GameEvents.SubscribeMatchTimeChanged(HandleClockUpdated);
            GameEvents.SubscribeGoalScored(HandleGoalScored);
        }

        private void OnDisable()
        {
            GameEvents.UnsubscribeScoreChanged(HandleScoreChanged);
            GameEvents.UnsubscribeMatchTimeChanged(HandleClockUpdated);
            GameEvents.UnsubscribeGoalScored(HandleGoalScored);
        }

        public void ShowScoreboard(bool show)
        {
            if (scoreboard != null)
                scoreboard.gameObject.SetActive(show);
        }

        public void UpdateScore(int home, int away)
        {
            if (scoreboard != null)
                scoreboard.UpdateScore(home, away);
        }

        public void UpdateClock(float time)
        {
            if (scoreboard != null)
                scoreboard.UpdateClock(time);
        }

        public void UpdateRadar(List<PlayerEntity> players, Vector3 ballPos)
        {
            if (radar != null)
                radar.UpdateRadar(players, ballPos);
        }

        public void ShowMessage(string text, float duration)
        {
            if (messageText == null) return;

            if (_messageRoutine != null)
                StopCoroutine(_messageRoutine);

            _messageRoutine = StartCoroutine(MessageRoutine(text, duration));
        }

        public void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }

        public void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }

        private IEnumerator MessageRoutine(string text, float duration)
        {
            messageText.text = text;
            messageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            messageText.gameObject.SetActive(false);
            _messageRoutine = null;
        }

        private void HandleScoreChanged(int home, int away)
        {
            UpdateScore(home, away);
        }

        private void HandleClockUpdated(float time)
        {
            UpdateClock(time);
        }

        private void HandleGoalScored(string team)
        {
            ShowMessage("GOAL!", 3f);
        }
    }
}

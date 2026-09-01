using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SoccerGame.UI
{
    public class ScoreboardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text homeScoreText;
        [SerializeField] private TMP_Text awayScoreText;
        [SerializeField] private TMP_Text clockText;
        [SerializeField] private Image possessionBar;

        public void UpdateScore(int h, int a)
        {
            if (homeScoreText != null)
                homeScoreText.text = h.ToString();

            if (awayScoreText != null)
                awayScoreText.text = a.ToString();
        }

        public void UpdateClock(float t)
        {
            if (clockText == null) return;

            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            clockText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public void UpdatePossession(float homePct)
        {
            if (possessionBar != null)
                possessionBar.fillAmount = Mathf.Clamp01(homePct);
        }
    }
}

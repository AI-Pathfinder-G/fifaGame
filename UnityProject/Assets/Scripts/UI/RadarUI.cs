using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SoccerGame.Core;
using SoccerGame.Player;

namespace SoccerGame.UI
{
    public class RadarUI : MonoBehaviour
    {
        [SerializeField] private RectTransform radarPanel;
        [SerializeField] private float radarScale = 0.01f;
        [SerializeField] private Color homeColor = new Color(0.2f, 0.5f, 1f);
        [SerializeField] private Color awayColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private Color ballColor = Color.white;
        [SerializeField] private float playerDotSize = 8f;
        [SerializeField] private float ballDotSize = 6f;

        private readonly List<RectTransform> _playerDots = new List<RectTransform>();
        private RectTransform _ballDot;

        public void UpdateRadar(List<PlayerEntity> allPlayers, Vector3 ballPos)
        {
            if (radarPanel == null || allPlayers == null) return;

            EnsurePlayerDots(allPlayers.Count);

            for (int i = 0; i < _playerDots.Count; i++)
            {
                bool active = i < allPlayers.Count;
                if (_playerDots[i].gameObject.activeSelf != active)
                    _playerDots[i].gameObject.SetActive(active);

                if (!active) continue;

                // Convert world position (X/Z plane) to radar UI position
                _playerDots[i].anchoredPosition = WorldToRadar(allPlayers[i].transform.position);

                Image img = _playerDots[i].GetComponent<Image>();
                if (img != null)
                    img.color = allPlayers[i].Team == TeamSide.Home ? homeColor : awayColor;
            }

            if (_ballDot == null)
                _ballDot = CreateDot(ballColor, ballDotSize);

            _ballDot.anchoredPosition = WorldToRadar(ballPos);
        }

        private Vector2 WorldToRadar(Vector3 worldPos)
        {
            Vector2 panelSize = radarPanel.rect.size;
            return new Vector2(
                worldPos.x * radarScale * panelSize.x,
                worldPos.z * radarScale * panelSize.y
            );
        }

        private void EnsurePlayerDots(int count)
        {
            while (_playerDots.Count < count)
                _playerDots.Add(CreateDot(homeColor, playerDotSize));
        }

        private RectTransform CreateDot(Color color, float size)
        {
            GameObject go = new GameObject("RadarDot", typeof(RectTransform), typeof(Image));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(radarPanel, false);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            Image img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            return rt;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace SoccerGame.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject teamSelectPanel;
        [SerializeField] private GameObject settingsPanel;

        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button backButton;

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayMatch);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
            if (backButton != null) backButton.onClick.AddListener(OnBack);

            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayMatch);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettings);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuit);
            if (backButton != null) backButton.onClick.RemoveListener(OnBack);
        }

        public void OnPlayMatch()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (teamSelectPanel != null) teamSelectPanel.SetActive(true);
        }

        public void OnSettings()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (teamSelectPanel != null) teamSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void OnBack()
        {
            ShowMainMenu();
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (teamSelectPanel != null) teamSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
    }
}

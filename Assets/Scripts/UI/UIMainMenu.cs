using UnityEngine;

namespace GameJam
{
    public class UIMainMenu : Window
    {
        [Header("References")]
        [SerializeField] private Window settingsWindow;

        private LevelController levelController;

        private void Awake()
        {
            levelController = FindObjectOfType<LevelController>();
        }

        private void Start()
        {
            levelController.UnpauseGame();
        }

        public void Play()
        {
            ScenesManager.Instance.LoadNextScene();
        }

        public void Settings()
        {
            settingsWindow.Show();
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}
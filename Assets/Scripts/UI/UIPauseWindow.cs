using UnityEngine;

namespace GameJam
{
    public class UIPauseWindow : Window
    {
        [Header("References")]
        [SerializeField] private Window settingsWindow;

        private LevelController levelController;

        private void Awake()
        {
            levelController = FindObjectOfType<LevelController>();

            if (IsOpen) Hide();
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (!IsOpen) Show();
                    else Hide();
                }
            }
            else
            {
                if (IsOpen) Hide();
            }
        }

        public void Resume()
        {
            Hide();
        }

        public void Settings()
        {
            settingsWindow.Show();
        }

        public void MainMenu()
        {
            ScenesManager.Instance.GoToMainMenu();
        }
    }
}
using TMPro;
using UnityEngine;

namespace GameJam
{
    public class UIGameWin : Window
    {
        [Header("UI References")]
        public TMP_Text gameTimerText;

        private LevelController levelController;

        private void Awake()
        {
            levelController = FindObjectOfType<LevelController>();
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null) { return; }

            if (levelController.IsGameWon)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void UpdateGameTimerText(float timer)
        {
            gameTimerText.text = Utils.PrettySeconds(timer);
        }

        //TODO Resurrect
        // gives xp to increase our level, but max health reduced?

        public void TryAgain()
        {
            ScenesManager.Instance.ReloadScene();
        }

        public void MainMenu()
        {
            ScenesManager.Instance.GoToMainMenu();
        }
    }
}
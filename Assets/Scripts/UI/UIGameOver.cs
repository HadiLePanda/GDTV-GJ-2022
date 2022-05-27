using TMPro;
using UnityEngine;

namespace GameJam
{
    public class UIGameOver : Window
    {
        [Header("UI References")]
        public TMP_Text deathScoreText;

        private LevelController levelController;

        private void Awake()
        {
            levelController = FindObjectOfType<LevelController>();
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null) { return; }

            if (!player.IsAlive)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        //private void UpdateDeathsDisplay()
        //{
        //    deathScoreText.text = displayCoins.ToString("N0");
        //}

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
using System;
using UnityEngine;

namespace GameJam
{
    public class LevelController : SingletonMonoBehaviour<LevelController>
    {
        [Header("References")]
        [SerializeField] private UIController uiController;

        [Header("Settings")]
        public bool isMainMenu;

        [Header("Audio")]
        public AudioClip levelMusic;
        public AudioClip levelAmbient;

        [Header("Gameplay Sounds")]
        public AudioClip gameOverSound;

        private bool gameOver = false;

        public bool IsGameOver => !player.IsAlive; //gameOver;

        private Player player;

        public static Action OnGameplayStart, OnGameOver;

        private void OnEnable()
        {
            if (Player.localPlayer == null) { return; }
            Player.localPlayer.Health.OnEmpty += GameOver;
        }
        private void OnDisable()
        {
            if (Player.localPlayer == null) { return; }
            Player.localPlayer.Health.OnEmpty -= GameOver;
        }

        private void Start()
        {
            player = Player.localPlayer;

            if (isMainMenu)
            {
                StartLevelBackgroundAudio();
            }
            else
            {
                StartGameplay();
            }
        }

        // gameplay =======================================
        // this is what starts the level gameplay
        public void StartGameplay()
        {
            // make sure to unpause game in case we're loading and we were paused
            UnpauseGame();

            // fade in
            Game.Fader.FadeOut(true);
            Game.Fader.FadeIn();

            // trigger audio
            StartLevelBackgroundAudio();

            OnGameplayStart?.Invoke();
        }

        public void StopGameplay()
        {
            // it's important to stop coroutines to make sure processes like spawners get aborted
            StopAllCoroutines();
        }

        private void StartLevelBackgroundAudio()
        {
            Game.Audio.PlayBackgroundAudio(levelMusic, levelAmbient);
        }

        // game over ======================================
        public void GameOver()
        {
            Player player = Player.localPlayer;

            gameOver = true;
            OnGameOver?.Invoke();

            StopGameplay();

            // show game over ui
            uiController.gameOverWindow.Show();

            // effects
            if (gameOverSound != null)
                AudioManager.Instance.PlaySfx(gameOverSound);
        }

        // pausing ==========================================
        public void PauseGame()
        {
            if (Time.timeScale != 0)
                Time.timeScale = 0f;
        }
        public void UnpauseGame()
        {
            if (Time.timeScale != 1)
                Time.timeScale = 1f;
        }

        // app focus pause ===================================
        void OnApplicationFocus(bool hasFocus)
        {
            if (isMainMenu)
            {
                if (hasFocus) { UnpauseGame(); }

                return;
            }

            if (!hasFocus)
            {
                if (IsGameOver) { return; }

                PauseGame();
            }
            else
            {
                UnpauseGame();
            }
        }
    }
}
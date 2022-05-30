using System;
using System.Linq;
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
        public AudioClip gameWinSound;
        public AudioClip gameWinAmbient;
        public AudioClip gameOverAmbient;
        public AudioClip bossAmbient;

        private bool gameOver = false;
        private bool gameWon = false;

        public bool IsGameOver => !player.IsAlive; //gameOver;
        public bool IsGameWon => gameWon;

        private Player player;

        [HideInInspector] public float gameplayStartTime;
        [HideInInspector] public float gameplayEndTime;

        public static Action OnGameplayStart, OnGameOver;

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

            gameplayStartTime = Time.time;

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

            gameplayEndTime = Time.time;
        }

        private void StartLevelBackgroundAudio()
        {
            Game.Audio.PlayBackgroundAudio(levelMusic, levelAmbient);
        }

        public void PlayBossBackgroundAudio()
        {
            if (bossAmbient != null)
            {
                // change background audio to boss theme
                Game.Audio.StopBackgroundAudio();

                Game.Audio.PlayBackgroundAudio(null, bossAmbient);
            }
        }

        // game over/win ======================================
        public void GameOver()
        {
            gameOver = true;
            OnGameOver?.Invoke();

            StopGameplay();

            // change background audio to crows loop ambient
            Game.Audio.StopBackgroundAudio();
            if (gameOverAmbient != null)
            {
                Game.Audio.PlayBackgroundAudio(null, gameOverAmbient);
            }

            // show game over ui
            uiController.gameOverWindow.Show();

            // effects
            if (gameOverSound != null)
                Game.Audio.PlaySfx(gameOverSound);
        }
        public void GameWin()
        {
            gameWon = true;

            StopGameplay();
            KillAllEntities();

            // change background audio to winning
            Game.Audio.StopBackgroundAudio();
            if (gameWinAmbient != null)
            {
                Game.Audio.PlayAmbient(gameWinAmbient);
            }

            // show game win ui
            uiController.gameWinWindow.Show();
            uiController.gameWinWindow.UpdateGameTimerText(gameplayEndTime - gameplayStartTime);

            // effects
            if (gameWinSound != null)
                Game.Audio.PlaySfx(gameWinSound);
        }

        private void KillAllEntities()
        {
            Entity[] entities = FindObjectsOfType<Entity>()
                .Where(x => x.IsFactionAlly(player) == false)
                .Where(x => x.IsAlive)
                .ToArray();

            for (int i = entities.Length - 1; i >= 0; i--)
            {
                Destroy(entities[i].gameObject);
                //entities[i].Health.Deplete();
            }
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
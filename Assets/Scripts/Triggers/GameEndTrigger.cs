using UnityEngine;

namespace GameJam
{
    public class GameEndTrigger : MonoBehaviour
    {
        [Header("References")]
        public Entity finalBoss;
        public LevelController levelController;

        private void OnEnable()
        {
            finalBoss.Health.OnEmpty += HandleFinalBossDied;
        }
        private void OnDisable()
        {
            finalBoss.Health.OnEmpty -= HandleFinalBossDied;
        }

        private void HandleFinalBossDied()
        {
            levelController.GameWin();
        }
    }
}
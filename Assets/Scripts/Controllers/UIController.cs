using UnityEngine;

namespace GameJam
{
    public class UIController : SingletonMonoBehaviour<UIController>
    {
        [Header("Windows")]
        public UIGameOver gameOverWindow;
        public UIGameWin gameWinWindow;
        public UIPauseWindow pauseWindow;
        public UISettings settingsWindow;
    }
}
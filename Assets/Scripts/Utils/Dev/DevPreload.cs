using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam
{
    /// <summary>
    /// This script is used to help loading the preload scene automatically from any scene.
    /// It helps making this easier when developing the game by not having to manually go to the preload scene.
    /// Note: should be first in the script excecution order (but make sure to remove for the final build)
    /// </summary>
    public class DevPreload : MonoBehaviour
    {
        private void Awake()
        {
            GameObject check = FindObjectOfType<DDOL>()?.gameObject;

            if (check == null)
                SceneManager.LoadScene(0);
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam
{
    public class ScenesManager : SingletonMonoBehaviour<ScenesManager>
    {
        [Header("Scenes")]
        [SerializeField] private int mainMenuSceneIndex = 1;

        public void LoadScene(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        //TODO Async scenes for loading screen
        public void LoadSceneAsync(int sceneIndex)
        {
            SceneManager.LoadSceneAsync(sceneIndex);
        }

        public void LoadNextScene()
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            LoadScene(nextSceneIndex);
        }

        public void ReloadScene()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            LoadScene(currentSceneIndex);
        }

        public void GoToMainMenu()
        {
            LoadScene(mainMenuSceneIndex);
        }
    }
}
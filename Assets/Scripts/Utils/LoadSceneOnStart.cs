using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam
{
    public class LoadSceneOnStart : MonoBehaviour
    {
        [Header("Load Settings")]
        [SerializeField] private int sceneIndex;

        private void Start()
        {
            if (SceneManager.GetSceneByBuildIndex(sceneIndex) != null)
            {
                ScenesManager.Instance.LoadScene(sceneIndex);
            }
        }
    }
}
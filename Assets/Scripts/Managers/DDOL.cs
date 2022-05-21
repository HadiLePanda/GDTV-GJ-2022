using UnityEngine;

namespace GameJam
{
    public class DDOL : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}